using Raylib_cs;
using ChessBot.Helpers;
using System.Numerics;

namespace ChessBot.Application {
	public class Button : ScreenObject {
		public string text;
		public Color color=ColorHelper.HexToColor("#555555ff");
		public Color highlightColor = ColorHelper.HexToColor("#03adfcff");
		public Color borderColor = ColorHelper.HexToColor("#333333ff");
		public int borderThickness;
		// public struct CallbackArgs {
		// 	public object[] args;
		// 	public ClickCallback callback;
		// 	public CallbackArgs(ClickCallback callback, params object[] args) {
		// 		this.callback = callback;
		// 		this.args = args;
		// 	}
		// }

		public Button(Rectangle rect, string text) : base(rect) {
			this.text = text;
		}
		public Button(Rectangle rect, string text, Color color) : base(rect) {
			this.text = text;
			this.color = color;
		}
		public Button(Rectangle rect, string text, Color color, Color borderColor, int borderThickness) : base(rect){
			this.text = text;
			this.color = color;
			this.borderColor = borderColor;
			this.borderThickness = borderThickness;
		}
		public Button(Rectangle rect, string text, string color) : this(rect, text, ColorHelper.HexToColor(color)) {}



		public override void Draw() {
			Raylib.DrawRectangleV(Position, Size, this.color);
			if (IsHoveringOver) {
				Raylib.DrawRectangleV(Position, Size, this.highlightColor);
			}
			// if (borderThickness != 0) {
			// 	Raylib.DrawRectangleLinesEx(_Rect, borderThickness, borderColor);
			// }

			UIHelper.DrawText(text, Position+(Size/2), 24, 0, Color.WHITE, UIHelper.AlignH.Center, UIHelper.AlignV.Center);
		}

		public override void Update() {
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