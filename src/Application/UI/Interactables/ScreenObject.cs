using Raylib_cs;
using System.Numerics;
namespace ChessBot.Application;

public class ScreenObject : IRenderable, IUpdatable {
	public delegate void ClickCallback();
	public ClickCallback? OnLeftPressed;
	public ClickCallback? OnLeftReleased;
	public ClickCallback? OnRightPressed;
	public ClickCallback? OnRightReleased;
	private Rectangle _Rect { get; set; }
	public ScreenObject(Rectangle rect) { _Rect = rect; }
	public Vector2 Size => new Vector2(_Rect.width, _Rect.height);
	public Vector2 Position => new Vector2(_Rect.x, _Rect.y);
	public bool IsHoveringOver => 0 <= (Raylib.GetMouseX()-_Rect.x) && (Raylib.GetMouseX()-_Rect.x) <= _Rect.width && 0 <= (Raylib.GetMouseY()-_Rect.y) && (Raylib.GetMouseY()-_Rect.y) <= _Rect.height;
	public virtual void Draw() {}
	public virtual void Update() {}
}