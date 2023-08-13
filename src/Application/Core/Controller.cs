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
		


		
		// Return active player based on color passed, throw error if invalid color
		public ChessPlayer GetPlayerFromColor(char color) => ("wb".IndexOf(color) == -1) ? throw new Exception("Invalid Color") : (color == 'w') ? model.whitePlayer : model.blackPlayer;
		


		public Controller() {

			Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
			Raylib.SetTraceLogLevel(TraceLogLevel.LOG_FATAL); // Ignore Raylib Errors unless fatal
			Raylib.InitWindow(1600, 900, "Chess");
			Raylib.InitAudioDevice();

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

				if (Raylib.IsWindowResized()) {
            		view.camera.offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
					View.screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
				}

				Raylib.BeginDrawing();
				Raylib.ClearBackground(new Color(22, 22, 22, 255));
				Raylib.DrawFPS(10, 10);
				
				model.Update();
				view.Update(dt);
				view.Draw();

				if ((model.ActivePlayer.Computer?.HasStarted ?? true) && ! model.ActivePlayer.IsSearching && ! Model.SuspendPlay) {
					model.ActivePlayer.IsSearching = true;
				}

				//* Draw menu here

				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();
			model.ExitPlayerThreads();
			model.JoinPlayerThreads();
			

			Raylib.CloseAudioDevice();
			view.Release();
            UIHelper.Release();
		}
		
		public void MakeMove(Move move, bool animate=true) {
			if (! move.IsNull) { // When null move is attempted, it's assumed it's checkmate, active color is the loser
				model.ActivePlayer.IsSearching = false;
				bool wasPieceCaptured = model.board.GetSquare(move.TargetSquare) != Piece.None || move.MoveFlag == Move.EnPassantCaptureFlag;
				model.board.MakeMove(move);
				view.TimeOfLastMove = view.fTimeElapsed;
				if (animate) { view.ui.activeAnimation = new BoardAnimation(model.board.prevBoard, model.board.board, .12f); }
				if (wasPieceCaptured) {
					Raylib.PlaySound(view.captureSound);
				} else if (move.MoveFlag == Move.CastleFlag) {
					Raylib.PlaySound(view.castleSound);
				} else {
					Raylib.PlaySound(view.moveSound);
				}
				return;
			}

			Model.SuspendPlay = true;

			ConsoleHelper.WriteLine("Checkmate!", ConsoleColor.DarkBlue);
			ConsoleHelper.WriteLine($"Winner: {(model.ActivePlayer.color == 'w' ? 'b' : 'w')}, Loser: {model.ActivePlayer.color}", ConsoleColor.DarkBlue);
			// Handle Checkmate
		}
	}
}
