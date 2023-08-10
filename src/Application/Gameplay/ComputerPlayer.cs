using System.Diagnostics;
using ChessBot.Helpers;
using ChessBot.Engine;

namespace ChessBot.Application;
public class ComputerPlayer : ChessPlayer {
	public Board board;
	public bool IsThreaded;
	public Thread thread;

	public ComputerPlayer(char color, Board board) {
		IsThreaded = true;
		this.color = color;
		this.board = board;

		ThreadStart ths = new ThreadStart(Start);
		thread = new Thread(ths);
		thread.Start();
	}

	public void Start() {
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

	
}
