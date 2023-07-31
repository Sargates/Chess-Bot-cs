using ChessBot.Helpers;
namespace ChessBot.Engine {
	public class MoveGenerator {

		public static readonly Coord[] kingSideDeltas = { new Coord(1, 0), new Coord(2, 0) };
		public static readonly Coord[] queenSideDeltas = { new Coord(-1, 0), new Coord(-2, 0), new Coord(-3, 0) };

		public static Move[] GetPawnMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			int piece = board.GetSquare(index);

			// `color` is the color of `piece`, if `piece` is a null piece, get `activeColor`
			int color = piece == PieceHelper.None ? board.activeColor : PieceHelper.GetColor(piece);


			// !!! Game crashes when pawn is at edge of board, need to find way to check these pieces, (precomputed move data?)


			Coord coord = new Coord(index);
			Coord delta = new Coord(board.forwardDir(color));
			Coord newPos = coord+delta;
			int pawnOneUp = board.GetSquare(newPos.SquareIndex);
			if (newPos.IsInBounds() && pawnOneUp == PieceHelper.None) { // ! add pinning capabilities
				moves.Add(new Move(index, index + board.forwardDir(color)));
				
				if (BoardHelper.RankIndex(index) == (color == PieceHelper.White ? 1 : 6) && board.GetSquare(index + 2*board.forwardDir(color)) == PieceHelper.None) {
					moves.Add(new Move(index, index + 2*board.forwardDir(color), Move.PawnTwoUpFlag));
				}
			}

			delta = new Coord(board.forwardDir(color) + 1);
			newPos = coord+delta;
			int pawnAttackPositive = board.GetSquare(newPos.SquareIndex);
			if ((PieceHelper.GetType(pawnAttackPositive) != PieceHelper.None) && PieceHelper.GetColor(pawnAttackPositive) != color ) { // ! add pinning capabilities
				moves.Add(new Move(index, newPos.SquareIndex));
			} else if ((newPos.SquareIndex) == board.enPassantIndex && PieceHelper.GetColor(board.GetSquare(index + 1)) != color) {
				moves.Add(new Move(index, newPos.SquareIndex, Move.EnPassantCaptureFlag));
			}

			delta = new Coord(board.forwardDir(color) - 1);
			newPos = coord+delta;
			int pawnAttackNegative = board.GetSquare(newPos.SquareIndex);
			if ((PieceHelper.GetType(pawnAttackNegative) != PieceHelper.None) && PieceHelper.GetColor(pawnAttackNegative) != color ) { // ! add pinning capabilities
				moves.Add(new Move(index, newPos.SquareIndex));
			} else if ((newPos.SquareIndex) == board.enPassantIndex && PieceHelper.GetColor(board.GetSquare(index - 1)) != color) {
				moves.Add(new Move(index, newPos.SquareIndex, Move.EnPassantCaptureFlag));
			}

			return moves.ToArray();
		}

		public static Move[] GetKnightMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			int piece = board.GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);
			foreach ((int x, int y) in new (int x, int y)[] { (-2, 1), (-1, 2), (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1) }) {
				Coord delta = new Coord(x, y);
				Coord newPos = coord + delta;
				if (! newPos.IsInBounds()) { continue; } // Passes guard clause if in bounds

				int newPiece = board.GetSquare(newPos.SquareIndex);
				int newType = PieceHelper.GetType(newPiece);
				int newColor = PieceHelper.GetColor(newPiece);

				if (newType == PieceHelper.None || newColor != color) { // ! add pinning capabilities
					moves.Add(new Move(index, newPos.SquareIndex));
				}
			}

			return moves.ToArray();
		}
		public static Move[] GetBishopMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			int piece = board.GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);
			// Iterate over each direction in moveset and scale by number [1-7]
			foreach ((int x, int y) in new (int x, int y)[] { (1, 1), (1, -1), (-1, -1), (-1, 1) }) {
				for (int i=1;i<8;i++) {
					Coord delta = new Coord(x, y)*i;
					Coord newPos = coord + delta;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds
					

					int newPiece = board.GetSquare(newPos.SquareIndex);
					int newType = PieceHelper.GetType(newPiece);
					int newColor = PieceHelper.GetColor(newPiece);
					
					
					if (newType == PieceHelper.None) { // ! add pinning capabilities
						moves.Add(new Move(index, newPos.SquareIndex));
						continue;
					} else if (newColor != color) {
						moves.Add(new Move(index, newPos.SquareIndex));
						break;
					}
					break;
				}
			}

			return moves.ToArray();
		}
		public static Move[] GetRookMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			int piece = board.GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);
			// Iterate over each direction in moveset and scale by number [1-7]
			foreach ((int x, int y) in new (int x, int y)[] { (0, 1), (1, 0), (0, -1), (-1, 0) }) {
				for (int i=1;i<8;i++) {
					Coord delta = new Coord(x, y)*i;
					Coord newPos = coord + delta;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds
					

					int newPiece = board.GetSquare(newPos.SquareIndex);
					int newType = PieceHelper.GetType(newPiece);
					int newColor = PieceHelper.GetColor(newPiece);
					
					
					if (newType == PieceHelper.None) { // ! add pinning capabilities
						moves.Add(new Move(index, newPos.SquareIndex));
						continue;
					} else if (newColor != color) {
						moves.Add(new Move(index, newPos.SquareIndex));
						break;
					}
					break;
				}
			}

			return moves.ToArray();
		}
		public static Move[] GetQueenMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			int piece = board.GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);

			// Iterate over each direction in moveset and scale by number [1-7]
			foreach ((int x, int y) in new (int x, int y)[] { (1, 1), (1, -1), (-1, -1), (-1, 1), (0, 1), (1, 0), (0, -1), (-1, 0) }) {
				for (int i=1;i<8;i++) {
					Coord delta = new Coord(x, y)*i;
					Coord newPos = coord + delta;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds


					int newPiece = board.GetSquare(newPos.SquareIndex);
					int newType = PieceHelper.GetType(newPiece);
					int newColor = PieceHelper.GetColor(newPiece);

					
					if (newType == PieceHelper.None) { // ! add pinning capabilities
						moves.Add(new Move(index, newPos.SquareIndex));
						continue;
					} else if (newColor != color) {
						moves.Add(new Move(index, newPos.SquareIndex));
						break;
					}
					break;
				}
			}

			return moves.ToArray();
		}
		public static Move[] GetKingMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			int piece = board.GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);
			foreach ((int x, int y) in new (int x, int y)[] { (1, 1), (1, -1), (-1, -1), (-1, 1), (0, 1), (1, 0), (0, -1), (-1, 0) }) {
				Coord delta = new Coord(x, y);
				Coord newPos = coord + delta;
				if (! newPos.IsInBounds()) { continue; } // Passes guard clause if in bounds

				int newPiece = board.GetSquare(newPos.SquareIndex);
				int newType = PieceHelper.GetType(newPiece);
				int newColor = PieceHelper.GetColor(newPiece);

				if (newType == PieceHelper.None || newColor != color) { // ! add pinning capabilities
					moves.Add(new Move(index, newPos.SquareIndex));
				}
			}

			int flagKing = color == PieceHelper.White ? Gamestate.whiteKingCastle : Gamestate.blackKingCastle;
			int flagQueen = color == PieceHelper.White ? Gamestate.whiteQueenCastle : Gamestate.blackQueenCastle;
			bool kingSide = (board.state.castlePrivsBin & flagKing) == flagKing;
			bool queenSide = (board.state.castlePrivsBin & flagQueen) == flagQueen;

			// bool isInCheck = isAttacked(index);
			// kingSide = kingSide && isInCheck;
			// queenSide = queenSide && isInCheck;

			if (kingSide) {
				for (int i=0; i<kingSideDeltas.Length; i++) {
					Coord newPos = coord + kingSideDeltas[i];
					ConsoleHelper.WriteLine($"King side, {newPos.SquareIndex}");
					// if (isAttacked(newPos.SquareIndex)) { kingSide = false; break; }
					if (board.GetSquare(newPos.SquareIndex) != PieceHelper.None) { kingSide = false; break; }
				}
			}			

			if (queenSide) {
				for (int i=0; i<queenSideDeltas.Length; i++) {
					Coord newPos = coord + queenSideDeltas[i];
					ConsoleHelper.WriteLine($"Queen side, {newPos.SquareIndex}");
					// if (i == 2) { if (isAttacked(newPos.SquareIndex)) { queenSide = false; break; } }
					if (board.GetSquare(newPos.SquareIndex) != PieceHelper.None) { queenSide = false; break; }
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

			
			return moves.ToArray();
		}

		// TODO: Add method to sum number of moves to an given depth (recursive? iterative?)
		public static Move[] GetMoves(Board board, int index) { // ! check edgecases

			// !!! Add pinning capabilities
			// !!! Make sure moving in opposite direction of pin doesnt work; moving in same direction as pin does

			int piece = board.GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);

			// ConsoleHelper.WriteLine($"PieceEnum = {Convert.ToString(piece, 2)}");

			Move[] moves = type switch {
				PieceHelper.Pawn => GetPawnMoves(board, index),
				PieceHelper.Knight => GetKnightMoves(board, index),
				PieceHelper.Bishop => GetBishopMoves(board, index),
				PieceHelper.Rook => GetRookMoves(board, index),
				PieceHelper.Queen => GetQueenMoves(board, index),
				PieceHelper.King => GetKingMoves(board, index),
				_ => throw new Exception("Invalid piece type")
			};

			// ConsoleHelper.WriteLine($"{PieceHelper.EnumToRepr[piece]}");
			// foreach (Move move in moves) {
			// 	ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
			// }

			return moves;
		}


	}
}