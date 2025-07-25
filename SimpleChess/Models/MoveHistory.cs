using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleChess.Models
{
    public class MoveHistory : INotifyPropertyChanged
    {
        private ObservableCollection<NotationRecord> _moves = new ObservableCollection<NotationRecord>();
        private NotationRecord _currentMove;
        private PlayerColor _lastMoveColor = PlayerColor.Black; // Start with Black so White is first

        public ObservableCollection<NotationRecord> Moves
        {
            get => _moves;
            set
            {
                _moves = value;
                OnPropertyChanged();
            }
        }

        public void AddMove(Piece piece, Position from, Position to, bool isCapture = false, bool isCheck = false, 
            bool isCheckmate = false, bool isPromotion = false, PieceType? promotionType = null, bool isCastling = false)
        {
            // Safety check
            if (piece == null || from == null || to == null)
            {
                return; // Don't record invalid moves
            }
            
            string moveText = GenerateMoveNotation(piece, from, to, isCapture, isCheck, isCheckmate, isPromotion, promotionType, isCastling);

            // Use the piece color to determine if it's White or Black's move
            if (piece.Color == PlayerColor.White)
            {
                // It's a White move - start a new record
                _currentMove = new NotationRecord
                {
                    MoveNumber = _moves.Count + 1,
                    WhiteMove = moveText
                };
                _moves.Add(_currentMove);
                _lastMoveColor = PlayerColor.White;
                
                // Also notify that moves collection has changed
                OnPropertyChanged(nameof(Moves));
            }
            else // Black's move
            {
                // Add Black's move to the current record
                if (_currentMove != null && string.IsNullOrEmpty(_currentMove.BlackMove))
                {
                    // Add to the existing move if white has already moved and black slot is empty
                    _currentMove.BlackMove = moveText;
                    System.Diagnostics.Debug.WriteLine($"Added Black move to existing record: {moveText}");
                }
                else
                {
                    // If there's no current move or the black move slot is already filled,
                    // create a new record with just the black move
                    _currentMove = new NotationRecord
                    {
                        MoveNumber = _moves.Count + 1,
                        WhiteMove = "",  // No white move yet
                        BlackMove = moveText
                    };
                    _moves.Add(_currentMove);
                    System.Diagnostics.Debug.WriteLine($"Created new record for Black move: {moveText}");
                }
                _lastMoveColor = PlayerColor.Black;

                // Raise property changed for the Moves collection
                OnPropertyChanged(nameof(Moves));
            }
        }

        private string GenerateMoveNotation(Piece piece, Position from, Position to, bool isCapture, bool isCheck, 
            bool isCheckmate, bool isPromotion, PieceType? promotionType, bool isCastling)
        {
            // Handle special case for castling
            if (isCastling)
            {
                // Kingside castling
                if (to.Column > from.Column)
                    return "O-O";
                // Queenside castling
                else
                    return "O-O-O";
            }

            // Safety check - if we don't have valid piece information, return a placeholder
            if (piece == null)
            {
                return "?";
            }

            string notation = "";

            // Add piece letter (except for pawns)
            if (piece.Type != PieceType.Pawn)
            {
                notation += GetPieceLetterNotation(piece.Type);
            }

            // For captures by pawns, add the file
            if (isCapture && piece != null && piece.Type == PieceType.Pawn && from != null)
            {
                notation += (char)('a' + from.Column);
            }

            // Add capture symbol
            if (isCapture)
            {
                notation += "x";
            }

            // Add destination square
            if (to != null)
            {
                notation += (char)('a' + to.Column);
                notation += (8 - to.Row).ToString();
            }
            else
            {
                notation += "??";  // Unknown destination
            }

            // Add promotion piece
            if (isPromotion && promotionType.HasValue)
            {
                notation += "=" + GetPieceLetterNotation(promotionType.Value);
            }

            // Add check or checkmate symbol
            if (isCheckmate)
            {
                notation += "#";
            }
            else if (isCheck)
            {
                notation += "+";
            }

            return notation;
        }

        private string GetPieceLetterNotation(PieceType type)
        {
            switch (type)
            {
                case PieceType.King: return "K";
                case PieceType.Queen: return "Q";
                case PieceType.Rook: return "R";
                case PieceType.Bishop: return "B";
                case PieceType.Knight: return "N";
                case PieceType.Pawn: return "";
                default: return "";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            
            // Add diagnostics for debugging
            if (propertyName == nameof(Moves))
            {
                System.Diagnostics.Debug.WriteLine($"MoveHistory updated. Current moves count: {_moves.Count}");
                foreach (var move in _moves)
                {
                    System.Diagnostics.Debug.WriteLine($"Move {move.MoveNumber}: White: {move.WhiteMove ?? "(none)"}, Black: {move.BlackMove ?? "(none)"}");
                }
            }
        }
    }
}
