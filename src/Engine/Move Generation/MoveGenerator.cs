using ChessBot.Helpers;
using ChessBot.Application;
namespace ChessBot.Engine {
	public class MoveGenerator {

		public static readonly Coord[] kingSideDeltas = { new Coord(1, 0), new Coord(2, 0) };
		public static readonly Coord[] queenSideDeltas = { new Coord(-1, 0), new Coord(-2, 0), new Coord(-3, 0) };

		public static int[] pinsBySquare = new int[64];
		public static (int checkerPos, int dirFromKing)[][] currentChecks = { new (int squareIndex, int dirFromKing)[0], new (int squareIndex, int dirFromKing)[0] }; // white, black respectively

		public static List<Move> GetPawnMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			Piece piece = board.GetSquare(index);

			if (currentChecks[(piece.Color == Piece.White) ? 0 : 1].Length == 2) { // King is double checks, must move king
				return moves;
			}

			Coord coord = new Coord(index);
			Coord delta = new Coord(board.forwardDir(piece.Color));
			Coord newPos = coord+delta;
			if (newPos.IsInBounds()) {
				Piece pawnOneUp = board.GetSquare(newPos.SquareIndex);
				if (pawnOneUp == Piece.None) {
					moves.Add(new Move(index, newPos.SquareIndex, (BoardHelper.RankIndex(index) == (piece.Color == Piece.White ? 6 : 1)) ? Move.PromoteToQueenFlag : Move.NoFlag));
					
					if (BoardHelper.RankIndex(index) == (piece.Color == Piece.White ? 1 : 6) && board.GetSquare(index + 2*board.forwardDir(piece.Color)) == Piece.None) {
						moves.Add(new Move(index, index + 2*board.forwardDir(piece.Color), Move.PawnTwoUpFlag));
					}
				}
			}

			delta = new Coord(+1, Math.Sign(board.forwardDir(piece.Color)));
			newPos = coord+delta;
			if (newPos.IsInBounds()) {
				Piece pawnAttackPositive = board.GetSquare(newPos.SquareIndex);
				if ((pawnAttackPositive.Type != Piece.None) && pawnAttackPositive.Color != piece.Color ) {
					moves.Add(new Move(index, newPos.SquareIndex, (BoardHelper.RankIndex(index) == (piece.Color == Piece.White ? 6 : 1)) ? Move.PromoteToQueenFlag : Move.NoFlag));
				} else if ((newPos.SquareIndex == board.enPassantIndex && board.GetSquare(index + 1) != piece.Color)) {
					moves.Add(new Move(index, newPos.SquareIndex, Move.EnPassantCaptureFlag));
				}
			}

			delta = new Coord(-1, Math.Sign(board.forwardDir(piece.Color)));
			newPos = coord+delta;
			if (newPos.IsInBounds()) {
				Piece pawnAttackNegative = board.GetSquare(newPos.SquareIndex);
				if ((pawnAttackNegative.Type != Piece.None) && pawnAttackNegative.Color != piece.Color ) {
					moves.Add(new Move(index, newPos.SquareIndex, (BoardHelper.RankIndex(index) == (piece.Color == Piece.White ? 6 : 1)) ? Move.PromoteToQueenFlag : Move.NoFlag));
				} else if ((newPos.SquareIndex == board.enPassantIndex && board.GetSquare(index - 1) != piece.Color)) {
					moves.Add(new Move(index, newPos.SquareIndex, Move.EnPassantCaptureFlag));
				}
			}


			for (int i=moves.Count-1; i>-1; i--) { // TODO: Change for compat. with PrecomputedMoveData
				Move move = moves[i];
				if (pinsBySquare[index]==0 && move.MoveFlag!=Move.EnPassantCaptureFlag) { continue; }
				if ((move.TargetSquare - move.StartSquare) == pinsBySquare[index] || (move.TargetSquare - move.StartSquare) == -pinsBySquare[index]) { continue; }
				if (move.MoveFlag == Move.PawnTwoUpFlag && ((move.TargetSquare - move.StartSquare)/2 == pinsBySquare[index] || (move.TargetSquare - move.StartSquare)/2 == -pinsBySquare[index])) { continue; }
				// Edge case in enpassant capture
				// See: https://www.chessprogramming.org/Perft_Results#cite_note-9:~:text=8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8
				if (move.MoveFlag == Move.EnPassantCaptureFlag) {
					// If the move is en-passant capture, then we need to check if the king would be in check after the move,
					// if the colorToMove's king is in the same file as both pawns and say a rook, en-passant capture would be illegal,
					// We need to cache the piece that's taken, remove it from the board, and check if the pawn to move is pinned by an enemy piece,

					int enemyPawnIndex = board.enPassantIndex - board.forwardDir(piece.Color);
					Piece enemyPawn = board.board[enemyPawnIndex];
					board.board[enemyPawnIndex] = Piece.None;
					(int, int)[] pins = MoveGenerator.GetCheckData(board, piece.Color == Piece.White ? board.whiteKingPos : board.blackKingPos, piece.Color).Item2;
					board.board[enemyPawnIndex] = enemyPawn;
					if (pins.Length == 0) { continue; }
				}



				// Console.WriteLine($"Move removed: {move}");
				moves.RemoveAt(i);
			}

			return moves;
		}
		public static List<Move> GetKnightMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			Piece piece = board.GetSquare(index);
			Coord coord = new Coord(index);
			if (currentChecks[(piece.Color == Piece.White) ? 0 : 1].Length == 2) { // King is double checks, must move king
				return moves;
			}
			if (! (pinsBySquare[index] == 0 )) { // Knights can never move when pinned
				return moves;
			}

			foreach (int direction in new int[]{ 6, 15, 17, 10, -6, -15, -17, -10 }) {
				Coord delta = new Coord(direction+26)-new Coord(26); // get relative delta from index that isnt on an edge, 26 is arbitrary
				// TODO: Change for compat. with PrecomputedMoveData
				Coord newPos = coord + delta;
				if (! newPos.IsInBounds()) { continue; } // Passes guard clause if in bounds

				Piece newPiece = board.GetSquare(newPos.SquareIndex);

				if (newPiece.Type == Piece.None || newPiece.Color != piece.Color) {
					moves.Add(new Move(index, newPos.SquareIndex));
				}
			}

			return moves;
		}
		public static List<Move> GetBishopMoves(Board board, int index) {
			List<Move> moves = new List<Move>();

			Piece piece = board.GetSquare(index);
			Coord coord = new Coord(index);
			if (currentChecks[(piece.Color == Piece.White) ? 0 : 1].Length == 2) { // King is double checks, must move king
				return moves;
			}
			
			// Iterate over each direction in moveset and scale by number [1-7]
			foreach (int direction in new int[] { 9, -7, -9, 7 }) {
				if (! (pinsBySquare[index] == 0  || pinsBySquare[index] == direction || pinsBySquare[index] == -direction)) { // value of 0 means piece is not pinned
					continue;
				}
				Coord delta = new Coord(direction+26)-new Coord(26); // get relative delta from index that isnt on an edge, 26 is arbitrary
				// TODO: Change for compat. with PrecomputedMoveData
				for (int i=1;i<8;i++) {
					Coord newPos = coord + delta*i;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds
					

					Piece newPiece = board.GetSquare(newPos.SquareIndex);
					
					
					if (newPiece.Type == Piece.None) {
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
			if (currentChecks[(piece.Color == Piece.White) ? 0 : 1].Length == 2) { // King is double checks, must move king
				return moves;
			}
			
			// Iterate over each direction in moveset and scale by number [1-7]
			foreach (int direction in new int[] { 8, 1, -8, -1 }) {
				if (! (pinsBySquare[index] == 0  || pinsBySquare[index] == direction || pinsBySquare[index] == -direction)) { // value of 0 means piece is not pinned
					continue;
				}
				Coord delta = new Coord(direction+26)-new Coord(26); // get relative delta from index that isnt on an edge, 26 is arbitrary
				// TODO: Change for compat. with PrecomputedMoveData
				for (int i=1;i<8;i++) {
					Coord newPos = coord + delta*i;
					if (! newPos.IsInBounds()) { break; } // Passes guard clause if in bounds
					

					Piece newPiece = board.GetSquare(newPos.SquareIndex);
					
					
					if (newPiece.Type == Piece.None) {
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

			if (currentChecks[(piece.Color == Piece.White) ? 0 : 1].Length == 2) { // King is double checks, must move king
				return moves;
			}

			// Iterate over each direction in moveset and scale by number [1-7]
			foreach (int direction in new int[] { 9, -7, -9, 7, 8, 1, -8, -1 }) {
				if (! (pinsBySquare[index] == 0  || pinsBySquare[index] == direction || pinsBySquare[index] == -direction)) { // value of 0 means piece is not pinned
					continue;
				}
				Coord delta = new Coord(direction+26)-new Coord(26); // get relative delta from index that isnt on an edge, 26 is arbitrary
				// TODO: Change for compat. with PrecomputedMoveData
				for (int i=1;i<8;i++) {
					Coord newPos = coord + (delta * i);
					if (! newPos.IsInBounds()) { break; } // Set up Precomputed move data to calculate num squares to edge of board
					// Passes guard clause if in bounds



					Piece newPiece = board.GetSquare(newPos.SquareIndex);
					
					if (newPiece.Type == Piece.None) {
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
			foreach (int direction in new int[] { 9, -7, -9, 7, 8, 1, -8, -1 }) {
				Coord delta = new Coord(direction+26)-new Coord(26); // get relative delta from index that isnt on an edge, 26 is arbitrary
				Coord newPos = coord + delta;
				if (! newPos.IsInBounds()) { continue; } // Passes guard clause if in bounds


				// check king in double check

				Piece newPiece = board.GetSquare(newPos.SquareIndex);

				if (newPiece.Type == Piece.None || newPiece.Color != piece.Color) {
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
					Coord checkSquareClear = coord + kingSideDeltas[i];
					Coord checkSquareInCheck = coord + kingSideDeltas[i] - new Coord(1, 0);
					if (! checkSquareClear.IsInBounds() || ! checkSquareInCheck.IsInBounds()) Console.WriteLine($"Castling error King side {checkSquareClear.IsInBounds()} {checkSquareInCheck.IsInBounds()}");
					// ConsoleHelper.WriteLine($"King side, {newPos.SquareIndex}");
					if (IsSquareAttacked(board, checkSquareInCheck.SquareIndex, piece.Color)) { kingSide = false; break; }
					if (board.GetSquare(checkSquareClear.SquareIndex) != Piece.None) { kingSide = false; break; }
				}
			}			

			if (queenSide) {
				for (int i=0; i<queenSideDeltas.Length; i++) {
					Coord checkSquareClear = coord + queenSideDeltas[i];
					Coord checkSquareInCheck = coord + queenSideDeltas[i] + new Coord(1, 0);
					if (! checkSquareClear.IsInBounds() || ! checkSquareInCheck.IsInBounds()) Console.WriteLine($"Castling error Queen side {checkSquareClear.IsInBounds()} {checkSquareInCheck.IsInBounds()}");
					// ConsoleHelper.WriteLine($"Queen side, {newPos.SquareIndex}");
					if (IsSquareAttacked(board, checkSquareInCheck.SquareIndex, piece.Color)) { queenSide = false; break; }
					if (board.GetSquare(checkSquareClear.SquareIndex) != Piece.None) { queenSide = false; break; }
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

		public static Move[] GetMoves(Board board, int index) { // ! check edgecases


			Piece piece = board.GetSquare(index);


			int kingPos = piece.Color == Piece.White ? board.whiteKingPos : board.blackKingPos;
			var checkData = GetCheckData(board, kingPos, piece.Color);
			bool isInCheck = checkData.Item1;
			pinsBySquare = new int[64];
			foreach ((int squareIndex, int dirFromKing) pin in checkData.Item2) {
				pinsBySquare[pin.squareIndex] = pin.dirFromKing;
				// Console.WriteLine($"{pin.squareIndex} {pin.dirFromKing}");
			}
			// Console.WriteLine(kingPos);
			currentChecks[(piece.Color == Piece.White) ? 0 : 1] = checkData.Item3;
			
			

			List<Move> moves = piece.Type switch {
				Piece.Pawn => GetPawnMoves(board, index),
				Piece.Knight => GetKnightMoves(board, index),
				Piece.Bishop => GetBishopMoves(board, index),
				Piece.Rook => GetRookMoves(board, index),
				Piece.Queen => GetQueenMoves(board, index),
				Piece.King => GetKingMoves(board, index),
				_ => new List<Move>() // Space is invalid, no moves, this is for Computer players not causing the program to crash
			};


			if (piece.Type == Piece.King || currentChecks[(piece.Color == Piece.White) ? 0 : 1].Length == 2) { // Check if each end square is in attacked for each king move
				// Combined `if king in double check` logic because outcome is the same (moves should be empty if king is in doublecheck)
				for (int i=moves.Count-1; i>-1; i--) {
					Move move = moves[i];
					if (IsSquareAttacked(board, move.TargetSquare, piece.Color)) {
						moves.RemoveAt(i);
					}
				}
				return moves.ToArray();
			}

			if (! isInCheck) {
				return moves.ToArray();
			} // Passes guard clause if king is checked


			for (int i=moves.Count-1; i>-1; i--) { // Check each move against needed squares to block the check
				Move move = moves[i];
				bool NotHit = true;
				int checkingPosition = currentChecks[(piece.Color == Piece.White) ? 0 : 1][0].checkerPos;
				int dirFromKing = currentChecks[(piece.Color == Piece.White) ? 0 : 1][0].dirFromKing;
				while (checkingPosition != kingPos) { // Start at position of checker, subtract dirFromKing until checkingPosition == kingPos
					if (move.TargetSquare == checkingPosition) {
						NotHit = false;
						break;
					}
					
					checkingPosition -= dirFromKing;
				}

				if (NotHit) { moves.RemoveAt(i); }
			}
			return moves.ToArray();

			// Console.WriteLine();
			// Console.Write("Checks: ");
			// foreach ((int squareIndex, int dirFromKing) check in currentChecks[(piece.Color == Piece.White) ? 0 : 1]) {
			// 	Console.Write($"{check.squareIndex} {check.dirFromKing}, ");
			// }
			// Console.WriteLine();
			// Console.WriteLine();


			// ConsoleHelper.WriteLine($"{Piece.EnumToRepr[piece]}");
			// foreach (Move move in moves) {
			// 	ConsoleHelper.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)} {BoardHelper.IndexToSquareName(move.TargetSquare)} {move.MoveFlag}", ConsoleColor.DarkMagenta);
			// }

		}

		public static Move[] GetAllMoves(Board board, int color) {
			List<Move> totalMoves = new List<Move>();

			for (int i=0; i<64;i++) {
				if (board.GetSquare(i).Color != color) continue;
				totalMoves.AddRange(GetMoves(board, i));
			}

			return totalMoves.ToArray();
		}
		public static bool IsSquareAttacked(Board board, int index, int color) {
			return GetCheckData(board, index, color).Item1;
		}


		public static (bool, (int, int)[], (int, int)[]) GetCheckData(Board board, int index, int color) {
			// Credits to Eddie Sharick for this algorithm (adapted from python)
			// https://youtu.be/coAOXj6ZnSI

			List<(int, int)> pins = new List<(int, int)>();
			List<(int, int)> checks = new List<(int, int)>();
			bool inCheck = false;

			int[] directions = { -8, 1, 8, -1 , 9, 7, -9, -7 };
			int start = index;

			for (int j=0; j<directions.Length; j++) {

				Coord delta = new Coord(directions[j]+26)-new Coord(26);

				(int, int) possiblePin = (0, 0);
				for (int i = 1; i < 8; i++) {
					if (! (new Coord(start) + (delta*i)).IsInBounds()) { // TODO: Change for compat. with PrecomputedMoveData
						break;
					}

					int testedPos = start + directions[j] * i;

					Piece attackingSquare = board.GetSquare(testedPos);

					if (attackingSquare.IsNull) {
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
						else {
							break;
						}
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
				if (endPiece.IsNull) {
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
}