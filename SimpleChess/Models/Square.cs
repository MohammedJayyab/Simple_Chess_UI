
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleChess.Models
{
    public class Square : INotifyPropertyChanged
    {
        private Piece _piece;
        private bool _isSelected;
        private bool _isValidMove;
        private bool _isInCheck;
        private bool _isLastMove;

        public Position Position { get; }
        public string Notation => Position.GetNotation();
        public bool IsLight => (Position.Row + Position.Column) % 2 == 0;

        public Piece Piece
        {
            get => _piece;
            set
            {
                if (_piece != value)
                {
                    _piece = value;
                    if (_piece != null)
                        _piece.Position = Position;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsValidMove
        {
            get => _isValidMove;
            set
            {
                if (_isValidMove != value)
                {
                    _isValidMove = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsInCheck
        {
            get => _isInCheck;
            set
            {
                if (_isInCheck != value)
                {
                    _isInCheck = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLastMove
        {
            get => _isLastMove;
            set
            {
                if (_isLastMove != value)
                {
                    _isLastMove = value;
                    OnPropertyChanged();
                }
            }
        }

        public Square(int row, int column)
        {
            Position = new Position(row, column);
        }

        public Square(Position position)
        {
            Position = position ?? throw new ArgumentNullException(nameof(position));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{Notation}{(Piece != null ? $" - {Piece}" : " - empty")}";
        }
    }
}
