using ChessBot.Helpers;
using ChessBot.Application;
namespace ChessBot.Engine {
	public class MoveGenerator {

		public static readonly Coord[] kingSideDeltas = { new Coord(1, 0), new Coord(2, 0) };
		public static readonly Coord[] queenSideDeltas = { new Coord(-1, 0), new Coord(-2, 0), new Coord(-3, 0) };

		public static List<Move> GetPawnMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			Piece piece = board.GetSquare(index);

			// `color` is the color of `piece`, if `piece` is a null piece, get `activeColor`
			int color = piece == Piece.None ? board.activeColor : piece.Color;


			// !!! Game crashes when pawn is at edge of board, need to find way to check these pieces, (precomputed move data?)


			Coord coord = new Coord(index);
			Coord delta = new Coord(board.forwardDir(color));
			Coord newPos = coord+delta;
			Piece pawnOneUp = board.GetSquare(newPos.SquareIndex);
			if (newPos.IsInBounds() && pawnOneUp == Piece.None) { // ! add pinning capabilities
				moves.Add(new Move(index, newPos.SquareIndex, (BoardHelper.RankIndex(index) == (color == Piece.White ? 6 : 1)) ? Move.PromoteToQueenFlag : Move.NoFlag));
				
				if (BoardHelper.RankIndex(index) == (color == Piece.White ? 1 : 6) && board.GetSquare(index + 2*board.forwardDir(color)) == Piece.None) {
					moves.Add(new Move(index, index + 2*board.forwardDir(color), Move.PawnTwoUpFlag));
				}
			}

			delta = new Coord(+1, Math.Sign(board.forwardDir(color)));
			newPos = coord+delta;
			Piece pawnAttackPositive = board.GetSquare(newPos.SquareIndex);
			if (newPos.IsInBounds()) {
				if ((pawnAttackPositive.Type != Piece.None) && pawnAttackPositive.Color != color ) { // ! add pinning capabilities
					moves.Add(new Move(index, newPos.SquareIndex, (BoardHelper.RankIndex(index) == (color == Piece.White ? 6 : 1)) ? Move.PromoteToQueenFlag : Move.NoFlag));
				} else if ((newPos.SquareIndex == board.enPassantIndex && board.GetSquare(index + 1) != color)) {
					moves.Add(new Move(index, newPos.SquareIndex, Move.EnPassantCaptureFlag));
				}
			}

			delta = new Coord(-1, Math.Sign(board.forwardDir(color)));
			newPos = coord+delta;
			Piece pawnAttackNegative = board.GetSquare(newPos.SquareIndex);
			if (newPos.IsInBounds()) {
				if ((pawnAttackNegative.Type != Piece.None) && pawnAttackNegative.Color != color ) { // ! add pinning capabilities
					moves.Add(new Move(index, newPos.SquareIndex, (BoardHelper.RankIndex(index) == (color == Piece.White ? 6 : 1)) ? Move.PromoteToQueenFlag : Move.NoFlag));
				} else if ((newPos.SquareIndex == board.enPassantIndex && board.GetSquare(index - 1) != color)) {
					moves.Add(new Move(index, newPos.SquareIndex, Move.EnPassantCaptureFlag));
				}
			}

			return moves;
		}

		public static List<Move> GetKnightMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			Piece piece = board.GetSquare(index);
			Coord coord = new Coord(index);
			foreach ((int x, int y) in new (int x, int y)[] { (-2, 1), (-1, 2), (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1) }) {
				Coord delta = new Coord(x, y);
				Coord newPos = coord + delta;
				if (! newPos.IsInBounds()) { continue; } // Passes guard clause if in bounds

				Piece newPiece = board.GetSquare(newPos.SquareIndex);

				if (newPiece.Type == Piece.None || newPiece.Color != piece.Color) { // ! add pinning capabilities
					moves.Add(new Move(index, newPos.SquareIndex));
				}
			}

			return moves;
		}
		public static List<Move> GetBishopMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			Piece piece = board.GetSquare(index);
			Coord coord = new Coord(index);
			// Iterate over each direction in moveset and scale by number [1-7]
			foreach ((int x, int y) in new (int x, int y)[] { (1, 1), (1, -1), (-1, -1), (-1, 1) }) {
				for (int i=1;i<8;i++) {
					Coord delta = new Coord(x, y)*i;
					Coord newPos = coord + delta;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds
					

					Piece newPiece = board.GetSquare(newPos.SquareIndex);
					
					
					if (newPiece.Type == Piece.None) { // ! add pinning capabilities
						moves.Add(new Move(index, newPos.SquareIndex));
						continue;
					} else if (newPiece.Color != piece.Color) {
						moves.Add(new Move(index, newPos.SquareIndex));
						break;
					}
					break;
				}
			}

			return moves;
		}
		public static List<Move> GetRookMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			Piece piece = board.GetSquare(index);
			Coord coord = new Coord(index);
			// Iterate over each direction in moveset and scale by number [1-7]
			foreach ((int x, int y) in new (int x, int y)[] { (0, 1), (1, 0), (0, -1), (-1, 0) }) {
				for (int i=1;i<8;i++) {
					Coord delta = new Coord(x, y)*i;
					Coord newPos = coord + delta;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds
					

					Piece newPiece = board.GetSquare(newPos.SquareIndex);
					
					
					if (newPiece.Type == Piece.None) { // ! add pinning capabilities
						moves.Add(new Move(index, newPos.SquareIndex));
						continue;
					} else if (newPiece.Color != piece.Color) {
						moves.Add(new Move(index, newPos.SquareIndex));
						break;
					}
					break;
				}
			}

			return moves;
		}
		public static List<Move> GetQueenMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			Piece piece = board.GetSquare(index);
			Coord coord = new Coord(index);

			// Iterate over each direction in moveset and scale by number [1-7]
			foreach ((int x, int y) in new (int x, int y)[] { (1, 1), (1, -1), (-1, -1), (-1, 1), (0, 1), (1, 0), (0, -1), (-1, 0) }) {
				for (int i=1;i<8;i++) {
					Coord delta = new Coord(x, y)*i;
					Coord newPos = coord + delta;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds


					Piece newPiece = board.GetSquare(newPos.SquareIndex);

					
					if (newPiece.Type == Piece.None) { // ! add pinning capabilities
						moves.Add(new Move(index, newPos.SquareIndex));
						continue;
					} else if (newPiece.Color != piece.Color) {
						moves.Add(new Move(index, newPos.SquareIndex));
						break;
					}
					break;
				}
			}

			return moves;
		}
		public static List<Move> GetKingMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			Piece piece = board.GetSquare(index);
			Coord coord = new Coord(index);
			foreach ((int x, int y) in new (int x, int y)[] { (1, 1), (1, -1), (-1, -1), (-1, 1), (0, 1), (1, 0), (0, -1), (-1, 0) }) {
				Coord delta = new Coord(x, y);
				Coord newPos = coord + delta;
				if (! newPos.IsInBounds()) { continue; } // Passes guard clause if in bounds

				Piece newPiece = board.GetSquare(newPos.SquareIndex);

				if (newPiece.Type == Piece.None || newPiece.Color != piece.Color) { // ! add pinning capabilities
					moves.Add(new Move(index, newPos.SquareIndex));
				}
			}

			int flagKing = piece.Color == Piece.White ? Fen.whiteKingCastle : Fen.blackKingCastle;
			int flagQueen = piece.Color == Piece.White ? Fen.whiteQueenCastle : Fen.blackQueenCastle;
			bool kingSide = (board.currentFen.castlePrivsBin & flagKing) == flagKing;
			bool queenSide = (board.currentFen.castlePrivsBin & flagQueen) == flagQueen;

			//! Handle Double checks

			// var checkData = GetCheckData(board, index, piece.Color);
			// bool isInCheck = checkData.Item1;
			// List<(int, int)> gaming = checkData.Item2;
			// List<(int, int)> gaming2  = checkData.Item3;

			if (kingSide) {
				for (int i=0; i<kingSideDeltas.Length; i++) {
					Coord newPos = coord + kingSideDeltas[i];
					// ConsoleHelper.WriteLine($"King side, {newPos.SquareIndex}");
					if (IsSquareAttacked(board, newPos.SquareIndex, piece.Color)) { kingSide = false; break; }
					if (board.GetSquare(newPos.SquareIndex) != Piece.None) { kingSide = false; break; }
				}
			}			

			if (queenSide) {
				for (int i=0; i<queenSideDeltas.Length; i++) {
					Coord newPos = coord + queenSideDeltas[i];
					// ConsoleHelper.WriteLine($"Queen side, {newPos.SquareIndex}");
					if (i != 2) { if (IsSquareAttacked(board, newPos.SquareIndex, piece.Color)) { queenSide = false; break; } }
					if (board.GetSquare(newPos.SquareIndex) != Piece.None) { queenSide = false; break; }
				}
			}

			//* Check king interval for checks
			//* Check king interval for clearance
			if (kingSide) {
				Coord newPos = new Coord(index + 2);
				moves.Add(new Move(index, newPos.SquareIndex, Move.CastleFlag));	
			}
			//* Check king interval for checks
			//* Check king interval for clearance
			if (queenSide) {
				Coord newPos = new Coord(index - 2);
				moves.Add(new Move(index, newPos.SquareIndex, Move.CastleFlag));
			}

			return moves;
		}


		// TODO: Add method to sum number of moves to an given depth (recursive? iterative?)
		public static Move[] GetMoves(Board board, int index) { // ! check edgecases

			// !!! Add pinning capabilities
			// !!! Make sure moving in opposite direction of pin doesnt work; moving in same direction as pin does

			Piece piece = board.GetSquare(index);

			// ConsoleHelper.WriteLine($"PieceEnum = {Convert.ToString(piece, 2)}");

			List<Move> moves = piece.Type switch {
				Piece.Pawn => GetPawnMoves(board, index),
				Piece.Knight => GetKnightMoves(board, index),
				Piece.Bishop => GetBishopMoves(board, index),
				Piece.Rook => GetRookMoves(board, index),
				Piece.Queen => GetQueenMoves(board, index),
				Piece.King => GetKingMoves(board, index),
				_ => throw new Exception("Invalid piece type")
			};

			for (int i=moves.Count-1; i>-1; i--) {
				Move move = moves[i];
				board.MakeMove(move, true);
				int kingPos = piece.Color == Piece.White ? board.whiteKingPos : board.blackKingPos;
				var checkData = GetCheckData(board, kingPos, piece.Color);
				bool isInCheck = checkData.Item1;
				List<(int, int)> gaming = checkData.Item2;
				List<(int, int)> gaming2  = checkData.Item3;
				// Console.WriteLine($"{move}, {isInCheck}");
				board.UpdateFromState();
				if (isInCheck) { moves.RemoveAt(i); }
			}


			// ConsoleHelper.WriteLine($"{Piece.EnumToRepr[piece]}");
			// foreach (Move move in moves) {
			// 	ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
			// }

			return moves.ToArray();
		}

		public static bool IsSquareAttacked(Board board, int index, int color) {
			return GetCheckData(board, index, color).Item1;
		}


		public static (bool, List<(int, int)>, List<(int, int)>) GetCheckData(Board board, int index, int color) {

			List<(int, int)> pins = new List<(int, int)>();
			List<(int, int)> checks = new List<(int, int)>();
			bool inCheck = false;

			Coord[] directions = { new Coord(0, -1), new Coord(1, 0), new Coord(0, 1), new Coord(-1, 0), new Coord(1, 1), new Coord(-1, 1), new Coord(-1, -1), new Coord(1, -1) };
			Coord start = new Coord(index);

			for (int j=0; j<directions.Length; j++) {

				Coord delta = directions[j];

				(int, int) possiblePin = (0, 0);
				for (int i = 1; i < 8; i++) {
					if (! (start + delta*i).IsInBounds()) {
						break;
					}

					int testedPos = (start + delta * i).SquareIndex;

					Piece attackingSquare = board.GetSquare(testedPos);

					if (attackingSquare.IsNull) {
						continue;
					}

					if (attackingSquare.Color == color && attackingSquare.Type != Piece.King) {
						if (possiblePin == (0, 0)) {
							possiblePin = (testedPos, delta.SquareIndex);
						}
						else {
							break;
						}
					}
					else if (attackingSquare.Color != color) {
						int type = attackingSquare.Type;
						if ((0 <= j && j <= 3 && type == Piece.Rook) ||
							(4 <= j && j <= 7 && type == Piece.Bishop) ||
							(i == 1 && type == Piece.Pawn && ((color == Piece.White && 4 <= j && j <= 5) || (color == Piece.Black && 6 <= j && j <= 7))) ||
							(type == Piece.Queen) || (i == 1 && type == Piece.King)) {
							if (possiblePin == (0, 0)) {
								inCheck = true;
								checks.Add((testedPos, delta.SquareIndex));
								break;
							}
							else {
								pins.Add(possiblePin);
								break;
							}
						}
						else {
							break;
						}
					}
				}
			}

			Coord[] knightMoves = { new Coord(-2, 1), new Coord(-1, 2), new Coord(1, 2), new Coord(2, 1), new Coord(2, -1), new Coord(1, -2), new Coord(-1, -2), new Coord(-2, -1) };
			foreach (Coord delta in knightMoves) {
				Coord newPos = start + delta;
				if (! newPos.IsInBounds()) {
					continue;
				}


				Piece endPiece = board.GetSquare(newPos.SquareIndex);
				if (endPiece.IsNull) {
					continue;
				}

				if (endPiece.Color != color && endPiece.Type == Piece.Knight) {
					inCheck = true;
					checks.Add((newPos.SquareIndex, delta.SquareIndex));
				}
			}

			return (inCheck, pins, checks);
		}

	}
}