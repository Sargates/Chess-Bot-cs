using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class Model {

		public Board board;
		
		public Model() {
			board = new Board();
		}

		public void MakeMove(Move move) { //	Wrapper method for MovePiece, calls MovePiece and handles things like board history, 50 move rule, 3 move repition, 
			int movedFrom = move.StartSquare;
			int movedTo = move.TargetSquare;
			int moveFlag = move.MoveFlag;

			int pieceMoved = board.GetSquare(movedFrom);
			int pieceTaken = (moveFlag==Move.EnPassantCaptureFlag) ? (board.opponentColour|PieceHelper.Pawn) : board.GetSquare(movedTo);



			board.MovePiece(pieceMoved, movedFrom, movedTo);

			if (moveFlag == Move.EnPassantCaptureFlag) {
				board.board[board.enPassantIndex - board.forwardDir(PieceHelper.GetColor(pieceMoved))] = 0;
			}

			board.enPassantIndex = 0; // Ok to set this to 0 here because of how En-Passant works
			if (moveFlag == Move.PawnTwoUpFlag) {
				board.enPassantIndex = movedFrom + board.forwardDir(PieceHelper.GetColor(pieceMoved));
				Console.WriteLine(movedFrom + "" + board.forwardDir(PieceHelper.GetColor(pieceMoved)));
			}

			
			
			board.whiteToMove = !board.whiteToMove; // ForwardDir / anything related to the active color will be the same up until this point
			
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


	}
}