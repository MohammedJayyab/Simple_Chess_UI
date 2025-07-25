using System;

namespace SimpleChess.Models
{
    public enum GameState
    {
        InProgress,
        Check,
        Checkmate,
        Stalemate,
        Draw
    }
}