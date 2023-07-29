using ChessBot.Helpers;

namespace ChessBot.Engine {
	
	public class Board {

		public int[] board;

		public bool whiteToMove;
		public int enPassantIndex;


		public Board() {

			// TODO: Add FEN string loading, StartNewGame should be in Controller.cs, board should just be able to load a fen string in place
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
			board[27] = PieceHelper.Pawn 	| PieceHelper.Black;
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

		public void MakeMove(Move move) { //	Wrapper method for MovePiece, calls MovePiece and handles things like board history, 50 move rule, 3 move repition, 
			int movedFrom = move.StartSquare;
			int movedTo = move.TargetSquare;
			int moveFlag = move.MoveFlag;

			int pieceMoved = GetSquare(movedFrom);
			int pieceTaken = (moveFlag==Move.EnPassantCaptureFlag) ? (opponentColour|PieceHelper.Pawn) : GetSquare(movedTo);



			MovePiece(pieceMoved, movedFrom, movedTo);

			
			if (moveFlag == Move.EnPassantCaptureFlag) {
				board[enPassantIndex - forwardDir(activeColor)] = 0;
			}



			
			enPassantIndex = 0; // Ok to set this to 0 here because of how En-Passant works
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