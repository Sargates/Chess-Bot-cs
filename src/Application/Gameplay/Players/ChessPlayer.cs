using ChessBot.Engine;
using System.Diagnostics;
namespace ChessBot.Application;


public class ChessPlayer { // Stole this idea from SebLague, able to reference actual types after casting

	public float timeLeft;
	public readonly UCIPlayer? UCI;
	public readonly ComputerPlayer? Computer;
	public readonly Player? Player;

	public char color {
		get {
			if (UCI != null) {
				return UCI.color;
			} 
			if (Computer != null) {
				return Computer.color;
			}
			return color;
		}
		set {
			if (UCI != null) {
				UCI.color = value;
			} 
			if (Computer != null) {
				Computer.color = value;
			}
			color = value;
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
			return true;
		}
		set {
			if (UCI != null) {
				UCI.IsSearching = value;
			} else
			if (Computer != null) {
				Computer.IsSearching = value;
			} else
			{
				Console.WriteLine("IsSearching set on Human Player");
			}
		}
	}
	public ChessPlayer() {}

	public ChessPlayer(Player instance, float timeLeft)  {
		this.timeLeft = timeLeft;
		UCI = instance as UCIPlayer;
		if (UCI==null) Computer = instance as ComputerPlayer; // UCIPlayer is derived from Computer player, don't want both
		if (Computer==null) Player = instance as Player;
	}

	public override string ToString() {
		if (UCI != null) {
			return "UCI";
		}
		if (Computer != null) {
			return "Computer";
		}
		return $"Human Player";
	}

	public void Join() {
		UCI?.Join(); 
		Computer?.Join(); 
	}
	public void RaiseExitFlag() {
		UCI?.RaiseExitFlag();
		Computer?.RaiseExitFlag();
	}
}