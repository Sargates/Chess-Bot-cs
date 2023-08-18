using Raylib_cs;
using System.IO;
using System.Numerics;
using System.Diagnostics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application;
using static MoveGenerator;


public class MainController { // I would use `AppController` but OmniSharp's autocomplete keeps giving me `AppContext`

	private static MainController? instance; // singleton
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

	// in the format of: rightReleased, rightPressed, leftPressed, leftReleased
	public bool WildlySpecificBooleanToTellIfShouldDeselectSquare;
	public int mouseButtonsClicked=0; // 0b1111
	public int PressedKey=0;
	
	public bool IsRightReleased => (mouseButtonsClicked & 8) == 8;
	public bool IsRightPressed => (mouseButtonsClicked & 4) == 4;
	public bool IsLeftReleased => (mouseButtonsClicked & 2) == 2;
	public bool IsLeftPressed => (mouseButtonsClicked & 1) == 1;

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
			
			if (PressedKey != 0) {
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

	public int GetSquareMouseIsOver() {

		int squareClicked = -1;
		Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

		Vector2 boardPos = (pos/BoardUI.squareSize);
		if (view.ui.IsFlipped) { boardPos *= -1; }
		boardPos += new Vector2(4);

		if ((0 <= boardPos.X && boardPos.X < 8 && 0 <= boardPos.Y && boardPos.Y < 8) ) {
			squareClicked = 8*((int)(8-boardPos.Y))+(int)boardPos.X;
		} // If the interaction (click/release) is in bounds, set square clicked and clicked piece, otherwise they will be -1 and {Piece.None}

		return squareClicked;
	}

	
	public void HandleMouseInput() {
		// TODO Test how ineffective it would be to constantly update mousePos and check if mouse is on a square

		//* Precondition: Mouse button has changed state

		Piece clickedPiece = Piece.None;
		int squareClicked = -1;
		Move validMove = new Move(0);

		squareClicked = GetSquareMouseIsOver();
		if (squareClicked == -1) { //! Case 1
			view.ui.DeselectActiveSquare();
			return;
		} //* Passes guard clause if the click was in bounds
		clickedPiece = model.board.GetSquare(squareClicked);

		//* By this point a we know that the mouse was over a square when it changed state

		if (view.ui.SelectedIndex != -1) { //* If it's a second click (a square was already selected thus a move was tried), check if the move is valid
			foreach (Move move in view.ui.MovesForSelected) { //* Iterate through valid moves to 
				if (move == new Move(view.ui.SelectedIndex, squareClicked)) { validMove = move; break; }
			}
		}

		if (IsLeftPressed) mouseClickInfo[0] = squareClicked;
		if (IsLeftReleased) mouseClickInfo[1] = squareClicked;
		if (IsRightPressed) mouseClickInfo[2] = squareClicked;
		if (IsRightReleased) mouseClickInfo[3] = squareClicked;


		if (mouseClickInfo[0] != -1 && mouseClickInfo[1] == -1 && mouseClickInfo[2] == -1 ) { //* Rising edge left click
			view.ui.highlightedSquares = new bool[64];
			view.ui.ArrowsOnBoard.Clear();
			view.ui.IsDraggingPiece = true;
			if (view.ui.SelectedIndex == mouseClickInfo[0]) { //! Case 5
				WildlySpecificBooleanToTellIfShouldDeselectSquare = true;
				return;
			}
			WildlySpecificBooleanToTellIfShouldDeselectSquare = false;

			if (! validMove.IsNull) { //! Case 3
				model.MakeMoveOnBoard(validMove);
				return;
			} //* Passes guard clause if the move tried is invalid, either because it's illegal or because a square wasn't selected
			//* Need to check if move was valid before exitting because a piece wasnt clicked
			if (clickedPiece == Piece.None) { //! Case 8
				view.ui.DeselectActiveSquare();
				return;
			} //* Passes guard clause if a piece was clicked




			//! Case 2
			//* Case 2 is satisfied by this point, do additional checks to get desired behavior
			view.ui.SelectedIndex = squareClicked;
			view.ui.MovesForSelected = new Move[0]; // Reset moves because we're going to potentially recalculate them anyways
			if (! ((model.humanColor >> 1) == 1 && clickedPiece.Color == Piece.White || (model.humanColor & 1) == 1 && clickedPiece.Color == Piece.Black)) { //! Case 2a
				return;
			} //* Passes guard clause if square clicked is a human color
			if (! (!model.enforceColorToMove || model.enforceColorToMove && (model.board.whiteToMove ? Piece.White : Piece.Black) == clickedPiece.Color)) { //! Case 2b
				return;
			} //* Passes guard clause if it's that color's turn to move

			//! Case 2c

			//* Desired behavior is that when a piece is selected, it will only generate moves for that piece if it satisfies the two above conditions
			view.ui.MovesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
		}

		if (mouseClickInfo[1] != -1) { //* Release of left click
			view.ui.IsDraggingPiece = false;
			if (! validMove.IsNull) {
				model.MakeMoveOnBoard(validMove, false); //* Don't animate because it's a release
			}
			if (mouseClickInfo[0] == mouseClickInfo[1] && WildlySpecificBooleanToTellIfShouldDeselectSquare) {
				view.ui.DeselectActiveSquare();
				WildlySpecificBooleanToTellIfShouldDeselectSquare = false;
			}
		}

		if (mouseClickInfo[2] != -1 && mouseClickInfo[3] == -1) {
			if (mouseClickInfo[0] != -1 ) { //* Mouse is dragging a piece, deselect active piece
				view.ui.IsDraggingPiece = false;
				view.ui.DeselectActiveSquare();
				return;
			}
		}
		if (mouseClickInfo[3] != -1) { //* Release of right click
			if (mouseClickInfo[0] == -1) {
				if (mouseClickInfo[2] == mouseClickInfo[3]) { //* Should highlight square
					view.ui.highlightedSquares[mouseClickInfo[2]] = ! view.ui.highlightedSquares[mouseClickInfo[2]];
				} else { //* Draw arrow
					var tup = (mouseClickInfo[2], mouseClickInfo[3]);
					if (view.ui.ArrowsOnBoard.Contains(tup)) {
						// Console.WriteLine($"Removed Arrow From Draw: <{mouseClickInfo[2]}, {mouseClickInfo[3]}>");
						view.ui.ArrowsOnBoard.Remove(tup);
					} else {
						// Console.WriteLine($"Add Arrow To Draw: <{mouseClickInfo[2]}, {mouseClickInfo[3]}>");
						view.ui.ArrowsOnBoard.Add(tup);
					}
				}
			}
		}

		if (mouseClickInfo[1] != -1) { mouseClickInfo[0] = -1; mouseClickInfo[1] = -1; }; //* If a button was released, reset the mouse info for that button
		if (mouseClickInfo[3] != -1) { mouseClickInfo[2] = -1; mouseClickInfo[3] = -1; }; //* If a button was released, reset the mouse info for that button
		return;

		//* Cases for left button press (see writeup)
		//* Default:																		=> Set IsDragging to true;
		//* Case  1: 	No square is selected, and square clicked is out of bounds			=> call DeselectActiveSquare
		//* Case  5: 	Square is already selected and square clicked == selected index		=> Set special boolean value (see note)
		//* Case  3: 	Square is already selected and square clicked is a valid move		=> call model.board.MakeMove -- Fallthrough to 8, 2
		//* Case  8: 	Square is already selected and a piece is not clicked				=> call DeselectActiveSquare -- DO NOT fallthrough to case 2
		//* Case  2: 	No square is selected and piece is clicked							=> Set selectedIndex to square clicked -- Fallthrough to case 2a
		//* Case  2a:	ClickedPiece.Color is not a human color, (it's a computer's piece)	=> Do not fallthrough to case 2b
		//* Case  2b:	ClickedPiece.Color is not the color to move							=> Do not fallthrough to case 2c
		//* Case  2c:	2a and 2b were both false											=> Generate the moves for the selected index and save to the View
	}

	public void HandleKeyboardInput() {
		switch (PressedKey) {
			case (int) KeyboardKey.KEY_Z :{
				if ((model.ActivePlayer.Computer?.IsSearching ?? false)) { break; }
				if (model.board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get second previous state, is null"); return; }
				//* If there is exactly one human, undo two moves
				if ((((model.humanColor >> 0) & 1) ^ ((model.humanColor >> 1) & 1)) == 1) {

					//* If a human is playing black and a computer is playing white, you can force the AI to rethink it's first move
					//* This fixes that
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
				Console.WriteLine($"White Eval: {Evaluation.SumFromBoard(model.board, Piece.White)}");
				Console.WriteLine($"Black Eval: {Evaluation.SumFromBoard(model.board, Piece.Black)}");
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
		mouseButtonsClicked += Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_RIGHT) ? 8 : 0;
		mouseButtonsClicked += Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)  ? 4 : 0;
		mouseButtonsClicked += Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)  ? 2 : 0;
		mouseButtonsClicked += Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)   ? 1 : 0;
		PressedKey = Raylib.GetKeyPressed();
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