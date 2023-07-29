using ChessBot.Helpers;
namespace ChessBot.Engine {
	public class MoveGenerator {


		public static Move[] GetPawnMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			int piece = board.GetSquare(index);

			// `color` equals the color of `piece`, if `piece` is a null piece, get `activeColor`
			int color = piece == PieceHelper.None ? board.activeColor : PieceHelper.GetColor(piece);


			// !!! Game crashes when pawn is at edge of board, need to find way to check these pieces, (precomputed move data?)

			// Coord coord = new Coord(index);
			// Coord pawnOneUp = new Coord(index + board.forwardDir(color));
			// if ((coord+pawnOneUp).IsInBounds() && board.GetSquare(pawnOneUp.SquareIndex) == PieceHelper.None) { // ! add pinning capabilities
			// 	moves.Add(new Move(index, index + board.forwardDir(color)));
			// 	if (BoardHelper.RankIndex(index) == (color == PieceHelper.White ? 1 : 6) && board.GetSquare(index + 2*board.forwardDir(color)) == PieceHelper.None) {
			// 		moves.Add(new Move(index, index + 2*board.forwardDir(color), Move.PawnTwoUpFlag));
			// 	}
			// }


			int pawnOneUp = board.GetSquare(index + board.forwardDir(color));
			if (pawnOneUp == PieceHelper.None) { // ! add pinning capabilities
				moves.Add(new Move(index, index + board.forwardDir(color)));
				if (BoardHelper.RankIndex(index) == (color == PieceHelper.White ? 1 : 6) && board.GetSquare(index + 2*board.forwardDir(color)) == PieceHelper.None) {
					moves.Add(new Move(index, index + 2*board.forwardDir(color), Move.PawnTwoUpFlag));
				}
			}

			int pawnAttackPositive = board.GetSquare(index + board.forwardDir(color) + 1);
			if ((PieceHelper.GetType(pawnAttackPositive) != PieceHelper.None) && PieceHelper.GetColor(pawnAttackPositive) != color ) { // ! add pinning capabilities
				moves.Add(new Move(index, index + board.forwardDir(color) + 1));
			} else if ((index + board.forwardDir(color) + 1) == board.enPassantIndex) {
				moves.Add(new Move(index, index + board.forwardDir(color) + 1, Move.EnPassantCaptureFlag));
			}
			int pawnAttackNegative = board.GetSquare(index + board.forwardDir(color) - 1);
			if ((PieceHelper.GetType(pawnAttackNegative) != PieceHelper.None) && PieceHelper.GetColor(pawnAttackNegative) != color ) { // ! add pinning capabilities
				moves.Add(new Move(index, index + board.forwardDir(color) - 1));
			} else if ((index + board.forwardDir(color) - 1) == board.enPassantIndex) {
				moves.Add(new Move(index, index + board.forwardDir(color) - 1, Move.EnPassantCaptureFlag));
			}

			foreach (Move move in moves) {
				ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
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
			foreach (Move move in moves) {
				ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
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
			foreach (Move move in moves) {
				ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
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
			foreach (Move move in moves) {
				ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
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
			foreach (Move move in moves) {
				ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
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
			foreach (Move move in moves) {
				ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
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

			ConsoleHelper.WriteLine($"PieceEnum = {Convert.ToString(piece, 2)}");
			return type switch {
				PieceHelper.Pawn => GetPawnMoves(board, index),
				PieceHelper.Knight => GetKnightMoves(board, index),
				PieceHelper.Bishop => GetBishopMoves(board, index),
				PieceHelper.Rook => GetRookMoves(board, index),
				PieceHelper.Queen => GetQueenMoves(board, index),
				PieceHelper.King => GetKingMoves(board, index),
				_ => throw new Exception("Invalid piece type")
			};
		}


	}
}