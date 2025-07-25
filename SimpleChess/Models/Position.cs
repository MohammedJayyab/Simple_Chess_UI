
using System;

namespace SimpleChess.Models
{
    public class Position : IEquatable<Position>
    {
        public int Row { get; }
        public int Column { get; }

        public Position(int row, int column)
        {
            if (row < 0 || row > 7 || column < 0 || column > 7)
                throw new ArgumentOutOfRangeException("Position must be within the chessboard (0-7)");

            Row = row;
            Column = column;
        }

        public string GetNotation()
        {
            // Convert to standard chess notation (a1, b3, etc.)
            char file = (char)('a' + Column);
            int rank = 8 - Row;

            return $"{file}{rank}";
        }

        public static Position FromNotation(string notation)
        {
            if (string.IsNullOrEmpty(notation) || notation.Length != 2)
                throw new ArgumentException("Notation must be exactly 2 characters (e.g. 'e4')");

            char file = char.ToLower(notation[0]);
            int rank = int.Parse(notation[1].ToString());

            if (file < 'a' || file > 'h' || rank < 1 || rank > 8)
                throw new ArgumentException("Invalid notation. File must be a-h, rank must be 1-8");

            int column = file - 'a';
            int row = 8 - rank;

            return new Position(row, column);
        }

        public bool Equals(Position other)
        {
            if (other is null)
                return false;

            return Row == other.Row && Column == other.Column;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Position);
        }

        public override int GetHashCode()
        {
            return (Row * 8) + Column;
        }

        public static bool operator ==(Position left, Position right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Position left, Position right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return GetNotation();
        }
    }
}
