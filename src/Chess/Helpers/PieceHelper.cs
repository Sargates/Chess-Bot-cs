namespace ChessBot.Helpers {
	public static class PieceHelper {

		public const int None = 0b000;
		public const int Pawn = 0b001;
		public const int Knight = 0b010;
		public const int Bishop = 0b011;
		public const int Rook = 0b100;
		public const int Queen = 0b101;
		public const int King = 0b110;

		public const int White = 0b0000;
		public const int Black = 0b1000;

		// Masks
		public const int TypeMask = 0b111;
		public const int ColorMask = 0b1000;

		public static int GetType(int piece) => (piece & TypeMask);
		public static int GetColor(int piece) => (piece & ColorMask);

		public static readonly string[] EnumToRepr = {
			"  ", "wp", "wN", "wB",
			"wR", "wQ", "wK", "  ",
			"  ", "bp", "bN", "bB",
			"bR", "bQ", "bK", "  ",
		};

		
		
	}
}