using System.Diagnostics;
using ChessBot.Helpers;
using ChessBot.Engine;

namespace ChessBot.Application;
public class ComputerPlayer : Player {
	public Model model;
	public bool IsThreaded;
	public bool HasStarted;
	public Thread thread;
	public ConsoleColor threadColor;
	public static ConsoleColor[] ThreadColorBlacklist = { ConsoleColor.Black, ConsoleColor.DarkBlue, ConsoleColor.White, ConsoleColor.DarkYellow, ConsoleColor.Yellow };

	public ComputerPlayer(char color, Model model) : base(color) {
		this.model = model;
		IsThreaded = true;
		IsSearching = false;
		while (ThreadColorBlacklist.Contains(threadColor) ) {
			threadColor = (ConsoleColor) Controller.random.Next(16);
		}
		HasStarted = false;
		ThreadStart ths = new ThreadStart(Start);
		thread = new Thread(ths);
	}

	public void StartThread() {
		thread.Start();
	}


	public virtual void Start() {
		ConsoleHelper.WriteLine($"Starting Thread {color}", threadColor);
		HasStarted = true;
		while (true) {
			if (ExitFlag) {
				break;
			}
			if (OnMoveChosen == null) {
				continue;
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

	public override Move Think() {
		return Move.NullMove;
	}


	public override void Join() { thread.Join(); }

	
	~ComputerPlayer() { // In case player object ever goes out of scope
		ExitFlag = true;
		thread.Join();
	}
}
