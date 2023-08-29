
using ChessBot.Helpers;
using ChessBot.Application;

namespace ChessBot.Engine;

public static class WaveFunctionCollapse {
	static UCIEngine engine;
	static WaveFunctionCollapse() {
		engine = new UCIEngine(Model.stockfishExeExt);
		engine.Start();
	}

	public static void CalculateMoveDiscrepancy(string fenPosition, int totalDepth) {
		Board board = new Board(fenPosition);
		int depth = totalDepth;
		
		List<string> LineToDiscrepancy = new List<string>();
		ConsoleHelper.WriteLine($"Finding discrepancy from FEN: {fenPosition}");
		BoardHelper.PrintBoard(new Board(fenPosition));
		while (true) {
			if (depth == 0) { throw new Exception("Depth got to zero before loop break"); }
			engine.SetPosition($"position fen {fenPosition} moves {board.GetMoveHistory()}");
			Dictionary<string, int> stockfishPerftResults = engine.GoPerft(depth);
			Move[] boardMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor);
			string[] sBoardMoves = boardMoves.Select(x => $"{x}").ToArray();

			// Moves that the board generates that stockfish doesnt, (illegal moves)
			string[] stockfishMissing = sBoardMoves.Except(stockfishPerftResults.Keys).ToArray();

			// Moves that the engine doesn't generate that it should (engine imperfection)
			string[] engineMissing = stockfishPerftResults.Keys.Except(sBoardMoves).ToArray();

			string[] total = stockfishMissing.Concat(engineMissing).ToArray();
			// // Passes guard clause if no discrepency found in current iteration
			(Dictionary<int, int> depthList, Dictionary<string, int> totalNodesByMove) = Perft.GetDepth(board, depth);

			bool shouldBreak = false;
			if (engineMissing.Length > 0) {
				Console.WriteLine(string.Join(", ", engineMissing));
				ConsoleHelper.WriteLine("Engine is missing a move", ConsoleColor.DarkRed);
				
				foreach (string stringMove in stockfishMissing) {
					ConsoleHelper.WriteLine($"Failed to locate move: {stringMove}", ConsoleColor.DarkYellow);
				}
				shouldBreak = true;
				break;
				
			} else
			if (stockfishMissing.Length > 0) {
				Console.WriteLine(string.Join(", ", stockfishMissing));
				ConsoleHelper.WriteLine("Stockfish is missing a move", ConsoleColor.DarkRed);
				
				foreach (string stringMove in stockfishMissing) {
					ConsoleHelper.WriteLine($"Failed to locate move: {stringMove}", ConsoleColor.DarkYellow);
				}
				shouldBreak = true;
				break;
				
			} if (shouldBreak) break;
			// Passes guard clause if engine correctly generated available moves

			// Console.WriteLine(string.Join(",  ", stockfishPerftResults));
			// Console.WriteLine(string.Join(",  ", totalNodesByMove));

			bool foundDiscrepancy = false;
			foreach (var sfKVpair in stockfishPerftResults) {
				if (foundDiscrepancy) break;
				foreach (var engKVpair in totalNodesByMove) {
					// Console.WriteLine($"{sfKVpair} : {engKVpair}");
					if (foundDiscrepancy) break;
					if (sfKVpair.Key != engKVpair.Key) continue;
					if (sfKVpair.Value == engKVpair.Value) continue;
					ConsoleHelper.Write($"Found discrepancy; ", ConsoleColor.Red);
					ConsoleHelper.Write($"Stockfish Result: {sfKVpair} ", ConsoleColor.DarkGray);
					ConsoleHelper.Write($"Engine Result: {engKVpair} ", ConsoleColor.Yellow);
					Console.WriteLine();
					LineToDiscrepancy.Add(sfKVpair.Key);
					foundDiscrepancy = true;
					break;
				}
			}

			if (! foundDiscrepancy) {
				// Test was successful, engine passes
				ConsoleHelper.WriteLine("No discrepancy found, engine passed", ConsoleColor.Green);
				return;
			}

			// break;
			Move targetMove = new Move(BoardHelper.NameToSquareIndex(LineToDiscrepancy.Last().Substring(0, 2)),BoardHelper.NameToSquareIndex(LineToDiscrepancy.Last().Substring(2, 2)));
			foreach (Move testedMove in boardMoves) {
				if (testedMove == targetMove) {
					ConsoleHelper.WriteLine($"Move chosen: {testedMove}", ConsoleColor.DarkMagenta);
					board.MakeMove(testedMove);
					break;
				}
			}
			depth--;
		}

		ConsoleHelper.WriteLine($"Line to discrepancy: {string.Join(", ", LineToDiscrepancy)}");


	}
}