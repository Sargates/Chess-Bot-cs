using System.Diagnostics;
using ChessBot.Application;
using ChessBot.Engine;
using Newtonsoft.Json;

namespace ChessBot.Helpers;

public static class Perft {
	public static int maxDepth = 3;
	public static bool hasFailedBefore;

	public static Dictionary<string, Dictionary<string, int>> testedFens;
	static bool hadError = false;

	static Perft() {
		Dictionary<string, Dictionary<string, int>>? test;
		using (StreamReader r = new StreamReader(FileHelper.GetResourcePath("Perft.json"))) {
			var json = r.ReadToEnd();
			test = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(json);
		}
		Debug.Assert(test != null);
		testedFens = test;
	}

	public static void Test() { for (int i=0; i<testedFens.Count; i++) { TestSpecific(i); } }


	public static void TestSpecific(int index) {
		var kvPair = testedFens.ElementAt(index);
		var fenString = kvPair.Key;
		var perftResults = kvPair.Value;

		int depth = int.Parse(perftResults.Last().Key);
		maxDepth = depth;
		ConsoleHelper.WriteLine(ConsoleColor.Magenta);
		ConsoleHelper.WriteLine($"Starting test: {fenString}", ConsoleColor.Magenta);

		Board board = new Board(fenString);

		double startTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds()/1000.0;
		(Dictionary<int, int> depthList, Dictionary<string, int> totalNodesByMove) = GetDepth(board, depth);
		double finishTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds()/1000.0;

		ConsoleHelper.WriteLine(ConsoleColor.Magenta);

		bool testFailed = false;
		for (int i=0; i<depthList.Count; i++) {
			if (depthList[i] == perftResults[$"{i}"]) {
				// String.Format()
				ConsoleHelper.Write("Tested Passed: ", ConsoleColor.Green);
				ConsoleHelper.WriteLine($"Depth {i,2}: {depthList[i],10:n0}", ConsoleColor.Green);
			} else {
				testFailed = true;
				ConsoleHelper.Write("Tested Failed: ", ConsoleColor.DarkRed);
				ConsoleHelper.Write($"Depth {i,2}: {depthList[i],10:n0} ", ConsoleColor.Red);
				ConsoleHelper.WriteLine($"Expected {i,2}: {perftResults[$"{i}"],10:n0}", ConsoleColor.DarkYellow);
			}
		}
		ConsoleHelper.WriteLine($"Time elapsed: {finishTime-startTime}", ConsoleColor.Magenta);
		if (testFailed || !hasFailedBefore) {
			hasFailedBefore = true;
			
			// WaveFunctionCollapse.CalculateMoveDiscrepancy(fenString, depth);
		}
	}

	public static (Dictionary<int, int> depthList, Dictionary<string, int> totalNodesByMove) GetDepth(Board board, int maxDepth) {

		hadError = false;
		Dictionary<string, int> totalNodesByMove = new Dictionary<string, int>();
		Dictionary<int, int> depthList = new Dictionary<int, int>();
		depthList.Add(0, 1);
		for (int i=1; i<=maxDepth; i++) { depthList.Add(i, 0); }
		Perft.maxDepth = maxDepth;


		depthList[maxDepth] = CountMove(board, depthList, totalNodesByMove, maxDepth);
		return (depthList, totalNodesByMove);
	}

	public static int CountMove(Board board, Dictionary<int, int> depthList, Dictionary<string, int> totalNodesByMove, int depth) {
		Debug.Assert(board!=null);
		Move[] totalMoves;

		if (depth == maxDepth) { totalMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor, true); }
		// Sort all moves if true
		else { totalMoves = MoveGenerator.GetAllMoves(board, board.ActiveColor); }

		if (depth <= 1) { return totalMoves.Length; }

		int subsequentSum = 0;

		Move move = Move.NullMove;
		try {
			for (int i=0; i<totalMoves.Length; i++) {
				move = totalMoves[i];
				board.MakeMove(move);

				depthList[maxDepth-depth+1] += 1;
				int x = CountMove(board, depthList, totalNodesByMove, depth-1);
				if (hadError) {
					// BoardHelper.PrintBoard(board);
					ConsoleHelper.WriteLine($"{maxDepth-depth+1}: {move}", ConsoleColor.Magenta);
					break;
				}
				subsequentSum += x;
				if (depth == maxDepth) {
					if (!totalNodesByMove.ContainsKey(move.ToString())) totalNodesByMove.Add(move.ToString(), 0);
					totalNodesByMove[move.ToString()] += x;
				}

				board.UndoMove();
			}

		} catch (Exception e) {
			Console.WriteLine($"Caught Exception: {e}");
			ConsoleHelper.WriteLine($"{maxDepth-depth+1}: {move}", ConsoleColor.Magenta);
			BoardHelper.PrintBoard(board);
			hadError = true;
		}

		return subsequentSum;
	}
}