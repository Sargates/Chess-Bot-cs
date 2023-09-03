using Raylib_cs;
using System.Linq.Expressions;
using System.Numerics;
using System.Reflection;
using ChessBot.Helpers;

namespace ChessBot.Application;

public abstract class ScreenObject : IRenderable, IUpdatable {
	public Action LeftPressed = delegate 	{ Console.WriteLine("Left Button Pressed"); };
	public Action LeftReleased = delegate 	{ Console.WriteLine("Left Button Released"); };
	public Action RightPressed = delegate 	{ Console.WriteLine("Right Button Pressed"); };
	public Action RightReleased = delegate 	{ Console.WriteLine("Right Button Released"); };
	
	public Color color=ColorHelper.HexToColor("#555555ff");
	public Color highlightColor = ColorHelper.HexToColor("#03adfcff");
	public Color borderColor = ColorHelper.HexToColor("#333333ff");
	public Vector2 TOPLEFT => Position;
	public Vector2 TOPCENTER => Position+new Vector2(Size.X/2, 0);
	public Vector2 TOPRIGHT => Position+new Vector2(Size.X, 0);
	public Vector2 LEFT => Position+new Vector2(0, Size.Y/2);
	public Vector2 CENTER => Position+(Size/2);
	public Vector2 RIGHT => Position+new Vector2(Size.X, Size.Y/2);
	public Vector2 BOTTOMLEFT => Position+new Vector2(0, Size.Y);
	public Vector2 BOTTOMCENTER => Position+new Vector2(Size.X/2, Size.Y);
	public Vector2 BOTTOMRIGHT => Position+Size;

	protected Rectangle _Rect;
	public ScreenObject(Rectangle rect) { _Rect = rect; _position = new Vector2(_Rect.x, _Rect.y); _size = new Vector2(_Rect.width, _Rect.height); }
	private Vector2 _position;
	private Vector2 _size;
	public Vector2 Size {
		get { return _size; }
		set { _size = value; UpdatePositionAndSize(); }
	}
	public Vector2 Position {
		get { return _position; }
		set { _position = value; UpdatePositionAndSize(); }
	}
	public bool IsHoveringOver => 0 <= (Raylib.GetMouseX()-_Rect.x) && (Raylib.GetMouseX()-_Rect.x) <= _Rect.width && 0 <= (Raylib.GetMouseY()-_Rect.y) && (Raylib.GetMouseY()-_Rect.y) <= _Rect.height;
	private void UpdatePositionAndSize() {
		_Rect.x = _position.X;
		_Rect.y = _position.Y;
		_Rect.width = _size.X;
		_Rect.height = _size.Y;
	}
	public virtual ScreenObject SetLeftCallback(Action callback) {
		LeftPressed = callback;
		LeftReleased = delegate{};
		return this;
	}
	public virtual ScreenObject SetRightCallback(Action callback) {
		RightPressed = callback;
		RightReleased = delegate{};
		return this;
	}
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
	public void Draw() { Draw(Position); }
	public virtual void Draw(Vector2 pos) {}
	public void Update() { Update(0.0f); }
	public virtual void Update(float dt) {}
}