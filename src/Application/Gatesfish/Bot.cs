using ChessBot.Helpers;
using ChessBot.Engine;
using System.Diagnostics;

namespace ChessBot.Application;


public class Bot {
	public int maxSearchDepth=5;
	Model model;
	Board board;
	public Bot(Model model) { 
		this.model = model;
		Debug.Assert(board!=null);
	}

	public int Search(int depth) {
		if (depth <= 0) { return Evaluation.SumFromBoard(board, board.ActiveColor); }

		Move[] totalMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor);

		int maxEval = -1000000;
		for (int i=0; i<totalMoves.Length; i++) {
			Move move = totalMoves[i];
			board.MakeMove(move);

			maxEval = Math.Max(-Search(depth-1), maxEval);


			board.SetPrevState();
		}

		return maxEval;
	}


	public Move Think() {
		Move bestMove = Move.NullMove;
		board = BoardHelper.GetBoardCopy(model.board);
		Move[] totalMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor);

		
		int maxEval = -1000000;
		for (int i=0; i<totalMoves.Length; i++) {
			Move move = totalMoves[i];
			board.MakeMove(move);

			int searchedEval = -Search(maxSearchDepth-1);
			if (searchedEval > maxEval) {
				maxEval = searchedEval;
				bestMove = move;
			}


			board.SetPrevState();
		}

		return bestMove;
	}
}