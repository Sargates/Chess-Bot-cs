using System.Diagnostics;
using ChessBot.Application;
using ChessBot.Engine;

namespace ChessBot.Helpers;
public static class Perft {
	public static List<int> depthList = new List<int>();
	public static int maxDepth = 3;

	static Model? model;
	static Board? board;

	public static void GetDepth() {
		model = MainController.Instance.model;
		board = BoardHelper.GetBoardCopy(model.board);
		depthList.Clear();
		depthList.Add(1);
		for (int i=1; i<=maxDepth; i++) { depthList.Add(0); }


		DateTime time = DateTime.UtcNow;
		double startTime = ((DateTimeOffset)time).ToUnixTimeMilliseconds()/1000.0;
		depthList[maxDepth] = CountMove(maxDepth);
		time = DateTime.UtcNow;
		double finishTime = ((DateTimeOffset)time).ToUnixTimeMilliseconds()/1000.0;

		Console.WriteLine();
		for (int i=0; i<depthList.Count; i++) {
			Console.WriteLine($"Depth {i}: {depthList[i]}");
		}
		Console.WriteLine($"Time elapsed: {finishTime-startTime}");
	}

	public static int CountMove(int depth) {
		Debug.Assert(board!=null);
		Move[] totalMoves;
		if (depth == maxDepth) { // Sort all moves if true
			totalMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor, true);
		} else {
			totalMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor);
		}
		if (depth <= 1) { return totalMoves.Length; }


		int subsequentSum = 0;

		for (int i=0; i<totalMoves.Length; i++) {
			Move move = totalMoves[i];
			board.MakeMove(move);

			depthList[maxDepth-depth+1] += 1;
			int x = CountMove(depth-1);
			subsequentSum += x;
			if (depth == maxDepth) {
				Console.WriteLine($"{move}: {x}");
			}

			board.UndoMove();
		}


		return subsequentSum;
	}
}