using Raylib_cs;
using ChessBot.Helpers;

namespace ChessBot.Application;
public class BoardTheme {

	public static Color lightCol = ColorHelper.HexToColor("#eed8c0");
	public static Color darkCol = ColorHelper.HexToColor("#ab7965");

	public static Color selectedHighlight = ColorHelper.HexToColor("#ff3b0abf");

	public static Color movedFromHighlight = ColorHelper.HexToColor("#daa925bf");	//* represents moves made by opponent ()

	public static Color movedToHighlight = ColorHelper.HexToColor("#ffce2fbf");	//* represents moves made by opponent ()

	public static Color legalHighlight = ColorHelper.HexToColor("#17a6ffbf");
	public static Color arrowOutlineHighlight = ColorHelper.HexToColor("#ffa927b6");

	public static Color checkLight = ColorHelper.HexToColor("#ea4a4a");
	public static Color checkDark = ColorHelper.HexToColor("#cf2727");

	public static Color borderCol = ColorHelper.HexToColor("#2c2c2c");
}