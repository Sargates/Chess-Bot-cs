using ChessBot.Helpers;

namespace ChessBot.Engine {
	
	public class Board {

		public Piece[] board;
		public Gamestate state;

		public bool whiteToMove;
		public int enPassantIndex;

		public int whiteKingPos;
		public int blackKingPos;
		




		public Board(string fen) {

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

        public int opponentColour(int color) => color ^ Piece.White;
		public int forwardDir(int color) => color == Piece.White ? 8 : -8;


		public void MovePiece(Piece piece, int movedFrom, int movedTo) {
			//* modify bitboards here

			if (board[movedFrom] == (Piece.White | Piece.King)) { whiteKingPos = movedTo; }
			if (board[movedFrom] == (Piece.Black | Piece.King)) { blackKingPos = movedTo; }
			
			int temp = board[movedTo];
			board[movedTo] = board[movedFrom];
			board[movedFrom] = Piece.None;



		}

		
		public Piece GetSquare(int index) {
			if (! (0 <= index && index < 64) ) { throw new Exception("Board index out of bounds"); }
			return board[index];
		}
	}
}