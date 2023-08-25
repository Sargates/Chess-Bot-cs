using System.Diagnostics.CodeAnalysis;

namespace ChessBot.Engine;
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

	public const int WhitePawn 	 = White | Pawn;	// 1
	public const int WhiteKnight = White | Knight;	// 2
	public const int WhiteBishop = White | Bishop;	// 3
	public const int WhiteRook 	 = White | Rook;	// 4
	public const int WhiteQueen  = White | Queen;	// 5
	public const int WhiteKing 	 = White | King;	// 6

	public const int BlackPawn 	 = Black | Pawn;	// 9
	public const int BlackKnight = Black | Knight;	// 10
	public const int BlackBishop = Black | Bishop;	// 11
	public const int BlackRook 	 = Black | Rook;	// 12
	public const int BlackQueen  = Black | Queen;	// 13
	public const int BlackKing 	 = Black | King;	// 14

	public static readonly int[] pieceArray = new int[] {
		WhitePawn, WhiteKnight, WhiteBishop, WhiteRook, WhiteQueen, WhiteKing,
		BlackPawn, BlackKnight, BlackBishop, BlackRook, BlackQueen, BlackKing
	};

	// Masks
	public const int TypeMask = 0b111;
	public const int ColorMask = 0b1000;

	public int Type => (_Value & TypeMask);
	public int Color => (_Value & ColorMask);
	public int ColorAsBinary => Color>>3; //* Added after the fact to use colors as an index
	public bool IsNone => _Value == None;

	public bool Equals(Piece other) {
		return Type == other.Type && Color == other.Color;
	}

	public static readonly string[] EnumToRepr = {
		"**", "wp", "wN", "wB",
		"wR", "wQ", "wK", "  ",
		"  ", "bp", "bN", "bB",
		"bR", "bQ", "bK", "  ",
	};

	public override string ToString() {
		return $"{EnumToRepr[_Value]}";
	}

	public Piece(int v) { _Value = v; }

	public static implicit operator Piece(int value) {
		return new Piece { _Value = value };
	}

	public static implicit operator int(Piece value) {
		return value._Value;
	}
}
