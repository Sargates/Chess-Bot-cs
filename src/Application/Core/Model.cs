using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class Model {

		public Board board;
		public bool enforceColorToMove = false;

		public Model() {
			board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
			// board = new Board("rnb1nrk1/1pq3bp/p2p2p1/2pPp3/P1P1Pp2/2NBBN2/1PQ2PPP/R4RK1 w - - 0 13");
		}

		public void MakeMove(Move move) { //* Wrapper method for MovePiece, calls MovePiece and handles things like board history, 50 move rule, 3 move repition, 
			int movedFrom = move.StartSquare;
			int movedTo = move.TargetSquare;
			int moveFlag = move.MoveFlag;

			int pieceMoved = board.GetSquare(movedFrom);
			int color = GetColor(pieceMoved);
			int type = GetType(pieceMoved);
			int pieceTaken = (moveFlag==Move.EnPassantCaptureFlag) ? (board.opponentColour(color)|PieceHelper.Pawn) : board.GetSquare(movedTo);




			board.MovePiece(pieceMoved, movedFrom, movedTo);

			// If the move was an enpassant capture
			if (moveFlag == Move.EnPassantCaptureFlag) {
				board.board[board.enPassantIndex - board.forwardDir(color)] = 0;
			}

			board.enPassantIndex = -1; // Ok to set this to -1 here because of how En-Passant works
			// Set Enpassant square
			if (moveFlag == Move.PawnTwoUpFlag) {
				board.enPassantIndex = movedFrom + board.forwardDir(color);
			}

			// Is a promotion
			if (type == PieceHelper.Pawn && BoardHelper.RankIndex(movedTo) == (color==PieceHelper.White ? 7 : 0)) {
				board.board[movedTo] = PieceHelper.Queen|color;
			}


			board.state.UpdateBoardRepr(board);

			// If move is a castle, move rook
			if (moveFlag == Move.CastleFlag) {
				if (color == PieceHelper.White) {
					switch (movedTo) {
						case BoardHelper.c1:
							board.MovePiece(PieceHelper.Rook, BoardHelper.a1, BoardHelper.d1);
							break;
						case BoardHelper.g1:
							board.MovePiece(PieceHelper.Rook, BoardHelper.h1, BoardHelper.f1);
							break;
					}
				}
				if (color == PieceHelper.Black) {
					switch (movedTo) {
						case BoardHelper.c8:
							board.MovePiece(PieceHelper.Rook, BoardHelper.a8, BoardHelper.d8);
							break;
						case BoardHelper.g8:
							board.MovePiece(PieceHelper.Rook, BoardHelper.h8, BoardHelper.f8);
							break;
					}
				}
			}

			// If piece moved is a king or a rook, update castle perms
			if (PieceHelper.GetType(pieceMoved) == PieceHelper.King) {
				if (color == PieceHelper.White) {
					board.state.RemoveCastle(Gamestate.whiteKingCastle);
					board.state.RemoveCastle(Gamestate.whiteQueenCastle);
				}
				if (color == PieceHelper.Black) {
					board.state.RemoveCastle(Gamestate.blackKingCastle);
					board.state.RemoveCastle(Gamestate.blackQueenCastle);
				}

			}
			if (PieceHelper.GetType(pieceMoved) == PieceHelper.Rook) {
				if (color == PieceHelper.White) {
					if (movedFrom == BoardHelper.a1) {
						board.state.RemoveCastle(Gamestate.whiteQueenCastle);
					}
					if (movedFrom == BoardHelper.h1) {
						board.state.RemoveCastle(Gamestate.whiteKingCastle);
					}
				}
				if (color == PieceHelper.Black) {
					if (movedFrom == BoardHelper.a8) {
						board.state.RemoveCastle(Gamestate.blackQueenCastle);
					}
					if (movedFrom == BoardHelper.h8) {
						board.state.RemoveCastle(Gamestate.blackKingCastle);
					}
				}
			}

			board.state.castlePrivs = board.state.GetCastlePrivs();
			board.state.enpassantSquare = (board.enPassantIndex==-1) ? "-" : BoardHelper.IndexToSquareName(board.enPassantIndex);
			if (pieceTaken == PieceHelper.None || PieceHelper.GetType(pieceMoved) == PieceHelper.Pawn) { board.state.halfMoveCount += 1; }
			else { board.state.halfMoveCount = 0; }
			board.state.fullMoveCount += 1;

			board.whiteToMove = !board.whiteToMove; // ForwardDir / anything related to the active color will be the same up until this point
			board.state.fenColor = board.whiteToMove ? 'w' : 'b';
			
			board.state.SetRecentMove(move);

			Console.WriteLine(board.state.ToFEN());
			board.state.GetFuture().Clear();
			board.state.PushHistory();
		}

		public void UnmakeMove(Move move) {

			int movedFrom = move.TargetSquare;
			int movedTo = move.StartSquare;
			int moveFlag = move.MoveFlag;

			int pieceMoved = board.GetSquare(movedFrom);
			int pieceTaken = board.GetSquare(movedTo);

			board.MovePiece(pieceMoved, movedFrom, movedTo);
			board.whiteToMove = !board.whiteToMove;
		}

		public void PopHistory() {
			if (board.state.GetHistory().Count <= 1) {
				Console.WriteLine("Cannot pop history instance, empty history");
				return;
			}
			board.state.PopHistory().PushFuture();
			board.state = board.state.PeekHistory();
			board.UpdateFromState();

		}
		public void PopFuture() {
			if (board.state.GetFuture().Count <= 0) {
				Console.WriteLine("Cannot pop future instance, empty futures");
				return;
			}
			board.state.PopFuture().PushHistory();
			board.state = board.state.PeekHistory();
			board.UpdateFromState();

		}

		public int GetColor(int piece) => PieceHelper.GetColor(piece);
		public int GetType(int piece) => PieceHelper.GetType(piece);



		public void StartNewGame(string fenString) {
			//* Instantiate starting gamestate
			//* Instantiate new Board passing starting gamestate
			//* Recalc bitboards
			//* 
			//* 
		}


	}
}