using Raylib_cs;
using ChessBot.Helpers;
using System.Numerics;

namespace ChessBot.Application {
	public struct Button : IInteractable {
		public Rectangle _Rect;
		public string text;
		public Color color=ColorHelper.HexToColor("#555555ff");
		public Color highlightColor = ColorHelper.HexToColor("#03adfcff");
		public delegate void ClickCallback();
		public ClickCallback? OnLeftPressed;
		public ClickCallback? OnLeftReleased;
		public ClickCallback? OnRightPressed;
		public ClickCallback? OnRightReleased;
		public struct CallbackArgs {
			public object[] args;
			public ClickCallback callback;
			public CallbackArgs(ClickCallback callback, params object[] args) {
				this.callback = callback;
				this.args = args;
			}
		}

		public Button(Rectangle rect, string text) {
			_Rect = rect;
			this.text = text;
		}
		public Button(Rectangle rect, string text, Color color) {
			_Rect = rect;
			this.text = text;
			this.color = color;
		}
		public Button(Rectangle rect, string text, string color) : this(rect, text, ColorHelper.HexToColor(color)) {}

		public Vector2 Size => new Vector2(_Rect.width, _Rect.height);
		public Vector2 Position => new Vector2(_Rect.x, _Rect.y);

		public bool IsHoveringOver => 0 <= (Raylib.GetMouseX()-_Rect.x) && (Raylib.GetMouseX()-_Rect.x) <= _Rect.width && 0 <= (Raylib.GetMouseY()-_Rect.y) && (Raylib.GetMouseY()-_Rect.y) <= _Rect.height;

		public void Draw() {
			Raylib.DrawRectangleV(Position, Size, this.color);
			if (IsHoveringOver) {
				Raylib.DrawRectangleV(Position, Size, this.highlightColor);
			}

			UIHelper.DrawText(text, Position+(Size/2), 24, 0, Color.WHITE, UIHelper.AlignH.Centre, UIHelper.AlignV.Centre);
		}

		public void Update() {
			if (! this.IsHoveringOver) { return; }
			if (View.IsLeftPressed) {
				OnLeftPressed?.Invoke();
			}
			if (View.IsLeftReleased) {
				OnLeftReleased?.Invoke();
			}
			if (View.IsRightPressed) {
				OnRightPressed?.Invoke();
			}
			if (View.IsRightReleased) {
				OnRightReleased?.Invoke();
			}
		}

		public Button SetCallback(ClickCallback callback) {
			this.OnLeftPressed += callback;
			return this;
		}

		public override string ToString() {
			return $"Box at <{Size}> of size <{Position}>";
		}
	}

}
public struct Settings {

}