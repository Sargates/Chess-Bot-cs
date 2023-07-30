using Raylib_cs;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class View {
		public BoardUI board;
		public static Vector2 screenSize;
		public Model model;
		public Animation? activeAnimation;
		int squareClicked;

		// Animation? 
		public View(Vector2 screenSize, Model model) {
			board = new BoardUI();
			View.screenSize = screenSize;
			this.model = model;
		}

		public void Update() {
			board.DrawBoardBorder();
			board.DrawBoardSquares();
			board.DrawPiecesOnBoard(model.board);
			board.ResetBoardColors();


			HandleInput();

		}

		public void HandleInput() {
			// TODO Test how ineffective it would be to constantly update mousePos and check if mouse is on a square
			int clickedPiece=0;
			squareClicked = -1;
			Move validMove = new Move(0);
			bool leftReleased, leftPressed, rightPressed;
			leftPressed = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT);
			leftReleased = Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT);
			rightPressed = Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT);

			if (leftPressed || leftReleased) {
				Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

				Vector2 boardPos = (pos/board.squareSize)+new Vector2(4);
				
				if ((0 <= boardPos.X && boardPos.X < 8 && 0 <= boardPos.Y && boardPos.Y < 8) ) {
					squareClicked = 8*((int)(8-boardPos.Y))+(int)boardPos.X;
					clickedPiece = model.board.GetSquare(squareClicked);
				} // If the interaction (click/release) is in bounds, set square clicked and clicked piece, otherwise they will be -1 and {PieceHelper.None}

				if (squareClicked == -1) { // Case 1
					board.DeselectActiveSquare();
					return;
				} // Passes guard clause if the click was in bounds

				if (board.selectedIndex != -1) {
					foreach (Move move in board.movesForSelected) {
						if (move == new Move(board.selectedIndex, squareClicked)) {
							validMove = move;
							break;
						}
					}
				}

				if (leftPressed) {
					if (board.selectedIndex == -1 && clickedPiece != PieceHelper.None) { // Case 2
						board.selectedIndex = squareClicked;
						board.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
						board.isDraggingPiece = true;
					} else if (! validMove.IsNull ) { // Case 3
						board.DeselectActiveSquare();
						model.MakeMove(validMove);
						//* ANIMATION HERE
					} else if (board.selectedIndex != -1 && squareClicked == board.selectedIndex) { // Case 5
						board.isDraggingPiece = true;
					} else if (validMove.IsNull && PieceHelper.GetType(clickedPiece) != PieceHelper.None) { // Case 6
						board.selectedIndex = squareClicked;
						board.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
						board.isDraggingPiece = true;
					} else if (validMove.IsNull) { // Case 4
						board.DeselectActiveSquare();
					}

				} else if (leftReleased) {
					board.isDraggingPiece = false;

					if (! validMove.IsNull) {
						board.DeselectActiveSquare();
						model.MakeMove(validMove);
					}
				}
			}

			if (rightPressed) {
				board.DeselectActiveSquare();
				board.isDraggingPiece = false;
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