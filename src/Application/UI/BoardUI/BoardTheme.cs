using Raylib_cs;
using ChessBot.Helpers;

namespace ChessBot.Application;
public class BoardTheme {

	public static Color lightCol = ColorHelper.HexToColor("#eed8c0");
	public static Color darkCol = ColorHelper.HexToColor("#ab7965");

	public static Color selectedLight = ColorHelper.HexToColor("#ec7e6a");
	public static Color selectedDark = ColorHelper.HexToColor("#d46c51");
	public static Color selectedHighlight = ColorHelper.HexToColor("#ff3b0abf");

	public static Color movedFromLight = ColorHelper.HexToColor("#cfac6a");	//* represents moves made by opponent ()
	public static Color movedFromDark = ColorHelper.HexToColor("#c59e36");	//* represents moves made by opponent ()
	public static Color movedFromHighlight = ColorHelper.HexToColor("#daa925bf");	//* represents moves made by opponent ()

	public static Color movedToLight = ColorHelper.HexToColor("#ddd07c");	//* represents moves made by opponent ()
	public static Color movedToDark = ColorHelper.HexToColor("#c5ad60");	//* represents moves made by opponent ()
	public static Color movedToHighlight = ColorHelper.HexToColor("#ffce2fbf");	//* represents moves made by opponent ()

	public static Color legalLight = ColorHelper.HexToColor("#59abdd");
	public static Color legalDark = ColorHelper.HexToColor("#3e90c3");
	public static Color legalHighlight = ColorHelper.HexToColor("#17a6ffbf");

	public static Color checkLight = ColorHelper.HexToColor("#ea4a4a");
	public static Color checkDark = ColorHelper.HexToColor("#cf2727");

	public static Color borderCol = ColorHelper.HexToColor("#2c2c2c");
}