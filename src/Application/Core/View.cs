using Raylib_cs;
using System.Numerics;

namespace ChessBot.Application {
	public class View {
		public BoardUI board;
		public static Vector2 screenSize;
		public View(Vector2 screenSize) {
			board = new BoardUI();
			View.screenSize = screenSize;
		}

		public void Update(Model model) {
			board.DrawBoardBorder();
			board.DrawBoardSquares();
			board.DrawPiecesOnBoard(model.board);
			board.ResetBoardColors();

			
		}
	}
}