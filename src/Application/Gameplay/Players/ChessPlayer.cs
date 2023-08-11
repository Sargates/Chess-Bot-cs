using ChessBot.Engine;
using System.Diagnostics;
namespace ChessBot.Application;


public class ChessPlayer { // Stole this idea from SebLague, able to reference actual types after casting

	public float timeLeft;
	public readonly UCIPlayer? UCI=null;
	public readonly ComputerPlayer? Computer=null;
	public readonly Player? Player=null;

	public char color {
		get {
			if (UCI != null) {
				return UCI.color;
			} else 
			if (Computer != null) {
				return Computer.color;
			} else 
			if (Player != null) {
				return Player.color;
			}
			throw new Exception("Player object hidden by ChessPlayer is invalid");
		}
		set {
			if (UCI != null) {
				UCI.color = value;
			} 
			if (Computer != null) {
				Computer.color = value;
			}
			if (Player != null) {
				Player.color = value;
			}
		}
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
	public ChessPlayer() {}

	public ChessPlayer(Player instance, float timeLeft)  {
		this.timeLeft = timeLeft;
		UCI = instance as UCIPlayer;
		if (UCI==null) Computer = instance as ComputerPlayer; // UCIPlayer is derived from ComputerPlayer, don't want more than one
		if (UCI==null && Computer==null) Player = instance;  // UCIPlayer, ComputerPlayer is derived from Player, don't want more than one
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
		UCI?.StartThread();
		Computer?.StartThread();
	}

	public void Join() {
		UCI?.Join(); 
		Computer?.Join(); 
	}
	public void RaiseManualUpdateFlag() {
		UCI?.RaiseManualUpdateFlag();
		Computer?.RaiseManualUpdateFlag();
	}
	public void RaiseExitFlag() {
		UCI?.RaiseExitFlag();
		Computer?.RaiseExitFlag();
	}
}