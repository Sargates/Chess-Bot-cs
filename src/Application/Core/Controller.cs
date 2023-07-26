using Raylib_cs;
using System;
using System.IO;
using System.Numerics;
using System.Globalization;
using ChessBot.Engine;
using ChessBot.Engine.Helpers;
using ChessBot.Application.Helpers;


namespace ChessBot.Application {

	public class Controller {
		BoardUI boardUI;
		Board board;
		Vector2 screenSize;


		public Controller() {
			screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
			boardUI = new BoardUI();
			board = boardUI.board;
		}



		public void Draw() { // Refactor to draw BoardUI instead; `BoardUI.Draw() { BoardUI.DrawBoardStyling(); BoardUI.DrawPieces(); }



			boardUI.Draw();

			// for (int i=7; i>=0; i--) {
			// 	for (int j=0; j<8; j++) {
			// 		int index = 8*i+j;

			// 		// Console.Write($"{BoardHelper.NameToSquareIndex(BoardHelper.IndexToSquareName(index))} ");
			// 		int pieceEnum = board.GetSquare(index);
			// 		string p;
			// 		int boardSquare = board.GetSquare(index);
			// 		if (boardSquare == PieceHelper.None) {
			// 			p = (boardUI.squareColors[index] == ConsoleColor.White) ? "  " : "**";
			// 		} else {
			// 			p = PieceHelper.EnumToRepr[boardSquare];
			// 		}
			// 		ConsoleHelper.Write(p+" ", boardUI.squareColors[index]);
			// 	}
			// 	ConsoleHelper.WriteLine("");
			// }




		}

		public void Update() {

			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) {
				Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

				Vector2 boardPos = (pos/boardUI.squareSize)+new Vector2(4);
				
				if (! (0 <= boardPos.X && boardPos.X < 8 && 0 <= boardPos.Y && boardPos.Y < 8) ) {
					return;
				} // Passes guard clause if click is in bounds (on a board square)
				int squareClicked = 8*((int)(8-boardPos.Y))+(int)boardPos.X;

				if (boardUI.selectedIndex == -1) { // no piece selected
					int piece = board.GetSquare(squareClicked);
					if (piece == PieceHelper.None) {
						Console.WriteLine("No piece clicked");
						return;
					}
					// if (PieceHelper.GetColor(piece) != board.activeColor) {
					// 	Console.WriteLine("Wrong color clicked");
					// 	return;
					// }

					boardUI.selectedIndex = squareClicked;
					boardUI.movesForSelected = board.GetMoves(squareClicked);

				} else {
					if (squareClicked == boardUI.selectedIndex) {
						Console.WriteLine("Piece deselected");
						boardUI.selectedIndex = -1;
						boardUI.movesForSelected = new Move[0];
						return;
					}
					Move validMove = new Move(0);
					foreach (Move move in boardUI.movesForSelected) {
						if (move.TargetSquare == squareClicked) {
							Console.WriteLine($"{move} Is valid move");
							validMove = move;
							break;
						}
					}
					if (! validMove.IsNull) {
						board.MakeMove(validMove);
					}

					boardUI.selectedIndex = -1;
					boardUI.movesForSelected = new Move[0];
				}
				
			}
		}
	}
}