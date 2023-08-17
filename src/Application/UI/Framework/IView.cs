namespace ChessBot.Application;

public interface IView {
	public void Draw();
	public void Update(float dt);
}