using ChessBot.Helpers;

namespace ChessBot.Engine {
	
	public class Board {

		public Piece[] board;
		public Gamestate state;

		public bool whiteToMove;
		public int enPassantIndex;

		public int whiteKingPos;
		public int blackKingPos;
		




		public Board(string fen="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {

			// TODO: Add FEN string loading, StartNewGame should be in Controller/Model.cs, board should just be able to load a fen string in place
			state = new Gamestate(fen);
			board = BoardFromFen(state.fenBoard);
			for (int i = 0; i < board.Length; i++) {
				if (board[i] == (Piece.White | Piece.King)) { whiteKingPos = i; }
				if (board[i] == (Piece.Black | Piece.King)) { blackKingPos = i; }
			}


			Console.WriteLine(state.ToFEN());

			whiteToMove = state.fenColor == 'w';
		}


		public void UpdateFromState() {
			board = BoardFromFen(state.fenBoard);
			for (int i = 0; i < board.Length; i++) {
				if (board[i] == (Piece.White | Piece.King)) { whiteKingPos = i; }
				if (board[i] == (Piece.Black | Piece.King)) { blackKingPos = i; }
			}
			whiteToMove = state.fenColor == 'w';
			enPassantIndex = BoardHelper.NameToSquareIndex(state.enpassantSquare);
			
		}

		public static Piece[] BoardFromFen(string fen) {
			Piece[] board = new Piece[64];
			Piece file = 0; Piece rank = 7;
			foreach (char c in fen) {
				if (c == '/') {rank -= 1; file = 0; continue;}
				Piece index = 8*rank+file;

				if (char.IsNumber(c)) {

					file += int.Parse($"{c}");
					continue;
				}

				board[index] = Gamestate.BoardCharToEnum(c);
				file += 1;
			}

			return board;
		}

		public int activeColor => whiteToMove ? Piece.White : Piece.Black;

        public int opponentColour(int color) => whiteToMove ? Piece.Black : Piece.White;
		public int forwardDir(int color) => color == Piece.White ? 8 : -8;

		public void MakeMove(Move move, bool quiet = false) { //* Wrapper method for MovePiece, calls MovePiece and handles things like board history, 50 move rule, 3 move repition, 
			int movedFrom = move.StartSquare;
			int movedTo = move.TargetSquare;
			int moveFlag = move.MoveFlag;

			Piece pieceMoved = GetSquare(movedFrom);
			Piece pieceTaken = (moveFlag==Move.EnPassantCaptureFlag) ? (opponentColour(pieceMoved.Color)|Piece.Pawn) : GetSquare(movedTo);

			MovePiece(pieceMoved, movedFrom, movedTo);

			// If the move was an enpassant capture
			if (moveFlag == Move.EnPassantCaptureFlag) {
				board[enPassantIndex - forwardDir(pieceMoved.Color)] = 0;
			}

			enPassantIndex = -1; // Ok to set this to -1 here because of how En-Passant works
			// Set Enpassant square
			if (moveFlag == Move.PawnTwoUpFlag) {
				enPassantIndex = movedFrom + forwardDir(pieceMoved.Color);
			}

			// Is a promotion
			if (pieceMoved.Type == Piece.Pawn && BoardHelper.RankIndex(movedTo) == (pieceMoved.Color==Piece.White ? 7 : 0)) {
				board[movedTo] = Piece.Queen|pieceMoved.Color;
			}

			// If move is a castle, move rook
			if (moveFlag == Move.CastleFlag) {
				if (pieceMoved.Color == Piece.White) {
					switch (movedTo) {
						case BoardHelper.c1:
							MovePiece(Piece.Rook, BoardHelper.a1, BoardHelper.d1);
							break;
						case BoardHelper.g1:
							MovePiece(Piece.Rook, BoardHelper.h1, BoardHelper.f1);
							break;
					}
				}
				if (pieceMoved.Color == Piece.Black) {
					switch (movedTo) {
						case BoardHelper.c8:
							MovePiece(Piece.Rook, BoardHelper.a8, BoardHelper.d8);
							break;
						case BoardHelper.g8:
							MovePiece(Piece.Rook, BoardHelper.h8, BoardHelper.f8);
							break;
					}
				}
			}

			state.UpdateBoardRepr(this);

			// If piece moved is a king or a rook, update castle perms
			if (pieceMoved.Type == Piece.King) {
				if (pieceMoved.Color == Piece.White) {
					state.RemoveCastle(Gamestate.whiteKingCastle);
					state.RemoveCastle(Gamestate.whiteQueenCastle);
				}
				if (pieceMoved.Color == Piece.Black) {
					state.RemoveCastle(Gamestate.blackKingCastle);
					state.RemoveCastle(Gamestate.blackQueenCastle);
				}
			}
			if (pieceMoved.Type == Piece.Rook) {
				if (pieceMoved.Color == Piece.White) {
					if (movedFrom == BoardHelper.a1) {
						state.RemoveCastle(Gamestate.whiteQueenCastle);
					}
					if (movedFrom == BoardHelper.h1) {
						state.RemoveCastle(Gamestate.whiteKingCastle);
					}
				}
				if (pieceMoved.Color == Piece.Black) {
					if (movedFrom == BoardHelper.a8) {
						state.RemoveCastle(Gamestate.blackQueenCastle);
					}
					if (movedFrom == BoardHelper.h8) {
						state.RemoveCastle(Gamestate.blackKingCastle);
					}
				}
			}

			state.castlePrivs = state.GetCastlePrivs();
			state.enpassantSquare = (enPassantIndex==-1) ? "-" : BoardHelper.IndexToSquareName(enPassantIndex);
			if (pieceTaken == Piece.None || pieceMoved.Type == Piece.Pawn) { state.halfMoveCount = 0; }
			else { state.halfMoveCount += 1; }
			state.fullMoveCount += 1;

			whiteToMove = !whiteToMove; // ForwardDir / anything related to the active color will be the same up until this point
			state.fenColor = whiteToMove ? 'w' : 'b';

			state.SetRecentMove(move);

			if (! quiet) { Console.WriteLine(state.ToFEN()); }
			if (quiet) { state.GetFuture().Clear(); }
			state.PushHistory();
		}

		public void MovePiece(Piece piece, int movedFrom, int movedTo) {
			//* modify bitboards here

			if (board[movedFrom] == (Piece.White | Piece.King)) { whiteKingPos = movedTo; }
			if (board[movedFrom] == (Piece.Black | Piece.King)) { blackKingPos = movedTo; }
			
			int temp = board[movedTo];
			board[movedTo] = board[movedFrom];
			board[movedFrom] = Piece.None;
		}


		public void UnmakeMove() {
			PopHistory();
			state.PopFuture();
		}

		
		public void PopHistory() {
			if (state.GetHistory().Count <= 1) {
				Console.WriteLine("Cannot pop history instance, empty history");
				return;
			}
			state.PopHistory().PushFuture();
			state = state.PeekHistory();
			UpdateFromState();

		}
		public void PopFuture() {
			if (state.GetFuture().Count <= 0) {
				Console.WriteLine("Cannot pop future instance, empty futures");
				return;
			}
			state.PopFuture().PushHistory();
			state = state.PeekHistory();
			UpdateFromState();

		}
		
		public Piece GetSquare(int index) {
			if (! (0 <= index && index < 64) ) { throw new Exception("Board index out of bounds"); }
			return board[index];
		}
	}
}