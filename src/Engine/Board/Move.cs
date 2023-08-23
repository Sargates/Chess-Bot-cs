using ChessBot.Helpers;
namespace ChessBot.Engine;
public readonly struct Move {
	

	// 16bit move value // Copied from SebLague
	public readonly ushort moveValue;

	// Flags // Copied from SebLague
	public const int NoFlag = 0b0000;
	public const int EnPassantCaptureFlag = 0b0001;
	public const int CastleFlag = 0b0010;
	public const int PawnTwoUpFlag = 0b0011;

	public const int PromoteToQueenFlag = 0b0100;
	public const int PromoteToKnightFlag = 0b0101;
	public const int PromoteToRookFlag = 0b0110;
	public const int PromoteToBishopFlag = 0b0111;

	// Data masks // Copied from SebLague
	public const int startSquareMask = 0b0000000000111111;
	public const int targetSquareMask = 0b0000111111000000;
	public const int flagMask = 0b1111000000000000;


	public Move(ushort moveValue) {
		this.moveValue = moveValue;
	}

	public Move(int startSquare, int targetSquare) {
		this.moveValue = (ushort) ((startSquare & 0x3F) | (targetSquare & 0x3F) << 6);
	}

	public Move(int startSquare, int targetSquare, int flag) {
		this.moveValue = (ushort) ((startSquare & 0x3F) | (targetSquare & 0x3F) << 6 | (flag & 0xF) << 12);
	}

	public override bool Equals(object? obj) {
		if (ReferenceEquals(null, obj)) return false;
		// if (ReferenceEquals(this, obj)) return true;
		if (obj.GetType() != this.GetType()) return false;
			return Equals((Move) obj);
	}

	public override int GetHashCode() => base.GetHashCode();

	public bool Equals(Move other) {
		return (this.moveValue & (targetSquareMask | startSquareMask)) == (other.moveValue & (targetSquareMask | startSquareMask));
	}

	public override string ToString() {
		if (IsNull) return "null";
		string promoChar = Flag switch {
			PromoteToQueenFlag => "q",
			PromoteToBishopFlag => "b",
			PromoteToKnightFlag => "n",
			PromoteToRookFlag => "r",
			_ => ""
		};
		return $"{BoardHelper.IndexToSquareName(StartSquare)}{BoardHelper.IndexToSquareName(TargetSquare)}{promoChar}";
	}

	public static bool operator ==(Move a, Move b) => (a.moveValue & ~flagMask) == (b.moveValue & ~flagMask);
	public static bool operator !=(Move a, Move b) => (a.moveValue & ~flagMask) != (b.moveValue & ~flagMask);

	public bool IsPromotion => Flag >= PromoteToQueenFlag;

	public bool IsNull => moveValue == 0;

	public int StartSquare => moveValue & startSquareMask;
	public int TargetSquare => (moveValue & targetSquareMask) >> 6;
	public int Flag => moveValue >> 12;

	public static Move NullMove => new Move(0);
}