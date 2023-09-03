using System.Diagnostics;
using ChessBot.Application;
using ChessBot.Engine;
using Newtonsoft.Json;

namespace ChessBot.Helpers;

public class Perft {
	public static Dictionary<string, Dictionary<string, int>> testedFens;
	public string fenPosition;
	public int maxDepth = 3;
	public Board board;
	Dictionary<int, int> depthList;
	Dictionary<string, int> totalNodesByMove;
	public bool hadError = false;

	static Perft() {
		Dictionary<string, Dictionary<string, int>>? test;
		using (StreamReader r = new StreamReader(FileHelper.GetResourcePath("Perft.json"))) {
			var json = r.ReadToEnd();
			test = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, int>>>(json);
		}
		Debug.Assert(test != null);
		testedFens = test;
	}

	public Perft(string fenPosition, int depth, bool shouldExec=true) {
		this.fenPosition = fenPosition;
		this.board = new Board(fenPosition);
		PreInit(depth, shouldExec);
		Debug.Assert(depthList != null);
		Debug.Assert(totalNodesByMove != null);
	}
	public Perft(Board board, int depth, bool shouldExec=true) {
		this.board = board;
		this.fenPosition = new Fen(board).ToString();
		PreInit(depth, shouldExec);
		Debug.Assert(depthList != null);
		Debug.Assert(totalNodesByMove != null);
	}
	private void PreInit(int depth, bool shouldExec) {
		maxDepth = depth;
		depthList = new Dictionary<int, int>();
		totalNodesByMove = new Dictionary<string, int>();
		depthList.Add(0, 1);
		for (int i=1; i<=maxDepth; i++) { depthList.Add(i, 0); }
		if (shouldExec) Test();
	}



	public void Test() {
		if (! testedFens.ContainsKey(fenPosition)) throw new Exception($"No perft results from fen {fenPosition}");
		var perftResults = testedFens[fenPosition];

		int depth = int.Parse(perftResults.Last().Key);
		maxDepth = depth;
		ConsoleHelper.WriteLine(ConsoleColor.Magenta);
		ConsoleHelper.WriteLine($"Starting test: {fenPosition}", ConsoleColor.Magenta);

		Board board = new Board(fenPosition);

		double startTime = ((DateTimeOffset)DateTime.UtcNow).ToUnixTimeMilliseconds()/1000.0;
		(Dictionary<int, int> depthList, Dictionary<string, int> totalNodesByMove) = GetDepth();
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
		if (testFailed) {
			WaveFunctionCollapse.CalculateMoveDiscrepancy(fenPosition, depth);
		}
	}

	public (Dictionary<int, int> depthList, Dictionary<string, int> totalNodesByMove) GetDepth() {

		depthList[maxDepth] = CountMove(maxDepth);
		return (depthList, totalNodesByMove);
	}

	public int CountMove(int depth) {
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
				int x = CountMove(depth-1);
				if (hadError) {
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