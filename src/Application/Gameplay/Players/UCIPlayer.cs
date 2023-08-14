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
		HasStarted = true;
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
				Move bestMove = Think();
				// If the bot gets a manual update request it means the board state has changed and the previous move is garbage
				if (! ExitFlag && ! ShouldManualUpdate && ! model.SuspendPlay) {
					OnMoveChosen(bestMove);
				}
				// Console.WriteLine($"{ExitFlag} {ShouldManualUpdate} {model.SuspendPlay}");
				IsSearching = false;
			}
		}
		ConsoleHelper.WriteLine($"Exiting Thread {color}", threadColor);
	}



	public override Move Think() { // position should already be set
		string fen = model.board.currentFen.ToFEN();
		engine.SetPosition($"position fen {fen}");
		string response = engine.GetBestMoveTime(1500);

		int startSquare = BoardHelper.NameToSquareIndex(response.Substring(0, 2));
		int targetSquare = BoardHelper.NameToSquareIndex(response.Substring(2, 2));
		foreach (Move testedMove in MoveGenerator.GetMoves(model.board, startSquare)) {
			if (testedMove.TargetSquare == targetSquare) {
				return testedMove;
			}
		}
		// GetMoves returns empty Move[] if startSquare is empty, the only reason this should happen is if the
		// thread is supposed to exit. Otherwise something is actually wrong and an exception should be thrown
		if (ExitFlag) { return Move.NullMove; } // This shouldnt raise checkmate when it returns
		return Move.NullMove;
	}

	~UCIPlayer() { // In case player object ever goes out of scope
		ExitFlag = true;
		thread.Join();
	} // Apparently deconstructors aren't inherited, kind of makes sense
}
