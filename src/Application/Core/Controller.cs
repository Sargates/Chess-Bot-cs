using Raylib_cs;
using System;
using System.IO;
using System.Numerics;
using System.Globalization;
using ChessBot.Engine;
using ChessBot.Helpers;


namespace ChessBot.Application {
	using static MoveGenerator;


	public class Controller {
		Vector2 screenSize;
		static Camera2D cam;
		Model model;
		View view;
		int squareClicked;
		double timeClicked=0;


		public static void Main() {

			Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
			Raylib.InitWindow(1600, 900, "Chess");

			cam = new Camera2D();
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            cam.target = new Vector2(0, 0);
            cam.offset = new Vector2(screenWidth / 2f, screenHeight / 2f);
            cam.zoom = 1.0f;

			Controller controller = new Controller();

			controller.MainLoop();
		}


		public Controller() {
			screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
			model = new Model();
			view = new View(screenSize);
			squareClicked = -1;
		}

		public void MainLoop() {
			float dt = 0f;

			while (!Raylib.WindowShouldClose()) {
				dt = Raylib.GetFrameTime();
				
				Raylib.BeginDrawing();
				Raylib.ClearBackground(new Color(22, 22, 22, 255));
				Raylib.DrawFPS(10, 10);
				Raylib.BeginMode2D(cam);

				
				view.Update(model);
				HandleInput();

				Raylib.EndMode2D();

				//	Draw menu here

				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();
		}

		public void HandleInput() {
			// TODO Test how ineffective it would be to constantly update mousePos and check if mouse is on a square
			squareClicked = -1;
			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_LEFT)) {
				Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

				Vector2 boardPos = (pos/view.board.squareSize)+new Vector2(4);
				
				if (! (0 <= boardPos.X && boardPos.X < 8 && 0 <= boardPos.Y && boardPos.Y < 8) ) {
					view.board.selectedIndex = -1;
					view.board.movesForSelected = new Move[0];
					return;
				} // Passes guard clause if interaction (click/release) is on a board square
				squareClicked = 8*((int)(8-boardPos.Y))+(int)boardPos.X;

				int clickedPiece = model.board.GetSquare(squareClicked);
				if (clickedPiece == PieceHelper.None) {
					view.board.selectedIndex = -1;
					view.board.movesForSelected = new Move[0];
					return;
				}

				view.board.selectedIndex = squareClicked;
				view.board.movesForSelected = MoveGenerator.GetMoves(model.board, squareClicked);
			}

			// BUG: WHEN PIECE IS SELECTED BUT MOUSE IS NOT DRAGGING, WHEN SQUARE IS CLICKED, PIECE SNAPS TO CURSOR

			if (Raylib.IsMouseButtonReleased(MouseButton.MOUSE_BUTTON_LEFT)) {
				Vector2 pos = Raylib.GetMousePosition() - screenSize/2;

				Vector2 boardPos = (pos/view.board.squareSize)+new Vector2(4);
				
				if (! (0 <= boardPos.X && boardPos.X < 8 && 0 <= boardPos.Y && boardPos.Y < 8) ) {
					return;
				} // Passes guard clause if interaction (click/release) is on a board square
				squareClicked = 8*((int)(8-boardPos.Y))+(int)boardPos.X;

				Move validMove = new Move(0);
				foreach (Move move in view.board.movesForSelected) {
					if (move.TargetSquare == squareClicked) {
						// Console.WriteLine($"{move} is valid move");
						validMove = move;
						break;
					}
				}
				if (! validMove.IsNull) {
					model.board.MakeMove(validMove);
				} else {
					return;
				}
				view.board.selectedIndex = -1;
				view.board.movesForSelected = new Move[0];
			}
			if (Raylib.IsMouseButtonPressed(MouseButton.MOUSE_BUTTON_RIGHT)) {
				view.board.selectedIndex = -1;
				view.board.movesForSelected = new Move[0];
			}
		}
	}
}
