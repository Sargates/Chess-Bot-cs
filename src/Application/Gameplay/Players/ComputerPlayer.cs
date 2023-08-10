using System.Diagnostics;
using ChessBot.Helpers;
using ChessBot.Engine;

namespace ChessBot.Application;
public class ComputerPlayer : Player {
	public Model model;
	public bool IsThreaded;
	public Thread thread;

	public ComputerPlayer(char color, Model model, bool inherited=false) : base(color) {
		this.model = model;
		IsThreaded = true;

		ThreadStart ths = new ThreadStart(Start);
		thread = new Thread(ths);
		if (! inherited) {
			thread.Start();
		}
	}


	public virtual void Start() {
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
		return Move.NullMove;
	}

	public override void Join() { thread.Join(); }

	
	~ComputerPlayer() { // In case player object ever goes out of scope
		ExitFlag = true;
		thread.Join();
	}
}
