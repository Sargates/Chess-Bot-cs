using Raylib_cs;
using System.IO;
using System.Numerics;
using System.Diagnostics;
using ChessBot.Engine;
using ChessBot.Helpers;


namespace ChessBot.Application {
	using static MoveGenerator;


	public class Controller {
		Vector2 screenSize;
		static Camera2D cam;
		Model model;
		View view;
		public static Random random = new Random();
		
		public static bool SuspendPlay = false;

		// Square selected on each interaction, -1 for invalid square
		// { leftDown, leftUp, rightDown, rightUp }
		public int[] mouseClickInfo = {-1, -1, -1, -1};
		// in the format of: leftReleased, leftPressed, rightPressed, rightReleased
		public static int mouseButtonsClicked; // 0b1111
		public static int pressedKey=0;
		
		public static bool IsLeftPressed => (Controller.mouseButtonsClicked & 8) == 8;
		public static bool IsLeftReleased => (Controller.mouseButtonsClicked & 4) == 4;
		public static bool IsRightPressed => (Controller.mouseButtonsClicked & 2) == 2;
		public static bool IsRightReleased => (Controller.mouseButtonsClicked & 1) == 1;

		
		// Return active player based on color passed, throw error if invalid color
		public ChessPlayer GetPlayerFromColor(char color) => ("wb".IndexOf(color) == -1) ? throw new Exception("Invalid Color") : (color == 'w') ? model.whitePlayer : model.blackPlayer;
		


		public Controller() {

			Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
			Raylib.SetTraceLogLevel(TraceLogLevel.LOG_FATAL); // Ignore Raylib Errors unless fatal
			Raylib.InitWindow(1600, 900, "Chess");

			Debug.Assert(random != null);

			cam = new Camera2D();
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            cam.target = new Vector2(0, 0);
            cam.offset = new Vector2(screenWidth / 2f, screenHeight / 2f);
            cam.zoom = 1.0f;

			Player.OnMoveChosen += MakeMove;
			screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
			model = new Model();
			view = new View(screenSize, model, cam);
			Model.NewGameCalls += () => {
				view.ui.isFlipped = model.humanColor == 0b01; // if white is not a player and black is a player
			};

		}

		public void MainLoop() {
			float dt = 0f;

			while (!Raylib.WindowShouldClose()) {
				dt = Raylib.GetFrameTime();
				mouseButtonsClicked = 0;
				mouseButtonsClicked += Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)   ? 8 : 0;
				mouseButtonsClicked += Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)  ? 4 : 0;
				mouseButtonsClicked += Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)  ? 2 : 0;
				mouseButtonsClicked += Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT) ? 1 : 0;
				pressedKey = Raylib.GetKeyPressed();

				if (Raylib.IsWindowResized()) {
            		view.camera.offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
					View.screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
				}

				Raylib.BeginDrawing();
				Raylib.ClearBackground(new Color(22, 22, 22, 255));
				Raylib.DrawFPS(10, 10);
				
				model.Update();
				view.Update(dt);

				if ((model.ActivePlayer.Computer?.HasStarted ?? true) && ! model.ActivePlayer.IsSearching && ! SuspendPlay) {
					model.ActivePlayer.IsSearching = true;
				}

				if (pressedKey != 0) {
					HandleKeyboardInput();
				}
				if (view.ui.activeAnimation is null && mouseButtonsClicked > 0) {
					HandleMouseInput();
				}

				//* Draw menu here

				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();
			model.ExitPlayerThreads();
			model.JoinPlayerThreads();
			

			view.Release();
            UIHelper.Release();
		}

		public void HandleMouseInput() {
			// TODO Test how ineffective it would be to constantly update mousePos and check if mouse is on a square
			Piece clickedPiece = Piece.None;
			int squareClicked = -1;
			Move validMove = new Move(0);

			Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

			Vector2 boardPos = (pos/BoardUI.squareSize);
			if (view.ui.isFlipped) { boardPos *= -1; }
			boardPos += new Vector2(4);

			if ((0 <= boardPos.X && boardPos.X < 8 && 0 <= boardPos.Y && boardPos.Y < 8) ) {
				squareClicked = 8*((int)(8-boardPos.Y))+(int)boardPos.X;
				clickedPiece = model.board.GetSquare(squareClicked);
			} // If the interaction (click/release) is in bounds, set square clicked and clicked piece, otherwise they will be -1 and {Piece.None}

			if (squareClicked == -1) { // Case 1
				view.ui.DeselectActiveSquare();
				return;
			} // Passes guard clause if the click was in bounds

			if (view.ui.selectedIndex != -1) {
				foreach (Move move in view.ui.movesForSelected) {
					if (move == new Move(view.ui.selectedIndex, squareClicked)) {
						validMove = move;
						break;
					}
				}
			}

			if (IsLeftPressed) {
				mouseClickInfo[0] = squareClicked;
				view.ui.highlightedSquares = new bool[64];
				if (! validMove.IsNull ) { // Case 3
					view.ui.DeselectActiveSquare();
					MakeMove(validMove);
				} else
				if (view.ui.selectedIndex != -1 && squareClicked == view.ui.selectedIndex) { // Case 5
					view.ui.isDraggingPiece = true;
				} else
				if (view.ui.selectedIndex == -1 && clickedPiece != Piece.None) { // Case 2
					view.ui.selectedIndex = squareClicked;
					view.ui.isDraggingPiece = true;
					bool isHumanColor = false;
					isHumanColor = isHumanColor || (clickedPiece.Color == Piece.White && (model.humanColor & 0b10) != 0);
					isHumanColor = isHumanColor || (clickedPiece.Color == Piece.Black && (model.humanColor & 0b01) != 0);
					isHumanColor = isHumanColor || SuspendPlay; // if play is suspended, allow view to move pieces
					if (isHumanColor && clickedPiece.Color == model.board.activeColor) {
						view.ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
					}
				} else
				if (validMove.IsNull && clickedPiece.Type != Piece.None) { // Case 6
					bool isHumanColor = false;
					view.ui.selectedIndex = squareClicked;
					view.ui.isDraggingPiece = true;
					view.ui.movesForSelected = new Move[0];
					isHumanColor = isHumanColor || (clickedPiece.Color == Piece.White && (model.humanColor & 0b10) != 0);
					isHumanColor = isHumanColor || (clickedPiece.Color == Piece.Black && (model.humanColor & 0b01) != 0);
					isHumanColor = isHumanColor || SuspendPlay; // if play is suspended, allow view to move pieces
					if (isHumanColor && clickedPiece.Color == model.board.activeColor) {
						view.ui.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
					}
				} else
				if (validMove.IsNull) { // Case 4
					view.ui.DeselectActiveSquare();
				}
			}

			if (IsLeftReleased) {
				mouseClickInfo[1] = squareClicked;
				view.ui.isDraggingPiece = false;

				if (! validMove.IsNull) {
					view.ui.DeselectActiveSquare();
					MakeMove(validMove, false); // Do not animate a move made by a release
				}
				mouseClickInfo[0] = -1; mouseClickInfo[1] = -1;
			}

			if (IsRightPressed) {
				mouseClickInfo[2] = squareClicked;
				view.ui.DeselectActiveSquare();
				view.ui.isDraggingPiece = false;
			}

			if (IsRightReleased) {
				mouseClickInfo[3] = squareClicked;
				if (view.ui.selectedIndex == -1 && mouseClickInfo[0] == -1) {
					view.ui.highlightedSquares[squareClicked] = ! view.ui.highlightedSquares[squareClicked];
				} else
				if (true) {
					view.drawnArrows.Add((mouseClickInfo[2], mouseClickInfo[3]));
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
					view.ui.DeselectActiveSquare();
					Piece[] old = model.board.board.ToArray();
					model.SetPrevState();
					model.SetPrevState();
					view.ui.activeAnimation = new BoardAnimation(old, model.board.board, 0.08f);
					break;
				}
				case (int) KeyboardKey.KEY_X :{
					view.ui.DeselectActiveSquare();
					Piece[] old = model.board.board.ToArray();
					model.SetNextState();
					model.SetNextState();
					view.ui.activeAnimation = new BoardAnimation(old, model.board.board, 0.08f);
					break;
				}
				case (int) KeyboardKey.KEY_C :{
					view.ui.DeselectActiveSquare();
					SuspendPlay = ! SuspendPlay;
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

		public void MakeMove(Move move, bool animate=true) {
			if (! move.IsNull) { // When null move is attempted, it's assumed it's checkmate, active color is the loser
				view.TimeOfLastMove = view.fTimeElapsed;
				model.ActivePlayer.IsSearching = false;
				Piece[] oldState = model.board.board.ToArray();
				model.board.MakeMove(move);
				if (animate) {
					view.ui.activeAnimation = new BoardAnimation(oldState, model.board.board, .12f);
				}
				return;
			}

			SuspendPlay = true;

			ConsoleHelper.WriteLine("Checkmate!", ConsoleColor.DarkBlue);
			ConsoleHelper.WriteLine($"Winner: {(model.ActivePlayer.color == 'w' ? 'b' : 'w')}, Loser: {model.ActivePlayer.color}", ConsoleColor.DarkBlue);
			// Handle Checkmate
		}

	}
}
