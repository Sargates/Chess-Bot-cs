using ChessBot.Helpers;

namespace ChessBot.Engine {
	
	public class Board {

		public Piece[] board;

		public bool whiteToMove;
		public int enPassantIndex;

		public int whiteKingPos;
		public int blackKingPos;

		public LinkedList<Fen> stateHistory;
		public LinkedListNode<Fen> currentStateNode;
		public Fen currentFen;



		public Board(string fen="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {

			stateHistory = new LinkedList<Fen>();
			currentStateNode = new LinkedListNode<Fen>(new Fen(fen));
			stateHistory.AddLast(currentStateNode);
			currentFen = currentStateNode.Value;

			// TODO: Add FEN string loading, StartNewGame should be in Controller/Model.cs, board should just be able to load a fen string in place
            board = FenToBoard(this.currentFen.fenBoard);
			for (int i = 0; i < board.Length; i++) {
				if (board[i] == (Piece.White | Piece.King)) { whiteKingPos = i; }
				if (board[i] == (Piece.Black | Piece.King)) { blackKingPos = i; }
			}
			

            Console.WriteLine(this.currentFen.ToFEN());

            whiteToMove = this.currentFen.fenColor == 'w';
		}


		public void UpdateFromState() {
			board = FenToBoard(currentFen.fenBoard);
			for (int i = 0; i < board.Length; i++) {
				if (board[i] == (Piece.White | Piece.King)) { whiteKingPos = i; }
				if (board[i] == (Piece.Black | Piece.King)) { blackKingPos = i; }
			}
			whiteToMove = currentFen.fenColor == 'w';
			enPassantIndex = BoardHelper.NameToSquareIndex(currentFen.enpassantSquare);

		}


		public static Piece[] FenToBoard(string fen) {
			Piece[] board = new Piece[64];
			Piece file = 0; Piece rank = 7;
			foreach (char c in fen) {
				if (c == '/') {rank -= 1; file = 0; continue;}
				Piece index = 8*rank+file;

				if (char.IsNumber(c)) {

					file += int.Parse($"{c}");
					continue;
				}

				board[index] = BoardHelper.BoardCharToEnum(c);
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

			int castlesToRemove = 0b1111;
			// If piece moved is a king or a rook, update castle perms
			if (pieceMoved.Type == Piece.King) {
				if (pieceMoved.Color == Piece.White) {
					castlesToRemove -= Fen.whiteKingCastle;
					castlesToRemove -= Fen.whiteQueenCastle;
				}
				if (pieceMoved.Color == Piece.Black) {
					castlesToRemove -= Fen.blackKingCastle;
					castlesToRemove -= Fen.blackQueenCastle;
				}
			}
			if (pieceMoved.Type == Piece.Rook) {
				if (pieceMoved.Color == Piece.White) {
					if (movedFrom == BoardHelper.a1) {
						castlesToRemove -= Fen.whiteQueenCastle;
					}
					if (movedFrom == BoardHelper.h1) {
						castlesToRemove -= Fen.whiteKingCastle;
					}
				}
				if (pieceMoved.Color == Piece.Black) {
					if (movedFrom == BoardHelper.a8) {
						castlesToRemove -= Fen.blackQueenCastle;
					}
					if (movedFrom == BoardHelper.h8) {
						castlesToRemove -= Fen.blackKingCastle;
					}
				}
			}

			whiteToMove = !whiteToMove; // ForwardDir / anything related to the active color will be the same up until this point
			if (! quiet) {
				currentFen.moveMade = move;

				currentFen.enpassantSquare = (enPassantIndex==-1) ? "-" : BoardHelper.IndexToSquareName(enPassantIndex);
				if (pieceTaken == Piece.None || pieceMoved.Type == Piece.Pawn) { currentFen.halfMoveCount = 0; }
				else { currentFen.halfMoveCount += 1; }
				if (whiteToMove) currentFen.fullMoveCount += 1;

				currentFen.fenColor = whiteToMove ? 'w' : 'b';

				BoardHelper.UpdateFenAttachedToBoard(this);
				
				PushNewState(this.currentFen);
			}
		}
		public void MovePiece(Piece piece, int movedFrom, int movedTo) {
			//* modify bitboards here

			if (board[movedFrom] == (Piece.White | Piece.King)) { whiteKingPos = movedTo; }
			if (board[movedFrom] == (Piece.Black | Piece.King)) { blackKingPos = movedTo; }
			board[movedTo] = board[movedFrom];
			board[movedFrom] = Piece.None;
		}

		public void PushNewState(Fen newFen) {
			if (stateHistory.Last == null) {
				throw new Exception("`stateHistory.Last` is null");
			}
			while (stateHistory.Last != currentStateNode) { stateHistory.RemoveLast(); }
			stateHistory.AddLast(newFen);
			currentStateNode = stateHistory.Last;
		}

		public void SetPrevState() {
			if (currentStateNode.Previous == null) { Console.WriteLine("Cannot get previous state, is null"); return; }
			currentStateNode = currentStateNode.Previous;
			currentFen = currentStateNode.Value;
			UpdateFromState();
		}
		public void SetNextState() {
			if (currentStateNode.Next == null) { Console.WriteLine("Cannot get next state, is null"); return; }
			currentStateNode = currentStateNode.Next;
			currentFen = currentStateNode.Value;
			UpdateFromState();
		}
		
		public Piece GetSquare(int index) {
			if (! (0 <= index && index < 64) ) { throw new Exception("Board index out of bounds"); }
			return board[index];
		}

		
	}
}