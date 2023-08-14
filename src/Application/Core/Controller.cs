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
			model.NewGameCalls += () => {
				view.ui.isFlipped = model.humanColor == 0b01; // if white is not a player and black is a player
			};
			model.PushNewAnimations += (args) => {
				view.ui.activeAnimations.AddRange(args);
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

				if ((model.ActivePlayer.Computer?.HasStarted ?? true) && ! model.ActivePlayer.IsSearching && ! model.SuspendPlay) {
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
			if (move.IsNull) {
				throw new Exception("Null move was played");
			}

			model.ActivePlayer.IsSearching = false;


			Piece pieceMoved = model.board.GetSquare(move.StartSquare);
			model.board.MakeMove(move);
			view.TimeOfLastMove = view.fTimeElapsed;

			bool opponentInCheck = MoveGenerator.IsSquareAttacked(model.board, model.board.activeColor == Piece.White ? model.board.whiteKingPos : model.board.blackKingPos, model.board.activeColor);
			bool canOpponentRespond = MoveGenerator.GetAllMoves(model.board, model.board.activeColor).Length != 0; // Negated for readability

			if (model.board.currentStateNode.Previous == null) { throw new Exception("Something went wrong"); }
			Fen temp = model.board.currentStateNode.Previous.Value;

			if (opponentInCheck && canOpponentRespond) {
				temp.moveMade.moveSoundEnum = (int)SoundStates.Check;
			} else
			if (opponentInCheck && ! canOpponentRespond) {
				temp.moveMade.moveSoundEnum = 0; // Sound is played separately if game is over
			} else
			if (! opponentInCheck && ! canOpponentRespond) {
				temp.moveMade.moveSoundEnum = 0; // Sound is played separately if game is over
			}

			Debug.Assert(model.board.currentStateNode.Previous != null);
			model.board.currentStateNode.Previous.Value = temp;

			if (animate) {
				view.ui.activeAnimations.AddRange(AnimationHelper.FromMove(temp.moveMade, pieceMoved, 0.08f));
			} else {
				Raylib.PlaySound(BoardUI.sounds[model.board.currentStateNode.Previous.Value.moveMade.moveSoundEnum]);
			}

			// If opponent can't respond, fallthrough to game end handling
			if (canOpponentRespond) return;

			model.SuspendPlay = true;

			if (opponentInCheck) { // Checkmate
				Raylib.PlaySound(BoardUI.sounds[(int)SoundStates.Checkmate]);
				ConsoleHelper.WriteLine("Checkmate!", ConsoleColor.DarkBlue);
				ConsoleHelper.WriteLine($"Winner: {model.InactivePlayer.color}, Loser: {model.ActivePlayer.color}", ConsoleColor.DarkBlue);
			} else { // Stalemate
				Raylib.PlaySound(BoardUI.sounds[(int)SoundStates.Stalemate]);
				ConsoleHelper.WriteLine("Stalemate", ConsoleColor.DarkBlue);
				ConsoleHelper.WriteLine($"Draw.", ConsoleColor.DarkBlue);
			}

			// Handle Checkmate
		}
	}
}
