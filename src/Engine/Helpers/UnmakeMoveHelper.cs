using ChessBot.Application;
using ChessBot.Engine;

namespace ChessBot.Helpers;
public static class UnmakeMoveHelper {
	public static void Fast() {
		Console.WriteLine("Starting Fast Test");
		Test(MainController.Instance.model.botMatchStartFens.Take(10).ToArray());
		Console.WriteLine("Finished Fast Test");
	}
	public static void FullSuite() {
		Console.WriteLine("Starting Full Suite Test");
		Test(MainController.Instance.model.botMatchStartFens);
		Console.WriteLine("Finished Full Suite Test");
	}

	public static void Test(string[] allFens) {
		bool temp = true;
		foreach (string fenString in allFens) {
			List<Move> failedMoves = new List<Move>();
			Board board = new Board(fenString);
			Move[] allMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor);
			foreach (Move move in allMoves) {
				ulong[,] oldBoard = BitboardHelper.Copy(board.pieces);
				board.MakeMove(move);
				board.UndoMove();
				ulong[,] newBoard = BitboardHelper.Copy(board.pieces);
				if (! BitboardHelper.SequenceEquals(oldBoard, newBoard)) {
					if (temp) {
						temp = false;
						Piece pieceTaken = board.currentState.pieceTaken;
						// BoardHelper.PrintBoard(oldBoard);
						Console.WriteLine($"\n{pieceTaken}\n");
						// BoardHelper.PrintBoard(newBoard);
					}
					failedMoves.Add(move);
				}
			}
			if (failedMoves.Count != 0) {
				Console.WriteLine($"Failed Test: {fenString} with moves {string.Join(", ", failedMoves)}");
			}
		}
	}


}