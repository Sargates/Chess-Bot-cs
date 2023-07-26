using ChessBot.Application.Helpers;
using ChessBot.Engine.Helpers;

namespace ChessBot.Engine {
	
	public class Board {
		public States state;
		public enum States {
			selecting,
			moving,
		}

		public int[] board;

		public bool whiteToMove;
		public int enPassantIndex;


		public Board() {
			board = new int[64];
			board[0]  = PieceHelper.White | PieceHelper.Rook;
			board[1]  = PieceHelper.White | PieceHelper.Knight;
			board[2]  = PieceHelper.White | PieceHelper.Bishop;
			board[3]  = PieceHelper.White | PieceHelper.Queen;
			board[4]  = PieceHelper.White | PieceHelper.King;
			board[5]  = PieceHelper.White | PieceHelper.Bishop;
			board[6]  = PieceHelper.White | PieceHelper.Knight;
			board[7]  = PieceHelper.White | PieceHelper.Rook;

			board[8]  = PieceHelper.White | PieceHelper.Pawn;
			board[9]  = PieceHelper.White | PieceHelper.Pawn;
			board[10] = PieceHelper.White | PieceHelper.Pawn;
			board[11] = PieceHelper.White | PieceHelper.Pawn;
			board[12] = PieceHelper.White | PieceHelper.Pawn;
			board[13] = PieceHelper.White | PieceHelper.Pawn;
			board[14] = PieceHelper.White | PieceHelper.Pawn;
			board[15] = PieceHelper.White | PieceHelper.Pawn;

			board[48] = PieceHelper.Black | PieceHelper.Pawn;
			board[49] = PieceHelper.Black | PieceHelper.Pawn;
			board[50] = PieceHelper.Black | PieceHelper.Pawn;
			board[27] = PieceHelper.Black | PieceHelper.Pawn;
			board[52] = PieceHelper.Black | PieceHelper.Pawn;
			board[53] = PieceHelper.Black | PieceHelper.Pawn;
			board[54] = PieceHelper.Black | PieceHelper.Pawn;
			board[55] = PieceHelper.Black | PieceHelper.Pawn;

			board[56] = PieceHelper.Black | PieceHelper.Rook;
			board[57] = PieceHelper.Black | PieceHelper.Knight;
			board[58] = PieceHelper.Black | PieceHelper.Bishop;
			board[59] = PieceHelper.Black | PieceHelper.Queen;
			board[60] = PieceHelper.Black | PieceHelper.King;
			board[61] = PieceHelper.Black | PieceHelper.Bishop;
			board[62] = PieceHelper.Black | PieceHelper.Knight;
			board[63] = PieceHelper.Black | PieceHelper.Rook;

			state = States.selecting;
			whiteToMove = true;
		}

		public int activeColor => whiteToMove ? PieceHelper.White : PieceHelper.Black;
        public int opponentColour => whiteToMove ? PieceHelper.Black : PieceHelper.White;
		public int forwardDir(int color) => color == PieceHelper.White ? 8 : -8;

		public Move[] GetPawnMoves(int index) {
			List<Move> moves = new List<Move>();

			int piece = GetSquare(index);

			// `color` equals the color of `piece`, if `piece` is a null piece, get `activeColor`
			int color = piece == PieceHelper.None ? activeColor : PieceHelper.GetColor(piece);


			// !!! Game crashes when pawn is at edge of board, need to find way to check these pieces, (precomputed move data?)

			Coord coord = new Coord(index);
			Coord pawnOneUp = new Coord(index + forwardDir(color));
			if ((coord+pawnOneUp).IsInBounds() && GetSquare(pawnOneUp.SquareIndex) == PieceHelper.None) { // ! add pinning capabilities
				moves.Add(new Move(index, index + forwardDir(color)));
				if (BoardHelper.RankIndex(index) == (color == PieceHelper.White ? 1 : 6) && GetSquare(index + 2*forwardDir(color)) == PieceHelper.None) {
					moves.Add(new Move(index, index + 2*forwardDir(color), Move.PawnTwoUpFlag));
				}
			}


			// int pawnOneUp = GetSquare(index + forwardDir(color));
			// if (pawnOneUp == PieceHelper.None) { // ! add pinning capabilities
			// 	moves.Add(new Move(index, index + forwardDir(color)));
			// 	if (BoardHelper.RankIndex(index) == (color == PieceHelper.White ? 1 : 6) && GetSquare(index + 2*forwardDir(color)) == PieceHelper.None) {
			// 		moves.Add(new Move(index, index + 2*forwardDir(color), Move.PawnTwoUpFlag));
			// 	}
			// }

			// int pawnAttackPositive = GetSquare(index + forwardDir(color) + 1);
			// if ((PieceHelper.GetType(pawnAttackPositive) != PieceHelper.None) && PieceHelper.GetColor(pawnAttackPositive) != color ) { // ! add pinning capabilities
			// 	moves.Add(new Move(index, index + forwardDir(color) + 1));
			// } else if ((index + forwardDir(color) + 1) == enPassantIndex) {
			// 	moves.Add(new Move(index, index + forwardDir(color) + 1, Move.EnPassantCaptureFlag));
			// }
			// int pawnAttackNegative = GetSquare(index + forwardDir(color) - 1);
			// if ((PieceHelper.GetType(pawnAttackNegative) != PieceHelper.None) && PieceHelper.GetColor(pawnAttackNegative) != color ) { // ! add pinning capabilities
			// 	moves.Add(new Move(index, index + forwardDir(color) - 1));
			// } else if ((index + forwardDir(color) - 1) == enPassantIndex) {
			// 	moves.Add(new Move(index, index + forwardDir(color) - 1, Move.EnPassantCaptureFlag));
			// }

			foreach (Move move in moves) {
				ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
			}

			return moves.ToArray();
		}
		public Move[] GetKnightMoves(int index) {
			List<Move> moves = new List<Move>();

			int piece = GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);
			foreach ((int x, int y) in new (int x, int y)[] { (-2, 1), (-1, 2), (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1) }) {
				Coord delta = new Coord(x, y);
				Coord newPos = coord + delta;
				if (! newPos.IsInBounds()) { continue; } // Passes guard clause if in bounds

				int newPiece = GetSquare(newPos.SquareIndex);
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
		public Move[] GetBishopMoves(int index) {
			List<Move> moves = new List<Move>();

			int piece = GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);
			// Iterate over each direction in moveset and scale by number [1-7]
			foreach ((int x, int y) in new (int x, int y)[] { (1, 1), (1, -1), (-1, -1), (-1, 1) }) {
				for (int i=1;i<8;i++) {
					Coord delta = new Coord(x, y)*i;
					Coord newPos = coord + delta;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds
					

					int newPiece = GetSquare(newPos.SquareIndex);
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
		public Move[] GetRookMoves(int index) {
			List<Move> moves = new List<Move>();

			int piece = GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);
			// Iterate over each direction in moveset and scale by number [1-7]
			foreach ((int x, int y) in new (int x, int y)[] { (0, 1), (1, 0), (0, -1), (-1, 0) }) {
				for (int i=1;i<8;i++) {
					Coord delta = new Coord(x, y)*i;
					Coord newPos = coord + delta;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds
					

					int newPiece = GetSquare(newPos.SquareIndex);
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
		public Move[] GetQueenMoves(int index) {
			List<Move> moves = new List<Move>();

			int piece = GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);

			// Iterate over each direction in moveset and scale by number [1-7]
			foreach ((int x, int y) in new (int x, int y)[] { (1, 1), (1, -1), (-1, -1), (-1, 1), (0, 1), (1, 0), (0, -1), (-1, 0) }) {
				for (int i=1;i<8;i++) {
					Coord delta = new Coord(x, y)*i;
					Coord newPos = coord + delta;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds
					

					int newPiece = GetSquare(newPos.SquareIndex);
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
		public Move[] GetKingMoves(int index) {
			List<Move> moves = new List<Move>();

			int piece = GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);
			Coord coord = new Coord(index);
			foreach ((int x, int y) in new (int x, int y)[] { (1, 1), (1, -1), (-1, -1), (-1, 1), (0, 1), (1, 0), (0, -1), (-1, 0) }) {
				Coord delta = new Coord(x, y);
				Coord newPos = coord + delta;
				if (! newPos.IsInBounds()) { continue; } // Passes guard clause if in bounds

				int newPiece = GetSquare(newPos.SquareIndex);
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
		public Move[] GetMoves(int index) { // ! check edgecases

			// !!! Add pinning capabilities
			// !!! Make sure moving in opposite direction of pin doesnt work; moving in same direction as pin does

			int piece = GetSquare(index);
			int type = PieceHelper.GetType(piece);
			int color = PieceHelper.GetColor(piece);



			ConsoleHelper.WriteLine($"PieceEnum = {Convert.ToString(piece, 2)}");
			return type switch {
				PieceHelper.Pawn => GetPawnMoves(index),
				PieceHelper.Knight => GetKnightMoves(index),
				PieceHelper.Bishop => GetBishopMoves(index),
				PieceHelper.Rook => GetRookMoves(index),
				PieceHelper.Queen => GetQueenMoves(index),
				PieceHelper.King => GetKingMoves(index),
				_ => throw new Exception("Invalid piece type")
			};

			
		}


		public void MovePiece(int piece, int movedFrom, int movedTo) {
			// * modify bitboards here
			
			int temp = board[movedTo];
			board[movedTo] = board[movedFrom];
			board[movedFrom] = PieceHelper.None;
		}

		public void MakeMove(Move move) { // * Wrapper method for MovePiece, calls MovePiece and handles things like board history, 50 move rule, 3 move repition, 
			int movedFrom = move.StartSquare;
			int movedTo = move.TargetSquare;
			int moveFlag = move.MoveFlag;

			int pieceMoved = GetSquare(movedFrom);
			int pieceTaken = (moveFlag==Move.EnPassantCaptureFlag) ? (opponentColour|PieceHelper.Pawn) : GetSquare(movedTo);



			MovePiece(pieceMoved, movedFrom, movedTo);

			
			if (moveFlag == Move.EnPassantCaptureFlag) {
				board[enPassantIndex - forwardDir(activeColor)] = 0;
			}



			
			enPassantIndex = 0; // Ok to set this to 0 because of how En-Passant works
			if (moveFlag == Move.PawnTwoUpFlag) {
				enPassantIndex = movedFrom + forwardDir(activeColor);
			}
			
			whiteToMove = !whiteToMove; // ForwardDir / anything related to the active color will be the same up until this point

			
		}
		public void UnmakeMove(Move move) {

			int movedFrom = move.TargetSquare;
			int movedTo = move.StartSquare;
			int moveFlag = move.MoveFlag;

			int pieceMoved = GetSquare(movedFrom);
			int pieceTaken = GetSquare(movedTo);

			MovePiece(pieceMoved, movedFrom, movedTo);
			whiteToMove = !whiteToMove;
		}

		public int GetSquare(int index) {
			if (! (0 <= index && index < 64) ) { throw new Exception("Board index out of bounds"); }
			
			return board[index];
		}
	}
}