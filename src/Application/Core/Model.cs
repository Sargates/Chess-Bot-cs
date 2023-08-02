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

		public void MakeMove(Move move, bool quiet = false) { //* Wrapper method for MovePiece, calls MovePiece and handles things like board history, 50 move rule, 3 move repition, 
			int movedFrom = move.StartSquare;
			int movedTo = move.TargetSquare;
			int moveFlag = move.MoveFlag;

			Piece pieceMoved = board.GetSquare(movedFrom);
			Piece pieceTaken = (moveFlag==Move.EnPassantCaptureFlag) ? (board.opponentColour(pieceMoved.Color)|Piece.Pawn) : board.GetSquare(movedTo);




			board.MovePiece(pieceMoved, movedFrom, movedTo);

			// If the move was an enpassant capture
			if (moveFlag == Move.EnPassantCaptureFlag) {
				board.board[board.enPassantIndex - board.forwardDir(pieceMoved.Color)] = 0;
			}

			board.enPassantIndex = -1; // Ok to set this to -1 here because of how En-Passant works
			// Set Enpassant square
			if (moveFlag == Move.PawnTwoUpFlag) {
				board.enPassantIndex = movedFrom + board.forwardDir(pieceMoved.Color);
			}

			// Is a promotion
			if (pieceMoved.Type == Piece.Pawn && BoardHelper.RankIndex(movedTo) == (pieceMoved.Color==Piece.White ? 7 : 0)) {
				board.board[movedTo] = Piece.Queen|pieceMoved.Color;
			}



			// If move is a castle, move rook
			if (moveFlag == Move.CastleFlag) {
				if (pieceMoved.Color == Piece.White) {
					switch (movedTo) {
						case BoardHelper.c1:
							board.MovePiece(Piece.Rook, BoardHelper.a1, BoardHelper.d1);
							break;
						case BoardHelper.g1:
							board.MovePiece(Piece.Rook, BoardHelper.h1, BoardHelper.f1);
							break;
					}
				}
				if (pieceMoved.Color == Piece.Black) {
					switch (movedTo) {
						case BoardHelper.c8:
							board.MovePiece(Piece.Rook, BoardHelper.a8, BoardHelper.d8);
							break;
						case BoardHelper.g8:
							board.MovePiece(Piece.Rook, BoardHelper.h8, BoardHelper.f8);
							break;
					}
				}
			}

			board.state.UpdateBoardRepr(board);

			// If piece moved is a king or a rook, update castle perms
			if (pieceMoved.Type == Piece.King) {
				if (pieceMoved.Color == Piece.White) {
					board.state.RemoveCastle(Gamestate.whiteKingCastle);
					board.state.RemoveCastle(Gamestate.whiteQueenCastle);
				}
				if (pieceMoved.Color == Piece.Black) {
					board.state.RemoveCastle(Gamestate.blackKingCastle);
					board.state.RemoveCastle(Gamestate.blackQueenCastle);
				}
			}
			if (pieceMoved.Type == Piece.Rook) {
				if (pieceMoved.Color == Piece.White) {
					if (movedFrom == BoardHelper.a1) {
						board.state.RemoveCastle(Gamestate.whiteQueenCastle);
					}
					if (movedFrom == BoardHelper.h1) {
						board.state.RemoveCastle(Gamestate.whiteKingCastle);
					}
				}
				if (pieceMoved.Color == Piece.Black) {
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
			if (pieceTaken == Piece.None || pieceMoved.Type == Piece.Pawn) { board.state.halfMoveCount = 0; }
			else { board.state.halfMoveCount += 1; }
			board.state.fullMoveCount += 1;

			board.whiteToMove = !board.whiteToMove; // ForwardDir / anything related to the active color will be the same up until this point
			board.state.fenColor = board.whiteToMove ? 'w' : 'b';
			
			board.state.SetRecentMove(move);

			if (! quiet) { Console.WriteLine(board.state.ToFEN()); }
			if (quiet) { board.state.GetFuture().Clear(); }
			board.state.PushHistory();
		}

		public void UnmakeMove() {
			PopHistory();
			board.state.PopFuture();
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



		public void StartNewGame(string fenString) {
			//* Instantiate starting gamestate
			//* Instantiate new Board passing starting gamestate
			//* Recalc bitboards
			//* 
			//* 
		}


	}
}