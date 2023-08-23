using ChessBot.Application;
using ChessBot.Engine;

namespace ChessBot.Helpers;
public static class UnmakeMoveHelper {
	public static void Fast() {
		Console.WriteLine("Starting Fast Test");
		bool temp = true;
		foreach (string fenString in MainController.Instance.model.botMatchStartFens.Take(10)) {
			List<Move> failedMoves = new List<Move>();
			Board board = new Board(fenString);
			Move[] allMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor);
			foreach (Move move in allMoves) {
				Piece[] oldBoard = board.board.ToArray();
				board.MakeMove(move);
				board.UndoMove();
				Piece[] newBoard = board.board.ToArray();
				if (! Enumerable.SequenceEqual(oldBoard, newBoard)) {
					if (temp) {
						temp = false;
						Piece pieceTaken = board.currentState.pieceTaken;
						Board.PrintBoard(oldBoard);
						Console.WriteLine($"\n{pieceTaken}\n");
						Board.PrintBoard(newBoard);
					}
					failedMoves.Add(move);
				}
			}
			if (failedMoves.Count != 0) {
				Console.WriteLine($"Failed Test: {fenString} with moves {string.Join(", ", failedMoves)}");
			}
		}
		Console.WriteLine("Finished Fast Test");
	}
	public static void FullSuite() {
		Console.WriteLine("Starting Full Suite Test");
		bool temp = true;
		foreach (string fenString in MainController.Instance.model.botMatchStartFens.Take(10)) {
			List<Move> failedMoves = new List<Move>();
			Board board = new Board(fenString);
			Move[] allMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor);
			foreach (Move move in allMoves) {
				Piece[] oldBoard = board.board.ToArray();
				board.MakeMove(move);
				board.UndoMove();
				Piece[] newBoard = board.board.ToArray();
				if (! Enumerable.SequenceEqual(oldBoard, newBoard)) {
					if (temp) {
						temp = false;
						Piece pieceTaken = board.currentState.pieceTaken;
						Board.PrintBoard(oldBoard);
						Console.WriteLine($"\n{pieceTaken}\n");
						Board.PrintBoard(newBoard);
					}
					failedMoves.Add(move);
				}
			}
			if (failedMoves.Count != 0) {
				Console.WriteLine($"Failed Test: {fenString} with moves {string.Join(", ", failedMoves)}");
			}
		}
		Console.WriteLine("Finished Full Suite Test");
	}


}