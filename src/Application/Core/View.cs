using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class View {
		public BoardUI ui;
		public static Vector2 screenSize;
		public Model model;
		public Camera2D camera;
		public List<IInteractable> pipeline;

		public float fTimeElapsed = 0.0f;
		public float TimeOfLastMove = 0.0f;

		public List<(int tail, int head)> drawnArrows = new List<(int tail, int head)>();


		// Square selected on each interaction, -1 for invalid square
		// { leftDown, leftUp, rightDown, rightUp }
		public int[] mouseClickInfo = {-1, -1, -1, -1};
		// in the format of: leftReleased, leftPressed, rightPressed, rightReleased
		public static int mouseButtonsClicked; // 0b1111
		public static int pressedKey=0;
		
		public static bool IsLeftPressed => (mouseButtonsClicked & 8) == 8;
		public static bool IsLeftReleased => (mouseButtonsClicked & 4) == 4;
		public static bool IsRightPressed => (mouseButtonsClicked & 2) == 2;
		public static bool IsRightReleased => (mouseButtonsClicked & 1) == 1;

		public View(Vector2 screenSize, Model model, Camera2D cam) {
			ui = new BoardUI();
			View.screenSize = screenSize;
			this.model = model;
			this.model.enforceColorToMove = true;
			this.camera = cam;

			pipeline = new List<IInteractable>();

			AddButtons();
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

			fTimeElapsed += dt;
			if (ui.activeAnimation != null) {
				ui.activeAnimation.Update(dt);
				if (ui.activeAnimation.HasFinished) {
					ui.activeAnimation = null;
				}
			}

			
			if (pressedKey != 0) {
				HandleKeyboardInput();
			}
			if (ui.activeAnimation is null && mouseButtonsClicked > 0) {
				HandleMouseInput();
			}


			foreach (IInteractable asset in pipeline) {
				asset.Update();
			}

		}

		public void Draw() {

			Raylib.BeginMode2D(camera);

			ui.DrawBoardBorder();
			ui.DrawBoardSquares();
			ui.DrawPiecesOnBoard(model.board);

			ui.activeAnimation?.Draw(ui.isFlipped);

			DrawPlayerInfo();

			Raylib.EndMode2D();


			foreach (IInteractable asset in pipeline) {
				asset.Draw();
			}
		}




		public void DrawPlayerInfo() { // Draw this in Camera space
			// Draw player timer


			// Draw icon for player state (isSearching, hasStarted)
			// TODO: Improve this logic
			for (int i=0; i<2; i++) {
				ChessPlayer player = i==0 ? model.whitePlayer : model.blackPlayer;
				Vector2 displayPosition = new Vector2(500f, 350f * (ui.isFlipped ? -1 : 1) * (player == model.whitePlayer ? 1 : -1));
				Color playerInfoColor = ColorHelper.HexToColor("#2c2c2c"); // inactive color
				if (model.ActivePlayer == player) {
					playerInfoColor = ColorHelper.HexToColor("#79ff2b");
				}
				if (player.IsThreaded) { // Player is a computer
					Debug.Assert(player.Computer != null);
					if ((!player.Computer.HasStarted)) {
						playerInfoColor = ColorHelper.HexToColor("#ff4a4a"); // inactive color
					}
				}
				if (player.IsSearching) {
					int period = 1; // in seconds
					float bias = 0.2f; // percentage of period where output => 0
					// Neat little math function to set a bias of how long a value will be 0, bias of 0.2 means it will be 0 for 20% of the perios
					int output = (int) Math.Floor(1+(1/period)*((fTimeElapsed-TimeOfLastMove)%period) - bias); // No idea why Math.floor returns a double
					// With a bias of 0.2, the displayed color will be the first color, 20% of the time
					string[] sequencedColors = { "#2c2c2c", "#ffff00"};
					playerInfoColor = ColorHelper.HexToColor(sequencedColors[output]);
				}
				ui.DrawRectangleCentered(displayPosition, new Vector2(0.8f*BoardUI.squareSize), playerInfoColor);
			}
		}

		public void HandleMouseInput() {
			// TODO Test how ineffective it would be to constantly update mousePos and check if mouse is on a square
			Piece clickedPiece = Piece.None;
			int squareClicked = -1;
			Move validMove = new Move(0);

			Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

			Vector2 boardPos = (pos/BoardUI.squareSize);
			if (ui.isFlipped) { boardPos *= -1; }
			boardPos += new Vector2(4);

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
					Player.OnMoveChosen(validMove);
					TimeOfLastMove = fTimeElapsed;
				} else
				if (ui.selectedIndex != -1 && squareClicked == ui.selectedIndex) { // Case 5
					ui.isDraggingPiece = true;
				} else
				if (ui.selectedIndex == -1 && clickedPiece != Piece.None) { // Case 2
					ui.selectedIndex = squareClicked;
					ui.isDraggingPiece = true;
					bool isHumanColor = false;
					isHumanColor = isHumanColor || (clickedPiece.Color == Piece.White && (model.humanColor & 0b10) != 0);
					isHumanColor = isHumanColor || (clickedPiece.Color == Piece.Black && (model.humanColor & 0b01) != 0);
					isHumanColor = isHumanColor || Model.SuspendPlay; // if play is suspended, allow view to move pieces
					if (isHumanColor && clickedPiece.Color == model.board.activeColor) {
						ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
					}
				} else
				if (validMove.IsNull && clickedPiece.Type != Piece.None) { // Case 6
					bool isHumanColor = false;
					ui.selectedIndex = squareClicked;
					ui.isDraggingPiece = true;
					ui.movesForSelected = new Move[0];
					isHumanColor = isHumanColor || (clickedPiece.Color == Piece.White && (model.humanColor & 0b10) != 0);
					isHumanColor = isHumanColor || (clickedPiece.Color == Piece.Black && (model.humanColor & 0b01) != 0);
					isHumanColor = isHumanColor || Model.SuspendPlay; // if play is suspended, allow view to move pieces
					if (isHumanColor && clickedPiece.Color == model.board.activeColor) {
						ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
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
					Player.OnMoveChosen(validMove, false); // Do not animate a move made by a release
					TimeOfLastMove = fTimeElapsed;
				}
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

		public void HandleKeyboardInput() {
			switch (pressedKey) {
				case (int) KeyboardKey.KEY_Z :{
					ui.DeselectActiveSquare();
					Piece[] old = model.board.board.ToArray();
					model.SetPrevState();
					model.SetPrevState();
					ui.activeAnimation = new BoardAnimation(old, model.board.board, 0.08f);
					break;
				}
				case (int) KeyboardKey.KEY_X :{
					ui.DeselectActiveSquare();
					Piece[] old = model.board.board.ToArray();
					model.SetNextState();
					model.SetNextState();
					ui.activeAnimation = new BoardAnimation(old, model.board.board, 0.08f);
					break;
				}
				case (int) KeyboardKey.KEY_C :{
					ui.DeselectActiveSquare();
					Model.SuspendPlay = ! Model.SuspendPlay;
					break;
				}

				case (int) KeyboardKey.KEY_P :{
					Console.WriteLine();
					Console.WriteLine(model.board.GetUCIGameFormat());
					break;
				}
				case (int) KeyboardKey.KEY_O :{
					LinkedListNode<Fen>? currNode = model.board.stateHistory.First;
					while (currNode != null) {
						if (currNode == model.board.currentStateNode) {
							ConsoleHelper.WriteLine($"{currNode.Value} {currNode.Value.moveMade}", ConsoleColor.Red);
						} else
						if (currNode == model.board.currentStateNode.Previous || currNode == model.board.currentStateNode.Next) {
							ConsoleHelper.WriteLine($"{currNode.Value} {currNode.Value.moveMade}", ConsoleColor.Yellow);
						} else {
							ConsoleHelper.WriteLine($"{currNode.Value} {currNode.Value.moveMade}");
						}
						currNode = currNode.Next;
					}
					break;
				}
				case (int) KeyboardKey.KEY_I :{
					Console.WriteLine($"White: {model.whitePlayer.UCI}, {model.whitePlayer.Computer}, {model.whitePlayer.Player}");
					Console.WriteLine($"Black: {model.blackPlayer.UCI}, {model.blackPlayer.Computer}, {model.blackPlayer.Player}");
					// Console.WriteLine(model.blackPlayer.UCI?.engine.GetBoardVisual());
					// Console.WriteLine($"Current player: {ActivePlayer}, {ActivePlayer.IsSearching}");
					break;
				}
				default: {
					break;
				}
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

		public void AddButtons() {

			// AddToPipeline(new Button(new Rectangle(40, 300, 210, 50), "Set Fish Elo 3000").SetCallback(() => {
			// 	model.whitePlayer.UCI?.engine.SetElo(3000);
			// 	model.blackPlayer.UCI?.engine.SetElo(3000);
			// })); //* I don't really have a way to tell if `SetElo` even works until I make my own bot and test it
			// AddToPipeline(new Button(new Rectangle(40, 360, 210, 50), "Set Fish Elo 600").SetCallback(() => {
			// 	model.whitePlayer.UCI?.engine.SetElo(600);
			// 	model.blackPlayer.UCI?.engine.SetElo(600);
			// }));
			AddToPipeline(new Button(new Rectangle(40, 420, 210, 50), "Freeplay").SetCallback(() => {
				model.StartNewGame();
				// ui.activeAnimation = new BoardAnimation(model.oldBoard, model.board.board, 0.2f);
			}));
			AddToPipeline(new Button(new Rectangle(40, 480, 210, 50), "Human vs. Gatesfish").SetCallback(() => {
				model.StartNewGame(Model.Gametype.HvC);
				// ui.activeAnimation = new BoardAnimation(model.oldBoard, model.board.board, 0.2f);
			}));
			AddToPipeline(new Button(new Rectangle(40, 540, 210, 50), "Human vs. Stockfish").SetCallback(() => {
				model.StartNewGame(Model.Gametype.HvU);
				// ui.activeAnimation = new BoardAnimation(model.oldBoard, model.board.board, 0.2f);
			}));
			AddToPipeline(new Button(new Rectangle(40, 600, 210, 50), "Stockfish vs. Stockfish").SetCallback(() => {
				model.StartNewGame(Model.Gametype.UvU);
				// ui.activeAnimation = new BoardAnimation(model.oldBoard, model.board.board, 0.2f);
			}));

			// AddToPipeline(new Button(new Rectangle(40, 420, 210, 50), "Flip Board").SetCallback(() => {
			// 	ui.isFlipped = ! ui.isFlipped;
			// }));

			// // 2x3 array of buttons for player types
			// AddToPipeline(new Button(new Rectangle(40,  660, 60, 50), "HvH").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.HvH);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(115, 660, 60, 50), "HvC").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.HvC);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(190, 660, 60, 50), "HvU").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.HvU);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(40,  720, 60, 50), "CvC").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.CvC);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(115, 720, 60, 50), "CvU").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.CvU);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(190, 720, 60, 50), "UvU").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.UvU);
			// 	UpdateIsFlipped();
			// }));
		}
		public void Release() {
			ui.Release();
		}
	}
}