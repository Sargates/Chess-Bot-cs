﻿using System.Collections.Generic;
using ChessBot.Engine.Stockfish;

namespace ChessBot.Engine.Stockfish {
    public interface IStockfish {
        int Depth { get; set; }
        int SkillLevel { get; set; }
        void SetPosition(params string[] move);
        string GetBoardVisual();
        string GetFenPosition();
        void SetFenPosition(string fenPosition);
        string? GetBestMove();
        string? GetBestMoveTime(int time = 1000);
        bool IsMoveCorrect(string moveValue);
        Evaluation GetEvaluation();
    }
}