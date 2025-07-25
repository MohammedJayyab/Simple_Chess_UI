using System.Collections.Generic;

namespace SimpleChess.Models
{
    public class GameSaveState
    {
        public PlayerColor CurrentPlayer { get; set; }
        public GameState GameStatus { get; set; }
        public List<MoveRecord> Moves { get; set; }
    }

    public class MoveRecord
    {
        public int FromRow { get; set; }
        public int FromColumn { get; set; }
        public int ToRow { get; set; }
        public int ToColumn { get; set; }
        public PieceType PieceType { get; set; }
        public PlayerColor PlayerColor { get; set; }
    }
}