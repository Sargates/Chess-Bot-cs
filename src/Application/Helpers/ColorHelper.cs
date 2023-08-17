using Raylib_cs;
using System.Globalization;

namespace ChessBot.Helpers;
	static class ColorHelper {
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