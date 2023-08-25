using ChessBot.Application;
using ChessBot.Engine;

namespace ChessBot.Helpers;
public static class Zobrist {
	//* Comments and implementation taken from SebLague
	// Random numbers are generated for each aspect of the game state, and are used for calculating the hash:

	// piece type, colour, square index
	public static readonly ulong[,] piecesArray = new ulong[Piece.BlackKing+1, 64]; //* Black king is the largest enumeration of piece types
	// Each player has 4 possible castling right states: none, queenside, kingside, both.
	// So, taking both sides into account, there are 16 possible states.
	public static readonly ulong[] castlingRights = new ulong[16];
	// En passant file (0 = no ep).
	// Rank does not need to be specified since side to move is included in key
	public static readonly ulong[] enPassantFile = new ulong[9];
	public static readonly ulong sideToMove;
	static Zobrist() {
		const int seed = 29426028;
		System.Random rng = new System.Random(seed);

		// Console.WriteLine(piecesArray.Length);
		for (int squareIndex = 0; squareIndex < 64; squareIndex++) {
			for (int piece=0; piece<Piece.BlackKing+1; piece++ ) {
				//* `piece` goes from 0 to 15, `piecesArray` will have an entry for every piece type at a given square
				//* there will be some elements that are never used but that's fine
				piecesArray[piece, squareIndex] = RandomUnsigned64BitNumber(rng);
			}
		}


		for (int i = 0; i < castlingRights.Length; i++) {
			castlingRights[i] = RandomUnsigned64BitNumber(rng);
		}

		for (int i = 0; i < enPassantFile.Length; i++) {
			enPassantFile[i] = i == 0 ? 0 : RandomUnsigned64BitNumber(rng);
		}

		sideToMove = RandomUnsigned64BitNumber(rng);
	}

	// Calculate zobrist key from current board position.
	// NOTE: this function is slow and should only be used when the board is initially set up from fen.
	// During search, the key should be updated incrementally instead.
	public static ulong CalculateZobristKey(Board board) {
		ulong zobristKey = 0;

		for (int squareIndex = 0; squareIndex < 64; squareIndex++)
		{
			Piece piece = board.GetSquare(squareIndex);
			if (piece.Type == Piece.None) continue;

			zobristKey ^= piecesArray[piece, squareIndex];
		}

		zobristKey ^= enPassantFile[(board.currentState.enPassantIndex == -1) ? 8 : (board.currentState.enPassantIndex%8)]; //* Mod 8 because I keep track of it as an index and not a file, +1 because it's a file the index could be -1

		if (board.ActiveColor == Piece.Black)
		{
			zobristKey ^= sideToMove;
		}

		zobristKey ^= castlingRights[board.currentState.castleRights];

		return zobristKey;
	}

	static ulong RandomUnsigned64BitNumber(System.Random rng) { //* Taken from SebLague
		byte[] buffer = new byte[8];
		rng.NextBytes(buffer);
		return System.BitConverter.ToUInt64(buffer, 0);
	}
}