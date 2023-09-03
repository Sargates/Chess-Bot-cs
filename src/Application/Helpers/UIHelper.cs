using Raylib_cs;
using System;
using System.IO;
using System.Numerics;

namespace ChessBot.Helpers;
public static class UIHelper {
	static readonly bool SDF_Enabled = true;
	const string fontName = "OPENSANS-SEMIBOLD.TTF";
	const int referenceResolution = 1920;

	static Font font;
	static Font fontSdf;
	static Shader shader;

	public enum AlignH
	{
		Left,
		Center,
		Right
	}
	public enum AlignV
	{
		Top,
		Center,
		Bottom
	}

	static UIHelper()
	{
		if (SDF_Enabled)
		{
			unsafe
			{
				const int baseSize = 64;
				uint fileSize = 0;
				var fileData = Raylib.LoadFileData(FileHelper.GetResourcePath("Fonts", fontName), ref fileSize);
				Font fontSdf = default;
				fontSdf.baseSize = baseSize;
				fontSdf.glyphCount = 95;
				fontSdf.glyphs = Raylib.LoadFontData(fileData, (int)fileSize, baseSize, null, 0, FontType.FONT_SDF);

				Image atlas = Raylib.GenImageFontAtlas(fontSdf.glyphs, &fontSdf.recs, 95, baseSize, 0, 1);
				fontSdf.texture = Raylib.LoadTextureFromImage(atlas);
				Raylib.UnloadImage(atlas);
				Raylib.UnloadFileData(fileData);

				Raylib.SetTextureFilter(fontSdf.texture, TextureFilter.TEXTURE_FILTER_BILINEAR);
				UIHelper.fontSdf = fontSdf;
			}
			shader = Raylib.LoadShader("", FileHelper.GetResourcePath("Fonts", "sdf.fs"));
		}
		font = Raylib.LoadFontEx(FileHelper.GetResourcePath("Fonts", fontName), 128, null, 0);
	}

	public static void DrawText(string text, Vector2 pos, int size, int spacing, Color col, AlignH alignH = AlignH.Left, AlignV alignV = AlignV.Center)
	{
		Vector2 boundSize = Raylib.MeasureTextEx(font, text, size, spacing);
		float offsetX = alignH == AlignH.Left ? 0 : (alignH == AlignH.Center ? -boundSize.X / 2 : -boundSize.X);
		float offsetY = alignV == AlignV.Top ? 0 : (alignV == AlignV.Center ? -boundSize.Y / 2 : -boundSize.Y);
		Vector2 offset = new(offsetX, offsetY);

		if (SDF_Enabled) {
			Raylib.BeginShaderMode(shader);
			Raylib.DrawTextEx(fontSdf, text, pos + offset, size, spacing, col);
			Raylib.EndShaderMode();
		} else {
			Raylib.DrawTextEx(font, text, pos + offset, size, spacing, col);
		}
	}

	static bool MouseInRect(Rectangle rec) {
		Vector2 mousePos = Raylib.GetMousePosition();
		return mousePos.X >= rec.x && mousePos.Y >= rec.y && mousePos.X <= rec.x + rec.width && mousePos.Y <= rec.y + rec.height;
	}

	public static float Scale(float val, int referenceResolution = referenceResolution) {
		return Raylib.GetScreenWidth() / (float)referenceResolution * val;
	}

	public static int ScaleInt(int val, int referenceResolution = referenceResolution) {
		return (int)Math.Round(Raylib.GetScreenWidth() / (float)referenceResolution * val);
	}

	public static Vector2 Scale(Vector2 vec, int referenceResolution = referenceResolution) {
		float x = Scale(vec.X, referenceResolution);
		float y = Scale(vec.Y, referenceResolution);
		return new Vector2(x, y);
	}

	public static void DrawRectangleCentered(Vector2 position, Vector2 size, Color color) {
		Raylib.DrawRectangleV(position-size/2, size, color);
	}

	public static void DrawRectangleAsBorder(Rectangle rect, Color color, int w) {
		Vector2 width = new Vector2(w);
		Raylib.DrawRectangleV(new Vector2(rect.x, rect.y)-width, new Vector2(rect.width, rect.height)+2*width, color);
	}

	public static void Release() {
		Raylib.UnloadFont(font);
		if (SDF_Enabled) {
			Raylib.UnloadFont(fontSdf);
			Raylib.UnloadShader(shader);
		}
	}
}