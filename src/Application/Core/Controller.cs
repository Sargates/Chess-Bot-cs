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
		public static dynamic appSettings = new ApplicationSettings(FileHelper.GetResourcePath("settings.txt"));


		public Controller() {

			screenSize = new Vector2(appSettings.uiScreenWidth, appSettings.uiScreenHeight);
			View.screenSize = screenSize;
			Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
			Raylib.SetTraceLogLevel(TraceLogLevel.LOG_FATAL); // Ignore Raylib Errors unless fatal
			Raylib.InitWindow((int)screenSize.X, (int)screenSize.Y, "Chess");
			Raylib.InitAudioDevice();
			Raylib.SetMasterVolume(appSettings.uiSoundVolume);

			Debug.Assert(random != null);

			cam = new Camera2D();
            int screenWidth = (int)screenSize.X;
            int screenHeight = (int)screenSize.Y;
            cam.target = new Vector2(0, 0);
            cam.offset = new Vector2(screenWidth / 2f, screenHeight / 2f);
            cam.zoom = 1.0f;

			Player.OnMoveChosen += MakeMove;
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
			SaveApplicationSettings();

			view.Release();
            UIHelper.Release();
		}

		public void SaveApplicationSettings() {
			using (StreamWriter writer = new StreamWriter(FileHelper.GetResourcePath("settings.txt"))) {
				writer.WriteLine($"uiScreenWidth={View.screenSize.X}");
				writer.WriteLine($"uiScreenHeight={View.screenSize.Y}");
				foreach (var pair in Controller.appSettings._dictionary) {
					if (pair.Key == "uiScreenWidth" || pair.Key == "uiScreenHeight") continue;
					writer.WriteLine($"{pair.Key}={pair.Value}");
				}
			}
		}

		public void MakeMove(Move move, bool animate=true) {
			model.ActivePlayer.IsSearching = false;
			if (! move.IsNull) { // When null move is attempted, it's assumed it's checkmate, active color is the loser
				Piece pieceMoved = model.board.GetSquare(move.StartSquare);
				bool wasPieceCaptured = model.board.GetSquare(move.TargetSquare) != Piece.None || move.MoveFlag == Move.EnPassantCaptureFlag;
				model.board.MakeMove(move);
				view.TimeOfLastMove = view.fTimeElapsed;
				if (animate) { view.ui.activeAnimations.AddRange(AnimationHelper.FromMove(move, pieceMoved, 0.08f)); }
				if (MoveGenerator.IsSquareAttacked(model.board, model.board.activeColor == Piece.White ? model.board.whiteKingPos : model.board.blackKingPos, model.board.activeColor))
													   	{	Raylib.PlaySound(view.sounds[(int)View.Sounds.Check]); } else
				if (wasPieceCaptured) 				   	{	Raylib.PlaySound(view.sounds[(int)View.Sounds.Capture]); } else
				if (move.MoveFlag == Move.CastleFlag)  	{	Raylib.PlaySound(view.sounds[(int)View.Sounds.Castle]); } else
														{	Raylib.PlaySound(view.sounds[(int)View.Sounds.Move]); }
				return;
			}

			Model.SuspendPlay = true;

			ConsoleHelper.WriteLine("Checkmate!", ConsoleColor.DarkBlue);
			ConsoleHelper.WriteLine($"Winner: {model.InactivePlayer.color}, Loser: {model.ActivePlayer.color}", ConsoleColor.DarkBlue);
			// Handle Checkmate
		}
	}
}
