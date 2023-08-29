using System.Numerics;

namespace ChessBot.Helpers;

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
	public static bool IsSquareSet(ulong x, int square) {
		return (1 == (1 & (x>>square)));

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
	public static ulong[,] GetBitboardCopy(ulong[,] array) {
		int width = array.GetLength(0);
		int height = array.GetLength(1);
		ulong[,] copy = new ulong[width, height];

		for (int w = 0; w < width; w++) {
			for (int h = 0; h < height; h++) {
				copy[w, h] = array[w, h];
			}
		}

		return copy;
	}
	// REF: https://stackoverflow.com/a/66696857
	public static bool SequenceEquals(this ulong[,] a, ulong[,] b) => a.Rank == b.Rank
    && Enumerable.Range(0, a.Rank).All(d=> a.GetLength(d) == b.GetLength(d))
    && a.Cast<ulong>().SequenceEqual(b.Cast<ulong>());
}