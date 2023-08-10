using ChessBot.Engine;

namespace ChessBot.Application;

public class Player {
	
	public char color;

	public bool ExitFlag;
	public bool IsSearching;

	public Player(char color) {
		this.color = color;
	}

	public delegate void VoidDel(Move move, bool animate=true);
	public static VoidDel? OnMoveChosen;

	public virtual Move Think() { return Move.NullMove; }
	public virtual void RaiseExitFlag() { ExitFlag = true; }
	public virtual void Join() {}
}