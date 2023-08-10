using ChessBot.Engine;
namespace ChessBot.Application;


public interface IPlayer {
	
	public Move Think();
	public void Join();
	public void RaiseExitFlag();
	public void SetShouldSearch(bool n);
}