using System;

namespace ChessBot.Engine.Stockfish {
    public class MaxTriesException: Exception {
        public  MaxTriesException(string msg="") : base(msg) { }
    }
}