
using System;

namespace SimpleChess.Models
{
    public class Piece
    {
        public PieceType Type { get; }
        public PlayerColor Color { get; }
        public Position Position { get; set; }
        public bool HasMoved { get; set; }

        public Piece(PieceType type, PlayerColor color, Position position)
        {
            Type = type;
            Color = color;
            Position = position;
            HasMoved = false;
        }

        public string ImagePath => $"/Images/{Type.ToString().ToLower()}-{(Color == PlayerColor.White ? "w" : "b")}.png";

        public Piece Clone()
        {
            return new Piece(Type, Color, Position)
            {
                HasMoved = this.HasMoved
            };
        }

        public override string ToString()
        {
            string colorPrefix = Color == PlayerColor.White ? "W" : "B";
            string pieceCode;

            switch (Type)
            {
                case PieceType.King: pieceCode = "K"; break;
                case PieceType.Queen: pieceCode = "Q"; break;
                case PieceType.Rook: pieceCode = "R"; break;
                case PieceType.Bishop: pieceCode = "B"; break;
                case PieceType.Knight: pieceCode = "N"; break;
                case PieceType.Pawn: pieceCode = "P"; break;
                default: pieceCode = "?"; break;
            }

            return $"{colorPrefix}{pieceCode} at {Position}";
        }
    }
}
