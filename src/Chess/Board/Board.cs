using ChessBot.Helpers;

namespace ChessBot.Engine {
	
	public class Board {

		public int[] board;
		// public readonly Gamestate state;

		public bool whiteToMove;
		public int enPassantIndex;


		public Board() {

			// TODO: Add FEN string loading, StartNewGame should be in Controller/Model.cs, board should just be able to load a fen string in place
			board = new int[64];
			board[0]  = PieceHelper.Rook 	| PieceHelper.White;
			board[1]  = PieceHelper.Knight 	| PieceHelper.White;
			board[2]  = PieceHelper.Bishop 	| PieceHelper.White;
			board[3]  = PieceHelper.Queen 	| PieceHelper.White;
			board[4]  = PieceHelper.King 	| PieceHelper.White;
			board[5]  = PieceHelper.Bishop 	| PieceHelper.White;
			board[6]  = PieceHelper.Knight 	| PieceHelper.White;
			board[7]  = PieceHelper.Rook 	| PieceHelper.White;

			board[8]  = PieceHelper.Pawn 	| PieceHelper.White;
			board[9]  = PieceHelper.Pawn 	| PieceHelper.White;
			board[10] = PieceHelper.Pawn 	| PieceHelper.White;
			board[11] = PieceHelper.Pawn 	| PieceHelper.White;
			board[12] = PieceHelper.Pawn 	| PieceHelper.White;
			board[13] = PieceHelper.Pawn 	| PieceHelper.White;
			board[14] = PieceHelper.Pawn 	| PieceHelper.White;
			board[15] = PieceHelper.Pawn 	| PieceHelper.White;

			board[48] = PieceHelper.Pawn 	| PieceHelper.Black;
			board[49] = PieceHelper.Pawn 	| PieceHelper.Black;
			board[50] = PieceHelper.Pawn 	| PieceHelper.Black;
			board[51] = PieceHelper.Pawn 	| PieceHelper.Black;
			board[52] = PieceHelper.Pawn 	| PieceHelper.Black;
			board[53] = PieceHelper.Pawn 	| PieceHelper.Black;
			board[54] = PieceHelper.Pawn 	| PieceHelper.Black;
			board[55] = PieceHelper.Pawn 	| PieceHelper.Black;

			board[56] = PieceHelper.Rook 	| PieceHelper.Black;
			board[57] = PieceHelper.Knight 	| PieceHelper.Black;
			board[58] = PieceHelper.Bishop 	| PieceHelper.Black;
			board[59] = PieceHelper.Queen 	| PieceHelper.Black;
			board[60] = PieceHelper.King 	| PieceHelper.Black;
			board[61] = PieceHelper.Bishop 	| PieceHelper.Black;
			board[62] = PieceHelper.Knight 	| PieceHelper.Black;
			board[63] = PieceHelper.Rook 	| PieceHelper.Black;

			whiteToMove = true;
		}

		public int activeColor => whiteToMove ? PieceHelper.White : PieceHelper.Black;
        public int opponentColour => whiteToMove ? PieceHelper.Black : PieceHelper.White;
		public int forwardDir(int color) => color == PieceHelper.White ? 8 : -8;


		public void MovePiece(int piece, int movedFrom, int movedTo) {
			//	modify bitboards here
			
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