using Raylib_cs;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;

namespace ChessBot.Application;

public abstract class ScreenObject : IRenderable, IUpdatable {
	public Action LeftPressed = delegate 	{ Console.WriteLine("Left Button Pressed"); };
	public Action LeftReleased = delegate 	{ Console.WriteLine("Left Button Released"); };
	public Action RightPressed = delegate 	{ Console.WriteLine("Right Button Pressed"); };
	public Action RightReleased = delegate 	{ Console.WriteLine("Right Button Released"); };

	private Rectangle _Rect { get; set; }
	public ScreenObject(Rectangle rect) { _Rect = rect; }
	public Vector2 Size => new Vector2(_Rect.width, _Rect.height);
	public Vector2 Position => new Vector2(_Rect.x, _Rect.y);
	public bool IsHoveringOver => 0 <= (Raylib.GetMouseX()-_Rect.x) && (Raylib.GetMouseX()-_Rect.x) <= _Rect.width && 0 <= (Raylib.GetMouseY()-_Rect.y) && (Raylib.GetMouseY()-_Rect.y) <= _Rect.height;
	public virtual void Draw() {}
	public virtual ScreenObject SetCallback(Action callback) {
		LeftPressed = callback;
		return this;
	}
	// public virtual ScreenObject SetCallback(EventHandler callback) { return SetCallback(callback, EventArgs.Empty); }
	public virtual void OnLeftPressed() {
		LeftPressed?.Invoke();
	}
	public virtual void OnLeftReleased() {
		LeftReleased?.Invoke();
	}
	public virtual void OnRightPressed() {
		RightPressed?.Invoke();
	}
	public virtual void OnRightReleased() {
		RightReleased?.Invoke();
	}
	public virtual void Update() {}
}