// using Raylib_cs;
// using System.Numerics;
// using ChessBot.Engine;
// using ChessBot.Helpers;

// namespace ChessBot.Application;
// public static class Input {

// 	// Square selected on each interaction, -1 for invalid square
// 	// { leftDown, leftUp, rightDown, rightUp }
// 	public static int[] mouseClickInfo = {-1, -1, -1, -1};
// 	public static bool leftReleased, leftPressed, rightPressed, rightReleased;
// 	public static int pressedKey=0;

	
// 	public static void HandleKeyboardInput(BoardUI ui, Model model) {
// 		if (! (Input.pressedKey != 0)) { return; }

// 		switch (Input.pressedKey) {
// 			case (int) KeyboardKey.KEY_Z :{
// 				ui.DeselectActiveSquare();
// 				model.board.SetPrevState();
// 				break;
// 			}
// 			case (int) KeyboardKey.KEY_X :{
// 				ui.DeselectActiveSquare();
// 				model.board.SetNextState();
// 				break;
// 			}

// 			case (int) KeyboardKey.KEY_P :{
// 				// foreach (Fen fenString in model.board.stateHistory) {
// 				// 	ConsoleHelper.WriteLine($"{fenString}", ConsoleColor.Cyan);
// 				// }
// 				// Console.WriteLine(Raylib.GetMousePosition());
// 				// Console.WriteLine((new Vector2(ui.selectedIndex & 0b111, 7-(ui.selectedIndex >> 3)) - new Vector2(4, 4)) * BoardUI.squareSize);
// 				break;
// 			}
// 			default: {
// 				break;
// 			}
// 		}
// 	}

// 	public static void HandleMouseInput(BoardUI ui, Model model) {
// 		if (! (ui.activeAnimation is null && Input.leftPressed || Input.leftReleased || Input.rightPressed || Input.rightReleased)) { return; }

// 		// TODO Test how ineffective it would be to constantly update mousePos and check if mouse is on a square
// 		Piece clickedPiece = Piece.None;
// 		int squareClicked = -1;
// 		Move validMove = new Move(0);

// 		Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

// 		Vector2 boardPos = (pos/BoardUI.squareSize)+new Vector2(4);
		
// 		if ((0 <= boardPos.X && boardPos.X < 8 && 0 <= boardPos.Y && boardPos.Y < 8) ) {
// 			squareClicked = 8*((int)(8-boardPos.Y))+(int)boardPos.X;
// 			clickedPiece = model.board.GetSquare(squareClicked);
// 		} // If the interaction (click/release) is in bounds, set square clicked and clicked piece, otherwise they will be -1 and {Piece.None}

// 		if (squareClicked == -1) { // Case 1
// 			ui.DeselectActiveSquare();
// 			return;
// 		} // Passes guard clause if the click was in bounds

// 		if (ui.selectedIndex != -1) {
// 			foreach (Move move in ui.movesForSelected) {
// 				if (move == new Move(ui.selectedIndex, squareClicked)) {
// 					validMove = move;
// 					break;
// 				}
// 			}
// 		}

// 		if (Input.leftPressed) {
// 			mouseClickInfo[0] = squareClicked;
// 			ui.highlightedSquares = new bool[64];
// 			if (! validMove.IsNull ) { // Case 3
// 				ui.DeselectActiveSquare();
// 				model.board.MakeMove(validMove);
// 				ui.activeAnimation = new Animation(validMove.StartSquare, validMove.TargetSquare, model.board.GetSquare(validMove.TargetSquare), .12f);
// 				//* ANIMATION HERE
// 			} else
// 			if (ui.selectedIndex != -1 && squareClicked == ui.selectedIndex) { // Case 5
// 				ui.isDraggingPiece = true;
// 			} else
// 			if (ui.selectedIndex == -1 && clickedPiece != Piece.None) { // Case 2
// 				if (model.enforceColorToMove && clickedPiece.Color == model.board.activeColor) {
// 					ui.selectedIndex = squareClicked;
// 					ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
// 					ui.isDraggingPiece = true;
// 				}
// 			} else
// 			if (validMove.IsNull && clickedPiece.Type != Piece.None) { // Case 6
// 				if (model.enforceColorToMove && clickedPiece.Color == model.board.activeColor) {
// 					ui.selectedIndex = squareClicked;
// 					ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
// 					ui.isDraggingPiece = true;
// 				}
// 			} else
// 			if (validMove.IsNull) { // Case 4
// 				ui.DeselectActiveSquare();
// 			}
// 		}

// 		if (Input.leftReleased) {
// 			mouseClickInfo[1] = squareClicked;
// 			ui.isDraggingPiece = false;

// 			if (! validMove.IsNull) {
// 				ui.DeselectActiveSquare();
// 				model.board.MakeMove(validMove);
// 			}
// 			// Console.WriteLine(string.Join(", ", mouseClickInfo));
// 			mouseClickInfo[0] = -1; mouseClickInfo[1] = -1;
// 		}

// 		if (Input.rightPressed) {
// 			mouseClickInfo[2] = squareClicked;
// 			ui.DeselectActiveSquare();
// 			ui.isDraggingPiece = false;
// 		}

// 		if (Input.rightReleased) {
// 			mouseClickInfo[3] = squareClicked;
// 			if (ui.selectedIndex == -1 && mouseClickInfo[0] == -1) {
// 				ui.highlightedSquares[squareClicked] = ! ui.highlightedSquares[squareClicked];
// 			}
// 			// Console.WriteLine(string.Join(", ", mouseClickInfo));
// 			mouseClickInfo[2] = -1; mouseClickInfo[3] = -1;
// 		}

// 		//* Case 1: 	No square is selected, and square clicked is out of bounds 			=> call DeselectActiveSquare ✓
// 		//* Case 2: 	No square is selected and piece is clicked 							=> Set selectedIndex to square clicked ✘
// 		//* Case 3: 	Square is selected and square clicked is a valid move				=> call model.board.MakeMove ✘
// 		//* Case 4: 	Square is selected and square clicked is not a valid move			=> Deselect piece and fallthrough to case 7 ✘
// 		//* Case 5: 	Square is selected and square clicked == selected index				=> set isDragging to true ✘
// 		//* Case 6: 	Square is selected and clicked piece is the same color				=> Subset of Case 7 ✓
// 		//* Case 7: 	Square is selected and clicked piece is not in the valid moves		=> Superset of case 4 ✘
// 		//* Case 7.1: 	If clicked square is a piece, select that square
// 	}
// }
