using System.Numerics;

namespace ChessBot.Engine;

public static class BitboardHelper {
	// Get index of least significant set bit in given 64bit value. Also clears the bit to zero.
	public static int PopLSB(ref ulong b) {
		int i = BitOperations.TrailingZeroCount(b);
		b &= (b - 1);
		return i;
	}

	public static int PopCount(ulong x) {
		return BitOperations.PopCount(x);
	}

	public static void SetSquare(ref ulong bitboard, int squareIndex) {
		bitboard |= 1ul << squareIndex;
	}

	public static void ClearSquare(ref ulong bitboard, int squareIndex) {
		bitboard &= ~(1ul << squareIndex);
	}


	public static void ToggleSquare(ref ulong bitboard, int squareIndex) {
		bitboard ^= 1ul << squareIndex;
	}

	public static void ToggleSquares(ref ulong bitboard, int squareA, int squareB) {
		bitboard ^= (1ul << squareA | 1ul << squareB);
	}

	public static bool ContainsSquare(ulong bitboard, int square) {
		return ((bitboard >> square) & 1) != 0;
	}
}