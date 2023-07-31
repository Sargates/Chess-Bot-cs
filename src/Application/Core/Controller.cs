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
			view = new View(screenSize, model);
		}

		public void MainLoop() {
			float dt = 0f;

			while (!Raylib.WindowShouldClose()) {
				dt = Raylib.GetFrameTime();
				
				Raylib.BeginDrawing();
				Raylib.ClearBackground(new Color(22, 22, 22, 255));
				Raylib.DrawFPS(10, 10);
				Raylib.BeginMode2D(cam);

				
				view.Update();

				Raylib.EndMode2D();

				//* Draw menu here

				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();
		}

		public void HandleInput() {
			
		}
	}
}
