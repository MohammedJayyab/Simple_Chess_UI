using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using SimpleChess.Commands;
using SimpleChess.Models;

namespace SimpleChess.ViewModels
{
    public partial class MainViewModel : INotifyPropertyChanged
    {
        private Square _selectedSquare;
        private PlayerColor _currentPlayer = PlayerColor.White;
        private string _statusMessage = "White's turn to move";
        private GameState _gameState = GameState.InProgress;
        private bool _isKingInCheck = false;
        private string _moveInput;
        private MoveHistory _moveHistory = new MoveHistory();

        public ObservableCollection<Square> Squares { get; private set; }
        public ICommand SquareClickCommand { get; private set; }
        public ICommand PlayTextMoveCommand { get; private set; }
        public ICommand CopyMovesCommand { get; private set; }

        public string MoveInput
        {
            get => _moveInput;
            set
            {
                if (_moveInput != value)
                {
                    _moveInput = value;
                    OnPropertyChanged();
                }
            }
        }

        public MoveHistory MoveHistory 
        { 
            get => _moveHistory; 
            private set 
            {
                _moveHistory = value;
                OnPropertyChanged();
            }
        }

        public GameState GameState
        {
            get => _gameState;
            private set
            {
                if (_gameState != value)
                {
                    _gameState = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainViewModel()
        {
            InitializeBoard();
            SquareClickCommand = new RelayCommand(OnSquareClick);
            PlayTextMoveCommand = new RelayCommand(_ => OnPlayTextMove());
            CopyMovesCommand = new RelayCommand(_ => CopyMoves());
            InitializeCommands();
        }

        private void InitializeBoard()
        {
            Squares = new ObservableCollection<Square>();

            // Create all squares on the board
            for (int row = 0; row < 8; row++)
            {
                for (int col = 0; col < 8; col++)
                {
                    Squares.Add(new Square(row, col));
                }
            }

            // Set up the initial pieces
            SetupInitialPieces();
        }

        private void SetupInitialPieces()
        {
            // Set up white pieces
            SetupPiecesForColor(PlayerColor.White);

            // Set up black pieces
            SetupPiecesForColor(PlayerColor.Black);
        }

        private void SetupPiecesForColor(PlayerColor color)
        {
            int pawnRow = (color == PlayerColor.White) ? 6 : 1;
            int pieceRow = (color == PlayerColor.White) ? 7 : 0;

            // Pawns
            for (int col = 0; col < 8; col++)
            {
                GetSquareAt(pawnRow, col).Piece = new Piece(PieceType.Pawn, color, new Position(pawnRow, col));
            }

            // Rooks
            GetSquareAt(pieceRow, 0).Piece = new Piece(PieceType.Rook, color, new Position(pieceRow, 0));
            GetSquareAt(pieceRow, 7).Piece = new Piece(PieceType.Rook, color, new Position(pieceRow, 7));

            // Knights
            GetSquareAt(pieceRow, 1).Piece = new Piece(PieceType.Knight, color, new Position(pieceRow, 1));
            GetSquareAt(pieceRow, 6).Piece = new Piece(PieceType.Knight, color, new Position(pieceRow, 6));

            // Bishops
            GetSquareAt(pieceRow, 2).Piece = new Piece(PieceType.Bishop, color, new Position(pieceRow, 2));
            GetSquareAt(pieceRow, 5).Piece = new Piece(PieceType.Bishop, color, new Position(pieceRow, 5));

            // Queen
            GetSquareAt(pieceRow, 3).Piece = new Piece(PieceType.Queen, color, new Position(pieceRow, 3));

            // King
            GetSquareAt(pieceRow, 4).Piece = new Piece(PieceType.King, color, new Position(pieceRow, 4));
        }

        private Square GetSquareAt(int row, int col)
        {
            return Squares.FirstOrDefault(s => s.Position.Row == row && s.Position.Column == col);
        }

        private void OnSquareClick(object parameter)
        {
            if (parameter is not Square clickedSquare)
                return;

            // Don't allow moves if the game is over
            if (GameState == GameState.Checkmate || GameState == GameState.Stalemate || GameState == GameState.Draw)
                return;

            if (_selectedSquare == null)
            {
                // No square is currently selected
                if (clickedSquare.Piece != null && clickedSquare.Piece.Color == _currentPlayer)
                {
                    SelectSquare(clickedSquare);
                    HighlightValidMoves(clickedSquare);
                }
            }
            else if (_selectedSquare == clickedSquare)
            {
                // The same square is clicked again - deselect it
                ClearSelectionAndHighlights();
            }
            else if (clickedSquare.IsValidMove)
            {
                Square fromSquare = _selectedSquare;
                Square toSquare = clickedSquare;
                
                ExecuteMoveSequence(fromSquare, toSquare, string.Empty);
            }
            else if (clickedSquare.Piece != null && clickedSquare.Piece.Color == _currentPlayer)
            {
                // Another piece of the same color is selected
                ClearSelectionAndHighlights();
                SelectSquare(clickedSquare);
                HighlightValidMoves(clickedSquare);
            }
            else
            {
                // Invalid move, just clear the selection
                if (_selectedSquare != null && !clickedSquare.IsValidMove)
                {
                    System.Media.SystemSounds.Hand.Play();
                }
                ClearSelectionAndHighlights();
            }
        }

        private void SelectSquare(Square square)
        {
            _selectedSquare = square;
            square.IsSelected = true;
        }

        private void HighlightValidMoves(Square square)
        {
            if (square.Piece == null)
                return;

            foreach (var targetSquare in Squares)
            {
                if (IsValidMove(square, targetSquare))
                {
                    // Check if this move would leave our king in check
                    if (!WouldMoveResultInCheck(square, targetSquare))
                    {
                        targetSquare.IsValidMove = true;
                    }
                }
            }
        }

        private void ClearSelectionAndHighlights()
        {
            if (_selectedSquare != null)
            {
                _selectedSquare.IsSelected = false;
                _selectedSquare = null;
            }

            foreach (var square in Squares)
            {
                square.IsValidMove = false;
            }
        }

        private (Piece piece, Position from, Position to, bool isCapture, bool isPromotion, PieceType? promotionType, bool isCastling) MovePiece(Square fromSquare, Square toSquare)
        {
            // Record if it's the piece's first move
            bool isFirstMove = !fromSquare.Piece.HasMoved;

            // Special handling for castling
            if (fromSquare.Piece.Type == PieceType.King && Math.Abs(toSquare.Position.Column - fromSquare.Position.Column) == 2)
            {
                // Determine if it's kingside or queenside castling
                bool isKingside = toSquare.Position.Column > fromSquare.Position.Column;
                int rookFromCol = isKingside ? 7 : 0;
                int rookToCol = isKingside ? 5 : 3;

                // Get rook squares
                Square rookFromSquare = GetSquareAt(fromSquare.Position.Row, rookFromCol);
                Square rookToSquare = GetSquareAt(fromSquare.Position.Row, rookToCol);

                // Move the rook
                rookToSquare.Piece = rookFromSquare.Piece;
                rookFromSquare.Piece = null;

                // Update rook position and HasMoved flag
                rookToSquare.Piece.Position = rookToSquare.Position;
                rookToSquare.Piece.HasMoved = true;
            }

            // If capturing a piece, it's removed from the board
            if (toSquare.Piece != null)
            {
                // Could add captured piece to a list here if tracking is needed
            }

            // Record ALL move information before making any changes
            bool isCapture = toSquare.Piece != null;
            bool isCastling = fromSquare.Piece != null && 
                             fromSquare.Piece.Type == PieceType.King && 
                             Math.Abs(toSquare.Position.Column - fromSquare.Position.Column) == 2;

            // Store a reference to the moving piece for notation purposes
            Piece movingPiece = fromSquare.Piece;
            PieceType originalPieceType = movingPiece?.Type ?? PieceType.Pawn;

            // Move the piece
            toSquare.Piece = fromSquare.Piece;
            fromSquare.Piece = null;

            // Update piece position and HasMoved flag (if piece exists)
            if (toSquare.Piece != null)
            {
                toSquare.Piece.Position = toSquare.Position;
                toSquare.Piece.HasMoved = true;
            }
            PieceType? promotionType = null;

            // Handle pawn promotion
            bool willPromote = toSquare.Piece != null && 
                         toSquare.Piece.Type == PieceType.Pawn && 
                         (toSquare.Position.Row == 0 || toSquare.Position.Row == 7);

            if (willPromote)
            {
                // Store pawn info
                PlayerColor pawnColor = toSquare.Piece.Color;
                Position pawnPosition = toSquare.Position;

                // Get promotion type from dialog
                var promotionDialog = new Views.PawnPromotionDialog(pawnColor);
                promotionDialog.Owner = System.Windows.Application.Current.MainWindow;

                if (promotionDialog.ShowDialog() == true)
                {
                    promotionType = promotionDialog.SelectedPieceType;
                }
                else
                {
                    promotionType = PieceType.Queen; // Default
                }

                // Replace pawn with the promoted piece
                toSquare.Piece = new Piece(promotionType.Value, pawnColor, pawnPosition)
                {
                    HasMoved = true
                };
            }

            // Return all the information needed to record this move
            return (movingPiece, fromSquare.Position, toSquare.Position, isCapture, promotionType.HasValue, promotionType, isCastling);
        }

        private void PromotePawn(Square pawnSquare)
        {
            // Store the pawn's color before creating a new piece
            PlayerColor pawnColor = pawnSquare.Piece.Color;
            Position pawnPosition = pawnSquare.Position;

            // Create and show the promotion dialog
            var promotionDialog = new Views.PawnPromotionDialog(pawnColor);
            promotionDialog.Owner = System.Windows.Application.Current.MainWindow;

            // Default to Queen
            PieceType promotedType = PieceType.Queen;

            if (promotionDialog.ShowDialog() == true)
            {
                promotedType = promotionDialog.SelectedPieceType;
            }

            // Replace the pawn with the selected piece
            pawnSquare.Piece = new Piece(promotedType, pawnColor, pawnPosition)
            {
                HasMoved = true
            };
        }

        private void SwitchPlayer()
        {
            _currentPlayer = (_currentPlayer == PlayerColor.White) ? PlayerColor.Black : PlayerColor.White;

            // Status message will be updated by UpdateGameState method
        }

        private bool IsValidMove(Square fromSquare, Square toSquare)
        {
            // Basic validation: can't move to the same square
            if (fromSquare == toSquare)
                return false;

            // Can't capture pieces of the same color
            if (toSquare.Piece != null && toSquare.Piece.Color == fromSquare.Piece.Color)
                return false;

            // Delegate to piece-specific move validation
            return ValidatePieceSpecificMove(fromSquare.Piece, fromSquare.Position, toSquare.Position);
        }

        private bool ValidatePieceSpecificMove(Piece piece, Position from, Position to)
        {
            switch (piece.Type)
            {
                case PieceType.Pawn:
                    return ValidatePawnMove(piece, from, to);

                case PieceType.Knight:
                    return ValidateKnightMove(from, to);

                case PieceType.Bishop:
                    return ValidateBishopMove(from, to);

                case PieceType.Rook:
                    return ValidateRookMove(from, to);

                case PieceType.Queen:
                    return ValidateQueenMove(from, to);

                case PieceType.King:
                    return ValidateKingMove(from, to);

                default:
                    return false;
            }
        }

        private bool ValidatePawnMove(Piece pawn, Position from, Position to)
        {
            int direction = (pawn.Color == PlayerColor.White) ? -1 : 1;
            int rowDiff = to.Row - from.Row;
            int colDiff = Math.Abs(to.Column - from.Column);

            // Forward movement (no capture)
            if (colDiff == 0)
            {
                // Single square forward
                if (rowDiff == direction && GetSquareAt(to.Row, to.Column).Piece == null)
                    return true;

                // Double square forward from starting position
                if (rowDiff == 2 * direction && !pawn.HasMoved)
                {
                    int middleRow = from.Row + direction;
                    return GetSquareAt(middleRow, from.Column).Piece == null &&
                           GetSquareAt(to.Row, to.Column).Piece == null;
                }
            }
            // Diagonal capture
            else if (colDiff == 1 && rowDiff == direction)
            {
                Square targetSquare = GetSquareAt(to.Row, to.Column);
                return targetSquare.Piece != null && targetSquare.Piece.Color != pawn.Color;
            }

            return false;
        }

        private bool ValidateKnightMove(Position from, Position to)
        {
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);

            // Knights move in an L-shape: 2 squares in one direction and 1 square perpendicular
            return (rowDiff == 2 && colDiff == 1) || (rowDiff == 1 && colDiff == 2);
        }

        private bool ValidateBishopMove(Position from, Position to)
        {
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);

            // Bishops move diagonally
            if (rowDiff != colDiff)
                return false;

            // Check if path is clear
            return IsPathClear(from, to);
        }

        private bool ValidateRookMove(Position from, Position to)
        {
            // Rooks move horizontally or vertically
            if (from.Row != to.Row && from.Column != to.Column)
                return false;

            // Check if path is clear
            return IsPathClear(from, to);
        }

        private bool ValidateQueenMove(Position from, Position to)
        {
            // Queens combine rook and bishop movement
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);

            // Either moving like a rook (in a straight line) or like a bishop (diagonally)
            bool isValidDirection = (from.Row == to.Row || from.Column == to.Column) || (rowDiff == colDiff);

            if (!isValidDirection)
                return false;

            // Check if path is clear
            return IsPathClear(from, to);
        }

        private bool ValidateKingMove(Position from, Position to)
        {
            int rowDiff = Math.Abs(to.Row - from.Row);
            int colDiff = Math.Abs(to.Column - from.Column);

            // Standard king move - one square in any direction
            if (rowDiff <= 1 && colDiff <= 1)
                return true;

            // Check for castling
            Square fromSquare = GetSquareAt(from.Row, from.Column);
            if (!fromSquare.Piece.HasMoved && rowDiff == 0 && colDiff == 2)
            {
                // Determine if it's kingside or queenside castling
                bool isKingside = to.Column > from.Column;
                int rookCol = isKingside ? 7 : 0;

                // Get the rook
                Square rookSquare = GetSquareAt(from.Row, rookCol);
                if (rookSquare.Piece == null || 
                    rookSquare.Piece.Type != PieceType.Rook || 
                    rookSquare.Piece.HasMoved)
                {
                    return false;
                }

                // Check if path is clear between king and rook
                int step = isKingside ? 1 : -1;
                for (int col = from.Column + step; col != rookCol; col += step)
                {
                    if (GetSquareAt(from.Row, col).Piece != null)
                        return false;
                }

                // Also verify the square the king moves through isn't under attack
                // (For simplicity, we're skipping the check for squares being under attack)

                return true;
            }

            return false;
        }

        private bool IsPathClear(Position from, Position to)
        {
            int rowStep = 0;
            int colStep = 0;

            // Determine direction
            if (from.Row < to.Row) rowStep = 1;
            else if (from.Row > to.Row) rowStep = -1;

            if (from.Column < to.Column) colStep = 1;
            else if (from.Column > to.Column) colStep = -1;

            int row = from.Row + rowStep;
            int col = from.Column + colStep;

            // Check each square along the path
            while (row != to.Row || col != to.Column)
            {
                if (GetSquareAt(row, col).Piece != null)
                    return false;

                row += rowStep;
                col += colStep;
            }

            return true;
        }
        
        // Generate algebraic notation directly without relying on MoveHistory class
        private string GenerateMoveNotation(PlayerColor pieceColor, PieceType pieceType, Position from, Position to, bool isCapture)
        {
            string notation = "";
            
            // Check for castling (King moves 2 squares horizontally)
            if (pieceType == PieceType.King && Math.Abs(from.Column - to.Column) == 2)
            {
                // Kingside castling (O-O)
                if (to.Column > from.Column)
                {
                    notation = "O-O";
                }
                // Queenside castling (O-O-O)
                else
                {
                    notation = "O-O-O";
                }
                
                // Add check/checkmate symbols if needed
                if (GameState == GameState.Check)
                {
                    notation += "+";
                }
                else if (GameState == GameState.Checkmate)
                {
                    notation += "#";
                }
                
                return notation;
            }
            
            // Standard move notation
            
            // Add piece letter (except for pawns)
            if (pieceType != PieceType.Pawn)
            {
                switch (pieceType)
                {
                    case PieceType.King: notation += "K"; break;
                    case PieceType.Queen: notation += "Q"; break;
                    case PieceType.Rook: notation += "R"; break;
                    case PieceType.Bishop: notation += "B"; break;
                    case PieceType.Knight: notation += "N"; break;
                }
            }
            
            // For pawn captures, add the file
            if (isCapture && pieceType == PieceType.Pawn)
            {
                notation += (char)('a' + from.Column);
            }
            
            // Add capture symbol
            if (isCapture)
            {
                notation += "x";
            }
            
            // Add destination square
            notation += (char)('a' + to.Column);
            notation += (8 - to.Row).ToString();
            
            // Check if it would be check or checkmate
            if (GameState == GameState.Check)
            {
                notation += "+";
            }
            else if (GameState == GameState.Checkmate)
            {
                notation += "#";
            }
            
            return notation;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}