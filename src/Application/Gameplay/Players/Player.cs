using ChessBot.Engine;

namespace ChessBot.Application;

public class Player {
	
	public char color;

	public bool ShouldManualUpdate;
	public bool ExitFlag;
	public bool IsSearching=true;

	public Player(char color) {
		this.color = color;
	}

	public delegate void VoidDel(Move move, bool animate=true);
	public static VoidDel? OnMoveChosen;

	public virtual Move Think() { return Move.NullMove; }
	public void RaiseManualUpdateFlag() { ShouldManualUpdate = true; }
	public virtual void RaiseExitFlag() { ExitFlag = true; }
	public virtual void Join() {}
}