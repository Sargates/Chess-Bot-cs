/////////////////////////////////////////////////////////////////////
// Most of this is taken from Stockfish.NET
// The only real changes are to make it compatible with my engine
/////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using ChessBot.Helpers;
using ChessBot.Engine;

namespace ChessBot.Application;

public class UCIPlayer : ChessPlayer{
	// public char color;
	UCIEngine engine;
	Board board;

	public bool IsThreaded;
	public Thread thread;

	public UCIPlayer(
			char color,
			string pathToExe,
			Board board,
			int depth = 22,
			UCISettings? settings = null) {

		IsThreaded = true;
		engine = new UCIEngine(pathToExe, depth, settings);
		this.board = board;
		this.color = color;

		// OnMoveChosen += board.MakeMove;

		ThreadStart ths = new ThreadStart(Start);
		thread = new Thread(ths);
		thread.Start();
	}

	public void Start() {
		engine.Start();
		Console.WriteLine($"Starting Thread {color}");
		while (true) {
			if (ExitFlag) {
				break;
			}
			if (OnMoveChosen == null) {
				continue;
			}
			
			
			if (IsSearching) {
				OnMoveChosen(Think());
				IsSearching = false;
			}
		}
		Console.WriteLine($"Exiting Thread {color}");
	}

	public override Move Think() {
		engine.SetPosition(board.GetUCIGameFormat());
		string response = engine.GetBestMoveTime(400);
		int promoChar = 0;
		if (response.Length > 4) { // is Promotion
			promoChar = response[4] switch {
				'q' => Move.PromoteToQueenFlag,
				'b' => Move.PromoteToBishopFlag,
				'n' => Move.PromoteToKnightFlag,
				'r' => Move.PromoteToRookFlag,
				_ => 0
			};
		}
		return new Move(BoardHelper.NameToSquareIndex(response.Substring(0, 2)), BoardHelper.NameToSquareIndex(response.Substring(2, 2)), promoChar);
	}

	public override void Join() { thread.Join(); }
	public override void RaiseExitFlag() { ExitFlag = true; }
	public override void SetShouldSearch(bool n) { IsSearching = n; }

	~UCIPlayer() {
		ExitFlag = true;
		thread.Join();
	}
}
