using Raylib_cs;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class View {
		public BoardUI ui;
		public static Vector2 screenSize;
		public Model model;
		public Camera2D camera;

		// Square selected on each interaction, -1 for invalid square
		// { leftDown, leftUp, rightDown, rightUp }
		public int[] mouseClickInfo = {-1, -1, -1, -1};
		// in the format of: leftReleased, leftPressed, rightPressed, rightReleased
		public static int mouseButtonsClicked; // 0b1111
		public static int pressedKey=0;

		public List<(int tail, int head)> drawnArrows = new List<(int tail, int head)>();

		public List<IInteractable> pipeline;

		public static bool IsLeftPressed => (View.mouseButtonsClicked & 8) == 8;
		public static bool IsLeftReleased => (View.mouseButtonsClicked & 4) == 4;
		public static bool IsRightPressed => (View.mouseButtonsClicked & 2) == 2;
		public static bool IsRightReleased => (View.mouseButtonsClicked & 1) == 1;




		// Animation? 
		public View(Vector2 screenSize, Model model, Camera2D cam) {
			ui = new BoardUI();
			View.screenSize = screenSize;
			this.model = model;
			this.model.enforceColorToMove = true;
			this.camera = cam;

			pipeline = new List<IInteractable>();

			Button button = new Button(new Rectangle(40, 600, 200, 50), "Button");
			button.OnLeftPressed = () => {
				Piece[] oldBoard = model.board.board.ToArray();
				model.StartNewGame(Fen.startpos);
				ui.activeAnimation = new BoardAnimation(oldBoard, model.board.board, .12f);
			};
			AddToPipeline(button);
		}

		public void AddToPipeline(IInteractable interactable) {
			pipeline.Add(interactable);
		}




		public void Update(float dt) {
			mouseButtonsClicked = 0;
			mouseButtonsClicked += Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)   ? 8 : 0;
			mouseButtonsClicked += Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)  ? 4 : 0;
			mouseButtonsClicked += Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)  ? 2 : 0;
			mouseButtonsClicked += Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT) ? 1 : 0;
			pressedKey = Raylib.GetKeyPressed();

			Raylib.BeginMode2D(camera);

			ui.DrawBoardBorder();
			ui.DrawBoardSquares();
			ui.DrawPiecesOnBoard(model.board);
			ui.ResetBoardColors();

			if (ui.activeAnimation != null) {
				ui.activeAnimation.Update(dt);
				ui.activeAnimation.Draw();
				if (ui.activeAnimation.HasFinished) {
					ui.activeAnimation = null;
				}
			}
			Raylib.EndMode2D();

			foreach (IInteractable asset in pipeline) {
				asset.Update();
				asset.Draw();
			}



			if (pressedKey != 0) {
				HandleKeyboardInput();
			}
			if (ui.activeAnimation is null && mouseButtonsClicked > 0) {
				HandleMouseInput();
			}
		}

		public void GetAttackedSquares() {
			string o = "";

			for (int i=0;i<8;i++) {
				for (int j=0;j<8;j++) {
					int index = 8*(7-i)+j;

					Piece piece = model.board.GetSquare(index);

					string symbol = MoveGenerator.IsSquareAttacked(model.board, index, Piece.White) ? "**" : "--";
					
					o += $"{symbol} ";
				}
				Console.WriteLine(o);
				o = "";
			}
		}

		public void HandleKeyboardInput() {
			switch (pressedKey) {
				case (int) KeyboardKey.KEY_Z :{
					ui.DeselectActiveSquare();
					model.board.SetPrevState();
					break;
				}
				case (int) KeyboardKey.KEY_X :{
					ui.DeselectActiveSquare();
					model.board.SetNextState();
					break;
				}

				case (int) KeyboardKey.KEY_P :{
					Console.WriteLine(Convert.ToString(model.board.currentFen.castlePrivsBin, 2));
					// foreach (Fen fenString in model.board.stateHistory) {
					// 	ConsoleHelper.WriteLine($"{fenString}", ConsoleColor.Cyan);
					// }
					// Console.WriteLine(Raylib.GetMousePosition());
					// Console.WriteLine((new Vector2(ui.selectedIndex & 0b111, 7-(ui.selectedIndex >> 3)) - new Vector2(4, 4)) * BoardUI.squareSize);
					break;
				}
				default: {
					break;
				}
			}
		}

		public void HandleMouseInput() {
			// TODO Test how ineffective it would be to constantly update mousePos and check if mouse is on a square
			Piece clickedPiece = Piece.None;
			int squareClicked = -1;
			Move validMove = new Move(0);

			Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

			Vector2 boardPos = (pos/BoardUI.squareSize)+new Vector2(4);
			
			if ((0 <= boardPos.X && boardPos.X < 8 && 0 <= boardPos.Y && boardPos.Y < 8) ) {
				squareClicked = 8*((int)(8-boardPos.Y))+(int)boardPos.X;
				clickedPiece = model.board.GetSquare(squareClicked);
			} // If the interaction (click/release) is in bounds, set square clicked and clicked piece, otherwise they will be -1 and {Piece.None}

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

			if (IsLeftPressed) {
				mouseClickInfo[0] = squareClicked;
				ui.highlightedSquares = new bool[64];
				if (! validMove.IsNull ) { // Case 3
					ui.DeselectActiveSquare();
					Piece[] oldState = model.board.board.ToArray();
					model.board.MakeMove(validMove);
					ui.activeAnimation = new BoardAnimation(oldState, model.board.board, .12f);
					//* ANIMATION HERE
				} else
				if (ui.selectedIndex != -1 && squareClicked == ui.selectedIndex) { // Case 5
					ui.isDraggingPiece = true;
				} else
				if (ui.selectedIndex == -1 && clickedPiece != Piece.None) { // Case 2
					if (model.enforceColorToMove && clickedPiece.Color == model.board.activeColor) {
						ui.selectedIndex = squareClicked;
						ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
						ui.isDraggingPiece = true;
					}
				} else
				if (validMove.IsNull && clickedPiece.Type != Piece.None) { // Case 6
					if (model.enforceColorToMove && clickedPiece.Color == model.board.activeColor) {
						ui.selectedIndex = squareClicked;
						ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
						ui.isDraggingPiece = true;
					}
				} else
				if (validMove.IsNull) { // Case 4
					ui.DeselectActiveSquare();
				}
			}

			if (IsLeftReleased) {
				mouseClickInfo[1] = squareClicked;
				ui.isDraggingPiece = false;

				if (! validMove.IsNull) {
					ui.DeselectActiveSquare();
					model.board.MakeMove(validMove);
				}
				// Console.WriteLine(string.Join(", ", mouseClickInfo));
				mouseClickInfo[0] = -1; mouseClickInfo[1] = -1;
			}

			if (IsRightPressed) {
				mouseClickInfo[2] = squareClicked;
				ui.DeselectActiveSquare();
				ui.isDraggingPiece = false;
			}

			if (IsRightReleased) {
				mouseClickInfo[3] = squareClicked;
				if (ui.selectedIndex == -1 && mouseClickInfo[0] == -1) {
					ui.highlightedSquares[squareClicked] = ! ui.highlightedSquares[squareClicked];
				} else
				if (true) {
					drawnArrows.Add((mouseClickInfo[2], mouseClickInfo[3]));
				}
				// Console.WriteLine(string.Join(", ", mouseClickInfo));
				mouseClickInfo[2] = -1; mouseClickInfo[3] = -1;
			}

			//* Case 1: 	No square is selected, and square clicked is out of bounds 			=> call DeselectActiveSquare ✓
			//* Case 2: 	No square is selected and piece is clicked 							=> Set selectedIndex to square clicked ✘
			//* Case 3: 	Square is selected and square clicked is a valid move				=> call model.board.MakeMove ✘
			//* Case 4: 	Square is selected and square clicked is not a valid move			=> Deselect piece and fallthrough to case 7 ✘
			//* Case 5: 	Square is selected and square clicked == selected index				=> set isDragging to true ✘
			//* Case 6: 	Square is selected and clicked piece is the same color				=> Subset of Case 7 ✓
			//* Case 7: 	Square is selected and clicked piece is not in the valid moves		=> Superset of case 4 ✘
			//* Case 7.1: 	If clicked square is a piece, select that square
		}

		public void Release() {
			ui.Release();
		}
	}
}