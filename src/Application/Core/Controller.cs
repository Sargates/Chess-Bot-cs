using Raylib_cs;
using System.IO;
using System.Numerics;
using System.Diagnostics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application;
using static MoveGenerator;


public class MainController { // I would use `AppController` but OmniSharp's autocomplete keeps giving me `AppContext`

	public static MainController? instance; // singleton
	public static MainController Instance {
		get {
			if (instance == null) {
				instance = new MainController();
			}
			Debug.Assert(instance != null);
			return instance;
		}
	}

	Vector2 screenSize;
	Camera2D cam;
	public Model model;
	public View view;
	public Random random = new Random();
	public dynamic appSettings = new ApplicationSettings(FileHelper.GetResourcePath("settings.txt"));


	public float fTimeElapsed = 0.0f;


	// Square selected on each interaction, -1 for invalid square
	// { leftDown, leftUp, rightDown, rightUp }
	public int[] mouseClickInfo = {-1, -1, -1, -1};
	// in the format of: leftReleased, leftPressed, rightPressed, rightReleased
	public int mouseButtonsClicked; // 0b1111
	public int pressedKey=0;
	
	public bool IsLeftPressed => (mouseButtonsClicked & 8) == 8;
	public bool IsLeftReleased => (mouseButtonsClicked & 4) == 4;
	public bool IsRightPressed => (mouseButtonsClicked & 2) == 2;
	public bool IsRightReleased => (mouseButtonsClicked & 1) == 1;

	private MainController() {
		screenSize = new Vector2(appSettings.uiScreenWidth, appSettings.uiScreenHeight);
		View.screenSize = screenSize;
		Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
		Raylib.SetTraceLogLevel(TraceLogLevel.LOG_FATAL); // Ignore Raylib Errors unless fatal
		Raylib.InitWindow((int)screenSize.X, (int)screenSize.Y, "Chess");
		Raylib.InitAudioDevice();
		Raylib.SetMasterVolume(appSettings.uiSoundVolume);

		cam = new Camera2D();
		int screenWidth = (int)screenSize.X;
		int screenHeight = (int)screenSize.Y;
		cam.target = new Vector2(0, 0);
		cam.offset = new Vector2(screenWidth / 2f, screenHeight / 2f);
		cam.zoom = 1.0f;
		view = new View(screenSize, cam);
		model = new Model(view);

		Player.OnMoveChosen += model.MakeMoveOnBoard;
	}

	public void MainLoop() {
		float dt = 0f;

		while (!Raylib.WindowShouldClose()) {
			dt = Raylib.GetFrameTime();
			fTimeElapsed += dt;
			view.fTimeElapsed = fTimeElapsed;
			UpdateMouseButtons();
			
			if (pressedKey != 0) {
				HandleKeyboardInput();
			}
			if (view.ui.activeAnimations.Count == 0 && mouseButtonsClicked > 0) {
				HandleMouseInput();
			}

			if (Raylib.IsWindowResized()) {
				view.camera.offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
				View.screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
			}

			Raylib.BeginDrawing();
			Raylib.ClearBackground(new Color(22, 22, 22, 255));
			Raylib.DrawFPS(10, 10);

			view.Update(dt);
			view.Draw();

			if ((model.ActivePlayer.Computer?.HasStarted ?? true) && ! model.ActivePlayer.IsSearching && ! model.SuspendPlay) {
				model.ActivePlayer.IsSearching = true;
			}

			//* Draw menu here

			Raylib.EndDrawing();
		}
		view.Release();
		UIHelper.Release();

		Raylib.CloseAudioDevice();
		Raylib.CloseWindow();

		model.ExitPlayerThreads();
		model.JoinPlayerThreads();
		SaveApplicationSettings();
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
				Player.OnMoveChosen(validMove);
				view.TimeOfLastMove = fTimeElapsed;
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
				isHumanColor = isHumanColor || model.SuspendPlay; // if play is suspended, allow view to move pieces
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
				isHumanColor = isHumanColor || model.SuspendPlay; // if play is suspended, allow view to move pieces
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
				Player.OnMoveChosen(validMove, false); // Do not animate a move made by a release
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
				if ((model.ActivePlayer.Computer?.IsSearching ?? false)) { break; }
				// If there is exactly one human, undo two moves
				if ((((model.humanColor >> 0) & 1) ^ ((model.humanColor >> 1) & 1)) == 1) {

					// If a human is playing black and a computer is playing white, you can force the AI to rethink it's first move
					if ((model.board.currentStateNode.Previous?.Previous == null)) { break; }

					model.DoublePrevState();
				} else { // otherwise undo a single move
					model.SinglePrevState();
				}
				view.ui.DeselectActiveSquare();
				break;
			}
			case (int) KeyboardKey.KEY_X :{
				if ((model.ActivePlayer.Computer?.IsSearching ?? false)) { break; }
				// If there is exactly one human, undo two moves
				if ((((model.humanColor >> 0) & 1) ^ ((model.humanColor >> 1) & 1)) == 1) {
					model.DoubleNextState();
				} else { // otherwise undo a single move
					model.SingleNextState();
				}
				view.ui.DeselectActiveSquare();
				break;
			}
			case (int) KeyboardKey.KEY_C :{
				view.ui.DeselectActiveSquare();
				model.SuspendPlay = ! model.SuspendPlay;
				break;
			}

			case (int) KeyboardKey.KEY_P :{
				Raylib.PlaySound(BoardUI.sounds[(int)SoundStates.Check]);
				Raylib.PlaySound(BoardUI.sounds[(int)SoundStates.Checkmate]);
				// Console.WriteLine();
				// Console.WriteLine(model.board.GetUCIGameFormat());
				break;
			}
			case (int) KeyboardKey.KEY_O :{
				int maxOut = 20;
				LinkedListNode<Fen>? currNode = model.board.stateHistory.First;
				if (model.board.stateHistory.Count > maxOut) {
					for (int i=0; i<model.board.stateHistory.Count-maxOut; i++) {
						if (currNode==null) break; // Never happens, compiler gives warning
						currNode = currNode.Next;
					}
				}

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

	public void UpdateMouseButtons() {
		mouseButtonsClicked = 0;
		mouseButtonsClicked += Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)   ? 8 : 0;
		mouseButtonsClicked += Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)  ? 4 : 0;
		mouseButtonsClicked += Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)  ? 2 : 0;
		mouseButtonsClicked += Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT) ? 1 : 0;
		pressedKey = Raylib.GetKeyPressed();
	}

	public void SaveApplicationSettings() {
		using (StreamWriter writer = new StreamWriter(FileHelper.GetResourcePath("settings.txt"))) {
			writer.WriteLine($"uiScreenWidth={View.screenSize.X}");
			writer.WriteLine($"uiScreenHeight={View.screenSize.Y}");
			foreach (var pair in appSettings._dictionary) {
				if (pair.Key == "uiScreenWidth" || pair.Key == "uiScreenHeight") continue;
				writer.WriteLine($"{pair.Key}={pair.Value}");
			}
		}
	}
}