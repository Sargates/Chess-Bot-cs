using Raylib_cs;
using System.Globalization;

namespace ChessBot.Application{
	public class BoardTheme {

		public static Color lightCol = HexToColor("#eed8c0");
        public static Color darkCol = HexToColor("#ab7965");

        public static Color selectedLight = HexToColor("#ecc57b");
        public static Color selectedDark = HexToColor("#c89e50");

        public static Color moveFromLight = HexToColor("#cfac6a");	//	represents moves made by opponent ()
        public static Color moveFromDark = HexToColor("#c59e36");	//	represents moves made by opponent ()

        public static Color moveToLight = HexToColor("#ddd07c");	//	represents moves made by opponent ()
        public static Color moveToDark = HexToColor("#c5ad60");	//	represents moves made by opponent ()

        public static Color legalLight = HexToColor("#59abdd");
        public static Color legalDark = HexToColor("#3e90c3");

        public static Color checkLight = HexToColor("#ea4a4a");
        public static Color checkDark = HexToColor("#cf2727");

        public static Color borderCol = HexToColor("#2c2c2c");

		public static Color HexToColor(string hex) {

			// trim "#" from front of hex
			hex = hex[1..];

			int[] arr = {6, 8};
			if (! arr.Contains<int>(hex.Length)) {
				throw new ArgumentException("Size of hex color argument invalid");
			}
			if (hex.Length == 6) {
				hex += "FF";
			}

			byte r, g, b, a;
			byte.TryParse(hex.AsSpan(0, 2), NumberStyles.HexNumber, null, out r);
			byte.TryParse(hex.AsSpan(2, 2), NumberStyles.HexNumber, null, out g);
			byte.TryParse(hex.AsSpan(4, 2), NumberStyles.HexNumber, null, out b);
			byte.TryParse(hex.AsSpan(6, 2), NumberStyles.HexNumber, null, out a);
			return new Color(r, g, b, a);
		}
	}
}