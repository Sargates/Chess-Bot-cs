using Raylib_cs;
using ChessBot.Helpers;
using System.Numerics;

namespace ChessBot.Application;

public class Button : ScreenObject {
	public string text="Button";
	public int borderThickness;

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

	public override void Draw(Vector2 pos) {
		Raylib.DrawRectangleRec(_Rect, this.color);
		if (IsHoveringOver) { Raylib.DrawRectangleRec(_Rect, this.highlightColor); }
		UIHelper.DrawText(text, pos+(Size/2), 24, 0, Color.WHITE, UIHelper.AlignH.Center, UIHelper.AlignV.Center);
	}

	public override void Update(float dt) {
		if (! IsHoveringOver) { return; }
		if (MainController.Instance.IsLeftPressed) {
			OnLeftPressed();
		}
		if (MainController.Instance.IsLeftReleased) {
			OnLeftReleased();
		}
		if (MainController.Instance.IsRightPressed) {
			OnRightPressed();
		}
		if (MainController.Instance.IsRightReleased) {
			OnRightReleased();
		}
	}

	public override string ToString() {
		return $"Box at <{Size}> of size <{Position}>";
	}
	}
public struct Settings {

}