using Raylib_cs;
using ChessBot.Helpers;
using System.Numerics;

namespace ChessBot.Application {
	public struct Button : IInteractable {
		public Rectangle _Rect;
		public string text;
		public Color color=ColorHelper.HexToColor("#555555ff");
		public Color highlightColor = ColorHelper.HexToColor("#03adfcff");
		public delegate void ClickHandler();
		public ClickHandler? OnLeftPressed;
		public ClickHandler? OnLeftReleased;
		public ClickHandler? OnRightPressed;
		public ClickHandler? OnRightReleased;

		public Button(Rectangle rect, string text) {
			_Rect = rect;
			this.text = text;
		}
		public Button(Rectangle rect, string text, Color color) {
			_Rect = rect;
			this.text = text;
			this.color = color;
		}

		public Vector2 Size => new Vector2(_Rect.width, _Rect.height);
		public Vector2 Position => new Vector2(_Rect.x, _Rect.y);

		public bool IsHoveringOver => 0 <= (Raylib.GetMouseX()-_Rect.x) && (Raylib.GetMouseX()-_Rect.x) <= _Rect.width && 0 <= (Raylib.GetMouseY()-_Rect.y) && (Raylib.GetMouseY()-_Rect.y) <= _Rect.height;

		public void Draw() {
			Raylib.DrawRectangle((int)_Rect.x, (int)_Rect.y, (int)_Rect.width, (int)_Rect.height, this.color);
			if (IsHoveringOver) {
				Raylib.DrawRectangle((int)_Rect.x, (int)_Rect.y, (int)_Rect.width, (int)_Rect.height, this.highlightColor);
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

		public override string ToString() {
			return $"Box at <{Size}> of size <{Position}>";
		}
	}

}
public struct Settings {

}