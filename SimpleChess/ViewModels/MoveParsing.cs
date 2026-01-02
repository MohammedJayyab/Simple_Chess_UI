using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using SimpleChess.Models;

namespace SimpleChess.ViewModels
{
    public partial class MainViewModel
    {
        private void OnPlayTextMove()
        {
            if (string.IsNullOrWhiteSpace(MoveInput))
                return;

            string input = MoveInput.Trim();
            if (ParseAndExecuteMove(input))
            {
                MoveInput = string.Empty; // Clear input on success
            }
            else
            {
                // Play error sound for invalid text move
                System.Media.SystemSounds.Hand.Play();
            }
        }

        private void CopyMoves()
        {
            if (_moveHistory == null || _moveHistory.Moves.Count == 0)
            {
                StatusMessage = "No moves to copy.";
                return;
            }

            var sb = new System.Text.StringBuilder();
            foreach (var move in _moveHistory.Moves)
            {
                sb.Append($"{move.MoveNumber}. {move.WhiteMove}");
                if (!string.IsNullOrEmpty(move.BlackMove))
                {
                    sb.Append($" {move.BlackMove} ");
                }
            }

            try
            {
                System.Windows.Clipboard.SetText(sb.ToString().Trim());
                StatusMessage = "Move history copied to clipboard!";
            }
            catch (Exception)
            {
                StatusMessage = "Failed to copy moves to clipboard.";
            }
        }

        private bool ParseAndExecuteMove(string input)
        {
            try
            {
                // Handle Castling
                if (input == "O-O" || input == "0-0")
                {
                    return ExecuteCastling(true);
                }
                if (input == "O-O-O" || input == "0-0-0")
                {
                    return ExecuteCastling(false);
                }

                // Regular Move Regex
                // Groups: 1:Piece, 2:Disambiguation, 3:Capture, 4:Target, 5:Promotion, 6:Check
                var match = Regex.Match(input, @"^([KQRBN])?([a-h]|[1-8]|[a-h][1-8])?(x)?([a-h][1-8])(=[QRBN])?([+#])?$", RegexOptions.IgnoreCase);

                if (!match.Success)
                {
                    StatusMessage = "Invalid move notation format.";
                    return false;
                }

                string pieceChar = match.Groups[1].Value.ToUpper();
                string disambiguation = match.Groups[2].Value.ToLower();
                string targetStr = match.Groups[4].Value.ToLower();
                string promotionStr = match.Groups[5].Value.ToUpper();

                PieceType type = GetPieceTypeFromChar(pieceChar);
                Position targetPos = ParsePosition(targetStr);

                // Find candidate pieces
                var candidates = Squares
                    .Where(s => s.Piece != null && s.Piece.Color == _currentPlayer && s.Piece.Type == type)
                    .ToList();

                var validPieces = new List<Square>();

                foreach (var square in candidates)
                {
                    if (IsValidMove(square, GetSquareAt(targetPos.Row, targetPos.Column)))
                    {
                        // Check if move would result in check
                        if (!WouldMoveResultInCheck(square, GetSquareAt(targetPos.Row, targetPos.Column)))
                        {
                            // Apply disambiguation filters if provided
                            if (!string.IsNullOrEmpty(disambiguation))
                            {
                                if (disambiguation.Length == 1)
                                {
                                    if (char.IsDigit(disambiguation[0])) // Rank
                                    {
                                        int rank = 8 - (disambiguation[0] - '0');
                                        if (square.Position.Row != rank) continue;
                                    }
                                    else // File
                                    {
                                        int file = disambiguation[0] - 'a';
                                        if (square.Position.Column != file) continue;
                                    }
                                }
                                else if (disambiguation.Length == 2) // Full square
                                {
                                    Position p = ParsePosition(disambiguation);
                                    if (square.Position.Row != p.Row || square.Position.Column != p.Column) continue;
                                }
                            }
                            validPieces.Add(square);
                        }
                    }
                }

                if (validPieces.Count == 0)
                {
                    StatusMessage = $"Illegal move: No {type} can reach {targetStr}.";
                    return false;
                }

                if (validPieces.Count > 1)
                {
                    StatusMessage = "Ambiguous move. Please specify file or rank.";
                    return false;
                }

                // Execute the move
                Square fromSquare = validPieces[0];
                Square toSquare = GetSquareAt(targetPos.Row, targetPos.Column);

                // Refactored logic from OnSquareClick
                ExecuteMoveSequence(fromSquare, toSquare, promotionStr);
                return true;
            }
            catch (Exception)
            {
                StatusMessage = "Error parsing move.";
                return false;
            }
        }

        private bool ExecuteCastling(bool kingside)
        {
            int row = (_currentPlayer == PlayerColor.White) ? 7 : 0;
            Square kingSquare = GetSquareAt(row, 4);

            if (kingSquare.Piece == null || kingSquare.Piece.Type != PieceType.King || kingSquare.Piece.Color != _currentPlayer)
                return false;

            int targetCol = kingside ? 6 : 2;
            Square targetSquare = GetSquareAt(row, targetCol);

            if (IsValidMove(kingSquare, targetSquare) && !WouldMoveResultInCheck(kingSquare, targetSquare))
            {
                ExecuteMoveSequence(kingSquare, targetSquare, string.Empty);
                return true;
            }

            StatusMessage = "Illegal castling move.";
            return false;
        }

        private void ExecuteMoveSequence(Square fromSquare, Square toSquare, string promotionStr)
        {
            // Prepare info for notation/record
            PlayerColor movingColor = fromSquare.Piece.Color;
            PieceType movingType = fromSquare.Piece.Type;
            Position fromPos = fromSquare.Position;
            Position toPos = toSquare.Position;
            bool isCapture = toSquare.Piece != null;

            // In case of pawn promotion via text, we need to handle the dialog OR use the provided char
            // For now, if promotionStr is provided, we skip the dialog in MovePiece
            
            // Refactor: We need a way to tell MovePiece which piece to promote to
            // I'll add a helper or pass it through.
            
            // Record the move before switching player
            RecordMove(fromSquare, toSquare);

            // Clear previous last move highlights
            foreach (var square in Squares)
            {
                square.IsLastMove = false;
            }

            // Move the piece
            MovePieceWithPromotion(fromSquare, toSquare, promotionStr);
            ClearSelectionAndHighlights();

            // Set current last move highlights
            fromSquare.IsLastMove = true;
            toSquare.IsLastMove = true;

            // Play move sound
            System.Media.SystemSounds.Asterisk.Play();

            // Switch player and update state
            SwitchPlayer();
            UpdateGameState();

            // Notation
            string notation = GenerateMoveNotation(movingColor, movingType, fromPos, toPos, isCapture);
            UpdateHistory(movingColor, notation);
        }

        private void MovePieceWithPromotion(Square fromSquare, Square toSquare, string promotionStr)
        {
             // Record if it's the piece's first move
            bool isFirstMove = !fromSquare.Piece.HasMoved;

            // Special handling for castling
            if (fromSquare.Piece.Type == PieceType.King && Math.Abs(toSquare.Position.Column - fromSquare.Position.Column) == 2)
            {
                bool isKingside = toSquare.Position.Column > fromSquare.Position.Column;
                int rookFromCol = isKingside ? 7 : 0;
                int rookToCol = isKingside ? 5 : 3;

                Square rookFromSquare = GetSquareAt(fromSquare.Position.Row, rookFromCol);
                Square rookToSquare = GetSquareAt(fromSquare.Position.Row, rookToCol);

                rookToSquare.Piece = rookFromSquare.Piece;
                rookFromSquare.Piece = null;
                rookToSquare.Piece.Position = rookToSquare.Position;
                rookToSquare.Piece.HasMoved = true;
            }

            // Move the piece
            Piece movingPiece = fromSquare.Piece;
            toSquare.Piece = fromSquare.Piece;
            fromSquare.Piece = null;

            if (toSquare.Piece != null)
            {
                toSquare.Piece.Position = toSquare.Position;
                toSquare.Piece.HasMoved = true;
            }

            // Handle pawn promotion
            if (toSquare.Piece != null && 
                toSquare.Piece.Type == PieceType.Pawn && 
                (toSquare.Position.Row == 0 || toSquare.Position.Row == 7))
            {
                PieceType promotionType = PieceType.Queen; // Default
                if (!string.IsNullOrEmpty(promotionStr) && promotionStr.StartsWith("="))
                {
                    promotionType = GetPieceTypeFromChar(promotionStr.Substring(1));
                }
                else
                {
                    // Fallback to dialog if no promotion string in notation
                    var promotionDialog = new Views.PawnPromotionDialog(movingPiece.Color);
                    promotionDialog.Owner = System.Windows.Application.Current.MainWindow;
                    if (promotionDialog.ShowDialog() == true)
                    {
                        promotionType = promotionDialog.SelectedPieceType;
                    }
                }

                toSquare.Piece = new Piece(promotionType, movingPiece.Color, toSquare.Position)
                {
                    HasMoved = true
                };
            }
        }

        private void UpdateHistory(PlayerColor movingColor, string notation)
        {
            var oldMoves = new ObservableCollection<NotationRecord>(_moveHistory.Moves);
            if (movingColor == PlayerColor.White)
            {
                oldMoves.Add(new NotationRecord { MoveNumber = oldMoves.Count + 1, WhiteMove = notation });
            }
            else
            {
                if (oldMoves.Count > 0)
                {
                    var lastMove = oldMoves[oldMoves.Count - 1];
                    lastMove.BlackMove = notation;
                    oldMoves.RemoveAt(oldMoves.Count - 1);
                    oldMoves.Add(lastMove);
                }
            }

            _moveHistory = new MoveHistory();
            foreach (var move in oldMoves) _moveHistory.Moves.Add(move);
            OnPropertyChanged(nameof(MoveHistory));
        }

        private PieceType GetPieceTypeFromChar(string c)
        {
            return c switch
            {
                "K" => PieceType.King,
                "Q" => PieceType.Queen,
                "R" => PieceType.Rook,
                "B" => PieceType.Bishop,
                "N" => PieceType.Knight,
                _ => PieceType.Pawn
            };
        }

        private Position ParsePosition(string pos)
        {
            int col = pos[0] - 'a';
            int row = 8 - (pos[1] - '0');
            return new Position(row, col);
        }
    }
}
