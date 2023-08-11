using ChessBot.Engine;
using System.Diagnostics;
namespace ChessBot.Application;


public class ChessPlayer { // Stole this idea from SebLague, able to reference actual types after casting

	public float timeLeft;
	public readonly UCIPlayer? UCI=null;
	public readonly ComputerPlayer? Computer=null;
	public readonly Player Player;

	public bool IsThreaded => Computer!=null;

	public char color {
		get { return Player.color; }
		set { Player.color = color; }
	}

	public bool IsSearching {
		get {
			if (UCI != null) {
				return UCI.IsSearching;
			} 
			if (Computer != null) {
				return Computer.IsSearching;
			}
			return false;
		}
		set {
			if (UCI != null) {
				UCI.IsSearching = value;
			} else
			if (Computer != null) {
				Computer.IsSearching = value;
			}
		}
	}
	public ChessPlayer() { Player = new Player('f'); }

	public ChessPlayer(Player instance, float timeLeft)  {
		this.timeLeft = timeLeft;
		UCI = instance as UCIPlayer;
		Computer = instance as ComputerPlayer;
		Player = instance;
	}

	public override string ToString() {
		if (UCI != null) {
			return "UCI Player";
		}
		if (Computer != null) {
			return "Computer Player";
		}
		return "Human Player";
	}

	
	public void StartThread() {
		Computer?.StartThread();
	}

	public void Join() {
		Player.Join(); 
	}
	public void RaiseManualUpdateFlag() {
		Player.RaiseManualUpdateFlag();
	}
	public void RaiseExitFlag() {
		Player.RaiseExitFlag();
	}
}