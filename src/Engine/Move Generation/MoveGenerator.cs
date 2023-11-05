using ChessBot.Helpers;
using ChessBot.Application;
namespace ChessBot.Engine;
public class MoveGenerator {

	public static readonly Coord[] kingSideDeltas = { new Coord(1, 0), new Coord(2, 0) };
	public static readonly Coord[] queenSideDeltas = { new Coord(-1, 0), new Coord(-2, 0), new Coord(-3, 0) };

	public static int[] pinsBySquare = new int[64];
	public static (int checkerPos, int dirFromKing)[][] currentChecks = { new (int squareIndex, int dirFromKing)[0], new (int squareIndex, int dirFromKing)[0] }; // white, black respectively

	public static Move[] GetPawnMoves(Board board, int index) {
		List<Move> moves = new List<Move>();
		ulong final = 0;
		Piece piece = board.GetSquare(index);
		final |= PrecomputedMoveData.GetPawnAttacks(piece.ColorAsBinary, index) ^ board.AllPiecesBitboard & PrecomputedMoveData.GetPawnAttacks(piece.ColorAsBinary, index);
		final |= PrecomputedMoveData.GetPawnAttacks(piece.ColorAsBinary, index) & board.EnemyPieces(piece.Color);
		return moves.ToArray();
	}
	public static Move[] GetKnightMoves(Board board, int index) {
		List<Move> moves = new List<Move>();
		// ulong final = 0;
		return moves.ToArray();
	}
	public static Move[] GetBishopMoves(Board board, int index) {
		List<Move> moves = new List<Move>();
		// ulong final = 0;
		return moves.ToArray();
	}
	public static Move[] GetRookMoves(Board board, int index) {
		List<Move> moves = new List<Move>();
		// ulong final = 0;
		return moves.ToArray();
	}
	public static Move[] GetQueenMoves(Board board, int index) {
		List<Move> moves = new List<Move>();
		Move[] rooks = GetRookMoves(board, index);
		Move[] bishops = GetBishopMoves(board, index);
		return rooks.Concat(bishops).ToArray();
	}
	public static Move[] GetKingMoves(Board board, int index) {
		List<Move> moves = new List<Move>();
		// ulong final = 0;
		return moves.ToArray();
	}
	public static Move[] GetMoves(Board board, int index) {


		Piece piece = board.GetSquare(index);

		//* Space is invalid, no moves, this is for Computer players not causing the program to crash
		if (piece.IsNone) { return new Move[0]; }



		int kingPos = piece.Color == Piece.White ? board.whiteKingPos : board.blackKingPos;
		var checkData = GetCheckData(board, kingPos, piece.Color);
		bool isInCheck = checkData.Item1;
		pinsBySquare = new int[64];
		foreach ((int squareIndex, int dirFromKing) pin in checkData.Item2) { pinsBySquare[pin.squareIndex] = pin.dirFromKing; }
		currentChecks[piece.ColorAsBinary] = checkData.Item3;



		Move[] moves = piece.Type switch {
			Piece.Pawn => GetPawnMoves(board, index),
			Piece.Knight => GetKnightMoves(board, index),
			Piece.Bishop => GetBishopMoves(board, index),
			Piece.Rook => GetRookMoves(board, index),
			Piece.Queen => GetQueenMoves(board, index),
			Piece.King => GetKingMoves(board, index),
			_ => new Move[0]
		};

		return moves.ToArray();
	}
	public static Move[] GetAllMoves(Board board, int color, bool sort=false) {
		List<Move> totalMoves = new List<Move>();


		foreach (Piece piece in (color == Piece.White ? Piece.pieceArray[0..6] : Piece.pieceArray[6..12])) {
			ulong bb = board.GetPieceBBoard(piece);
			while (bb != 0) { totalMoves.AddRange(GetMoves(board, BitboardHelper.PopLSB(ref bb))); }
		}

		if (sort) {
			return totalMoves.OrderBy(x => (int)board.GetSquare(x.StartSquare)).ThenBy(x => (int)x.Flag).ToArray();
		}

		return totalMoves.ToArray();
	}
	public static bool IsSquareAttacked(Board board, int index, int color) {
		return GetCheckData(board, index, color).Item1;
	}

	public static (bool, (int, int)[], (int, int)[]) GetCheckData(Board board, int index, int color) {
		//* Credits to Eddie Sharick for this algorithm (adapted from python)
		//* https://youtu.be/coAOXj6ZnSI

		List<(int, int)> pins = new List<(int, int)>();
		List<(int, int)> checks = new List<(int, int)>();
		bool inCheck = false;

		int[] directions = { -8, 1, 8, -1 , 9, 7, -9, -7 };
		int start = index;

		for (int j=0; j<directions.Length; j++) {

			Coord delta = BoardHelper.GetAbsoluteDirection(directions[j]);

			(int, int) possiblePin = (0, 0);
			for (int i = 1; i < 8; i++) {
				if (! (new Coord(start) + (delta*i)).IsInBounds()) { // TODO: Change for compat. with PrecomputedMoveData
					break;
				}

				int testedPos = start + directions[j] * i;

				Piece attackingSquare = board.GetSquare(testedPos);

				if (attackingSquare.IsNone) {
					continue;
				}

				if (attackingSquare.Color == color && attackingSquare.Type != Piece.King) {
					if (possiblePin == (0, 0)) {
						possiblePin = (testedPos, directions[j]);
					}
					else {
						break;
					}
				} else if (attackingSquare.Color != color) {
					int type = attackingSquare.Type;
					if ((0 <= j && j <= 3 && type == Piece.Rook) ||
						(4 <= j && j <= 7 && type == Piece.Bishop) ||
						(i == 1 && type == Piece.Pawn && ((color == Piece.White && 4 <= j && j <= 5) || (color == Piece.Black && 6 <= j && j <= 7))) ||
						(type == Piece.Queen) || (i == 1 && type == Piece.King)) {
						if (possiblePin == (0, 0)) {
							inCheck = true;
							checks.Add((testedPos, directions[j]));
							break;
						}
						else {
							pins.Add(possiblePin);
							break;
						}
					}
					else { break; }
				}
			}
		}

		int[] knightMoves = { 6, 15, 17, 10, -6, -15, -17, -10 };
		foreach (int direction in knightMoves) { // TODO: Change for compat. with PrecomputedMoveData
			Coord newPos = new Coord(start) + (new Coord(direction+26) - new Coord(26));
			if (! newPos.IsInBounds()) {
				continue;
			}


			Piece endPiece = board.GetSquare(newPos.SquareIndex);
			if (endPiece.IsNone) {
				continue;
			}

			if (endPiece.Color != color && endPiece.Type == Piece.Knight) {
				inCheck = true;
				checks.Add((newPos.SquareIndex, direction));
			}
		}

		return (inCheck, pins.ToArray(), checks.ToArray());
	}
}

