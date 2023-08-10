using ChessBot.Engine;
namespace ChessBot.Application;


public class ChessPlayer {
	public char color;

	public bool ExitFlag;
	public bool IsSearching;
	public delegate void VoidDel(Move move, bool animate=true);
	public static VoidDel? OnMoveChosen;



	public virtual Move Think() {
		return Move.NullMove;
	}
	public virtual void RaiseExitFlag() { ExitFlag = true; }
	public virtual void SetShouldSearch(bool n) { IsSearching = n; }
	public virtual void Join() {}
}