using ChessBot.Helpers;

namespace ChessBot.Engine {
	
	public class Board {

		public int[] board;
		public Gamestate state;

		public bool whiteToMove;
		public int enPassantIndex;


		public Board(string fen) {

			// TODO: Add FEN string loading, StartNewGame should be in Controller/Model.cs, board should just be able to load a fen string in place
			state = new Gamestate(fen);
			board = BoardFromFen(state.fenBoard);

			Console.WriteLine(state.ToFEN());

			whiteToMove = state.fenColor == 'w';
		}


		public void UpdateFromState() {
			board = BoardFromFen(state.fenBoard);
			whiteToMove = state.fenColor == 'w';
			enPassantIndex = BoardHelper.NameToSquareIndex(state.enpassantSquare);
			
		}

		public static int[] BoardFromFen(string fen) {
			int[] board = new int[64];
			int file = 0; int rank = 7;
			foreach (char c in fen) {
				if (c == '/') {rank -= 1; file = 0; continue;}
				int index = 8*rank+file;

				if (char.IsNumber(c)) {

					file += int.Parse($"{c}");
					continue;
				}

				board[index] = Gamestate.BoardCharToEnum(c);
				file += 1;
			}

			return board;
		}

		public int activeColor => whiteToMove ? PieceHelper.White : PieceHelper.Black;

        public int opponentColour(int color) => color ^ PieceHelper.White;
		public int forwardDir(int color) => color == PieceHelper.White ? 8 : -8;


		public void MovePiece(int piece, int movedFrom, int movedTo) {
			//* modify bitboards here
			
			int temp = board[movedTo];
			board[movedTo] = board[movedFrom];
			board[movedFrom] = PieceHelper.None;

		}

		
		public int GetSquare(int index) {
			if (! (0 <= index && index < 64) ) { throw new Exception("Board index out of bounds"); }
			
			return board[index];
		}
	}
}