using System;
using System.Collections.Generic;
using System.Linq;
using SimpleChess.Models;

namespace SimpleChess.ViewModels
{
    public partial class MainViewModel
    {
        private bool IsKingInCheck(PlayerColor kingColor)
        {
            // Find the king
            Square kingSquare = Squares.FirstOrDefault(s => s.Piece?.Type == PieceType.King && s.Piece.Color == kingColor);
            if (kingSquare == null)
                return false;

            // Check if any opponent piece can attack the king
            PlayerColor opponentColor = kingColor == PlayerColor.White ? PlayerColor.Black : PlayerColor.White;

            foreach (var square in Squares)
            {
                if (square.Piece != null && square.Piece.Color == opponentColor)
                {
                    // Check if this piece can attack the king
                    if (ValidatePieceSpecificMove(square.Piece, square.Position, kingSquare.Position))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool WouldMoveResultInCheck(Square fromSquare, Square toSquare)
        {
            // Save current state
            Piece originalFromPiece = fromSquare.Piece;
            Piece originalToPiece = toSquare.Piece;
            Position originalFromPosition = originalFromPiece?.Position;

            // Simulate the move
            toSquare.Piece = fromSquare.Piece;
            if (toSquare.Piece != null)
                toSquare.Piece.Position = toSquare.Position;
            fromSquare.Piece = null;

            // Check if the king is in check after the move
            bool wouldBeInCheck = IsKingInCheck(originalFromPiece.Color);

            // Restore original state
            fromSquare.Piece = originalFromPiece;
            if (fromSquare.Piece != null)
                fromSquare.Piece.Position = originalFromPosition;
            toSquare.Piece = originalToPiece;

            return wouldBeInCheck;
        }

        private bool HasValidMoves(PlayerColor playerColor)
        {
            foreach (var fromSquare in Squares.Where(s => s.Piece?.Color == playerColor))
            {
                foreach (var toSquare in Squares)
                {
                    // Skip same square
                    if (fromSquare == toSquare)
                        continue;

                    // Check if move is valid according to piece rules
                    if (IsValidMove(fromSquare, toSquare))
                    {
                        // Check if move would leave king in check
                        if (!WouldMoveResultInCheck(fromSquare, toSquare))
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        private void UpdateGameState()
        {
            bool isInCheck = IsKingInCheck(_currentPlayer);
            _isKingInCheck = isInCheck;

            if (isInCheck)
            {

                // Check if there are any valid moves that would get out of check
                if (!HasValidMoves(_currentPlayer))
                {
                    GameState = GameState.Checkmate;
                    StatusMessage = $"Checkmate! {(_currentPlayer == PlayerColor.White ? "Black" : "White")} wins!";
                }
                else
                {
                    GameState = GameState.Check;
                    StatusMessage = $"{_currentPlayer} is in check!";
                }
            }
            else
            {
                // Check for stalemate - no valid moves but not in check
                if (!HasValidMoves(_currentPlayer))
                {
                    GameState = GameState.Stalemate;
                    StatusMessage = "Stalemate! The game is a draw.";
                }
                else
                {
                    GameState = GameState.InProgress;
                    StatusMessage = $"{_currentPlayer}'s turn to move";
                }
            }
        }
    }
}
