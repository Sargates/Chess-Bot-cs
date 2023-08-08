using Raylib_cs;
using System;
using System.IO;
using System.Numerics;
using System.Globalization;
using ChessBot.Engine;
using ChessBot.Helpers;
using ChessBot.Engine.Stockfish;


namespace ChessBot.Application {
	using static MoveGenerator;


	public class Controller {
		Vector2 screenSize;
		static Camera2D cam;
		Model model;
		View view;

		public Controller() {

			
			Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE);
			Raylib.SetTraceLogLevel(TraceLogLevel.LOG_FATAL); // Ignore Raylib Errors unless fatal
			Raylib.InitWindow(1600, 900, "Chess");

			cam = new Camera2D();
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            cam.target = new Vector2(0, 0);
            cam.offset = new Vector2(screenWidth / 2f, screenHeight / 2f);
            cam.zoom = 1.0f;


			screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
			model = new Model();
			view = new View(screenSize, model, cam);

			
		}

		public void MainLoop() {
			float dt = 0f;

			Stockfish stockfish = new Stockfish("./resources/stockfish-windows-x86-64-avx2.exe");


			while (!Raylib.WindowShouldClose()) {
				dt = Raylib.GetFrameTime();

				if (Raylib.IsWindowResized()) {
            		view.camera.offset = new Vector2(Raylib.GetScreenWidth() / 2f, Raylib.GetScreenHeight() / 2f);
					View.screenSize = new Vector2(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
				}
				
				Raylib.BeginDrawing();
				Raylib.ClearBackground(new Color(22, 22, 22, 255));
				Raylib.DrawFPS(10, 10);
				view.Update(dt);


				//* Draw menu here

				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();

			view.Release();
            UIHelper.Release();

		}

		public void HandleInput() {
			
		}
	}
}
