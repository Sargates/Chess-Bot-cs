namespace ChessBot.Engine {
	public struct Piece {
		private int _Value;

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

		public int Type => (_Value & TypeMask);
		public int Color => (_Value & ColorMask);
		public bool IsNull => _Value == None;

		public static readonly string[] EnumToRepr = {
			"  ", "wp", "wN", "wB",
			"wR", "wQ", "wK", "  ",
			"  ", "bp", "bN", "bB",
			"bR", "bQ", "bK", "  ",
		};

		public static implicit operator Piece(int value) {
			return new Piece { _Value = value };
		}

		public static implicit operator int(Piece value) {
			return value._Value;
		}

	}
}