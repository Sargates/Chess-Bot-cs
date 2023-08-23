using ChessBot.Helpers;
using ChessBot.Engine;

namespace ChessBot.Application;


public class Bot {
	public int maxSearchDepth=5;
	Model model;
	public Bot(Model model) { this.model = model; }

	public int Search(int depth) {
		if (depth <= 0) { return Evaluation.SumFromBoard(model.board, model.board.ActiveColor); }

		Move[] totalMoves = MoveGenerator.GetAllMoves(model.board, model.board.ActiveColor);

		int maxEval = -1000000;
		for (int i=0; i<totalMoves.Length; i++) {
			Move move = totalMoves[i];
			model.board.MakeMove(move);

			maxEval = Math.Max(-Search(depth-1), maxEval);


			model.board.SetPrevState();
		}

		return maxEval;
	}

	public Move Think() {
		Move bestMove = Move.NullMove;

		Move[] totalMoves = MoveGenerator.GetAllMoves(model.board, model.board.ActiveColor);

		int maxEval = -1000000;
		for (int i=0; i<totalMoves.Length; i++) {
			Move move = totalMoves[i];
			model.board.MakeMove(move);

			int searchedEval = -Search(maxSearchDepth-1);
			if (searchedEval > maxEval) {
				maxEval = searchedEval;
				bestMove = move;
			}


			model.board.SetPrevState();
		}

		return bestMove;
	}
}