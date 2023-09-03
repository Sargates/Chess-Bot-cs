namespace ChessBot.Engine;

public struct Bitboard {
	private ulong _Value = 0;
	public static Bitboard Empty { get { return new Bitboard(); } }
	public static Bitboard Filled { get { return new Bitboard(0xFFFFFFFFFFFFFFFF); } }
	public Bitboard(ulong value=0) { _Value = value; }

	
	public bool IsSquareSet(int square) {
		return (1 == (1 & (_Value>>square)));
	}

	public void SetSquare(int value, int squareIndex) {
		_Value |= 1ul << squareIndex;
	}

	public void ClearSquare(int value, int squareIndex) {
		_Value &= ~(1ul << squareIndex);
	}

	public void ToggleSquare(int value, int squareIndex) {
		_Value ^= 1ul << squareIndex;
	}

	public void ToggleSquares(int value, int squareA, int squareB) {
		_Value ^= (1ul << squareA | 1ul << squareB);
	}

}