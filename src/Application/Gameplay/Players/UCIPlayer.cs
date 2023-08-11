using System.Diagnostics;
using ChessBot.Helpers;
using ChessBot.Engine;

namespace ChessBot.Application;

public class UCIPlayer : ComputerPlayer {
	public UCIEngine engine;
	

	public UCIPlayer(
			char color, Model model,
			string pathToExe, int depth = 22,
			UCISettings? settings = null) : base(color, model) {

		engine = new UCIEngine(pathToExe, depth, settings);
	}

	public override void Start() {
		engine.Start();
		engine.SetPosition($"position fen {model.board.currentFen.ToFEN()}");
		ConsoleHelper.WriteLine($"Starting Thread {color}", threadColor);
		while (true) {
			if (ExitFlag) {
				break; }
			if (OnMoveChosen == null) {
				continue; }
			if (ShouldManualUpdate) {
				engine.SetPosition($"position fen {model.board.currentFen.ToFEN()}");
				ShouldManualUpdate = false;
			}

			if (IsSearching) {
				Move bestmove = Think();
				// If the bot gets a manual update request it means the board state has changed and the previous move is garbage
				if (! ShouldManualUpdate) OnMoveChosen(bestmove);
				IsSearching = false;
			}
		}
		ConsoleHelper.WriteLine($"Exiting Thread {color}", threadColor);
	}



	public override Move Think() { // position should already be set
		string fen = model.board.currentFen.ToFEN();
		engine.SetPosition($"position fen {fen}");
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
		// Console.WriteLine($"{(color == 'w' ? "White's" : "Blacks")} response: {response} on {fen}");
		return new Move(BoardHelper.NameToSquareIndex(response.Substring(0, 2)), BoardHelper.NameToSquareIndex(response.Substring(2, 2)), promoChar);
	}

	~UCIPlayer() { // In case player object ever goes out of scope
		ExitFlag = true;
		thread.Join();
	} // Apparently deconstructors aren't inherited, kind of makes sense
}