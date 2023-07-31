using Raylib_cs;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class View {
		public BoardUI ui;
		public static Vector2 screenSize;
		public Model model;
		public Animation? activeAnimation;
		int squareClicked;

		// Animation? 
		public View(Vector2 screenSize, Model model) {
			ui = new BoardUI();
			View.screenSize = screenSize;
			this.model = model;
			this.model.enforceColorToMove = true;
		}

		public void Update() {
			ui.DrawBoardBorder();
			ui.DrawBoardSquares();
			ui.DrawPiecesOnBoard(model.board);
			ui.ResetBoardColors();


			HandleMouseInput();
			HandleKeyboardInput();

		}

		public void HandleKeyboardInput() {
			int pressedKey = Raylib.GetKeyPressed();
			while (pressedKey != 0) {

				switch (pressedKey) {
					case (int) KeyboardKey.KEY_Z :{
						ui.DeselectActiveSquare();
						model.PopHistory();
						break;
					}
					case (int) KeyboardKey.KEY_X :{
						ui.DeselectActiveSquare();
						model.PopFuture();
						break;
					}
					case (int) KeyboardKey.KEY_P :{
						Console.WriteLine(model.board.state.PeekHistory());
						Console.WriteLine(model.board.state.GetHistory().Count);
						Console.WriteLine(model.board.state.GetFuture().Count);
						break;
					}
					default: {
						break;
					}
				}
				
				
				
				
				pressedKey = Raylib.GetKeyPressed();
			}
		}

		public void HandleMouseInput() {
			// TODO Test how ineffective it would be to constantly update mousePos and check if mouse is on a square
			int clickedPiece=0;
			squareClicked = -1;
			Move validMove = new Move(0);
			bool leftReleased, leftPressed, rightPressed;
			bool isSquareSelectedBeforeClick = ui.selectedIndex != -1;
			leftPressed = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
			leftReleased = Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT);
			rightPressed = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT);

			if (leftPressed || leftReleased) {
				Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

				Vector2 boardPos = (pos/ui.squareSize)+new Vector2(4);
				
				if ((0 <= boardPos.X && boardPos.X < 8 && 0 <= boardPos.Y && boardPos.Y < 8) ) {
					squareClicked = 8*((int)(8-boardPos.Y))+(int)boardPos.X;
					clickedPiece = model.board.GetSquare(squareClicked);
				} // If the interaction (click/release) is in bounds, set square clicked and clicked piece, otherwise they will be -1 and {PieceHelper.None}

				if (squareClicked == -1) { // Case 1
					ui.DeselectActiveSquare();
					return;
				} // Passes guard clause if the click was in bounds

				if (ui.selectedIndex != -1) {
					foreach (Move move in ui.movesForSelected) {
						if (move == new Move(ui.selectedIndex, squareClicked)) {
							validMove = move;
							break;
						}
					}
				}

				if (leftPressed) {
					if (! validMove.IsNull ) { // Case 3
						ui.DeselectActiveSquare();
						model.MakeMove(validMove);
						//* ANIMATION HERE
					} else
					if (ui.selectedIndex != -1 && squareClicked == ui.selectedIndex) { // Case 5
						ui.isDraggingPiece = true;
					} else
					if (ui.selectedIndex == -1 && clickedPiece != PieceHelper.None) { // Case 2
						if (model.enforceColorToMove && PieceHelper.GetColor(clickedPiece) == model.board.activeColor) {
							ui.selectedIndex = squareClicked;
							ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
							ui.isDraggingPiece = true;
						}
					} else
					if (validMove.IsNull && PieceHelper.GetType(clickedPiece) != PieceHelper.None) { // Case 6
						if (model.enforceColorToMove && PieceHelper.GetColor(clickedPiece) == model.board.activeColor) {
							ui.selectedIndex = squareClicked;
							ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
							ui.isDraggingPiece = true;
						}
					} else
					if (validMove.IsNull) { // Case 4
						ui.DeselectActiveSquare();
					}

				} else if (leftReleased) {
					ui.isDraggingPiece = false;

					if (! validMove.IsNull) {
						ui.DeselectActiveSquare();
						model.MakeMove(validMove);
					}
				}
			}

			if (rightPressed) {
				ui.DeselectActiveSquare();
				ui.isDraggingPiece = false;
			}

			//* Case 1: 	No square is selected, and square clicked is out of bounds 			=> call DeselectActiveSquare ✓
			//* Case 2: 	No square is selected and piece is clicked 							=> Set selectedIndex to square clicked ✘
			//* Case 3: 	Square is selected and square clicked is a valid move				=> call model.MakeMove ✘
			//* Case 4: 	Square is selected and square clicked is not a valid move			=> Deselect piece and fallthrough to case 7 ✘
			//* Case 5: 	Square is selected and square clicked == selected index				=> set isDragging to true ✘
			//* Case 6: 	Square is selected and clicked piece is the same color				=> Subset of Case 7 ✓
			//* Case 7: 	Square is selected and clicked piece is not in the valid moves		=> Superset of case 4 ✘
			//* Case 7.1: 	If clicked square is a piece, select that square
		}
	}
}