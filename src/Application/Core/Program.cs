using ChessBot;
using Raylib_cs;
using System.Numerics;

namespace ChessBot.Application {

	static class Program {
		static Camera2D cam;

		public static void HandleShutdown( object sender, ConsoleCancelEventArgs e) {
			e.Cancel = true;
			Console.WriteLine("Shutting down");
			Raylib.CloseWindow();
			Environment.Exit(0);
		}

		public static void Main()
		{

			// Console.CancelKeyPress += new ConsoleCancelEventHandler(HandleShutdown);

			Raylib.SetTraceLogLevel(TraceLogLevel.LOG_WARNING);
			Raylib.InitWindow(1600, 900, "Chess");

			cam = new Camera2D();
            int screenWidth = Raylib.GetScreenWidth();
            int screenHeight = Raylib.GetScreenHeight();
            cam.target = new Vector2(0, 0);
            cam.offset = new Vector2(screenWidth / 2f, screenHeight / 2f);
            cam.zoom = 1.0f;

			Controller controller = new Controller();

			// Console.Write("{ ");
			// foreach ((int x, int y) in new (int x, int y)[] { (-2, 1), (-1, 2), (1, 2), (2, 1), (2, -1), (1, -2), (-1, -2), (-2, -1) }) {
			// 	Console.Write(8*y+x+", ");
			// }
			// Console.WriteLine(" }");


			while (!Raylib.WindowShouldClose()) {
			// while (run) {
				
				Raylib.BeginDrawing();
				Raylib.ClearBackground(new Color(22, 22, 22, 255));
				Raylib.BeginMode2D(cam);

				controller.Draw();
				
				
				// string? inp = Console.ReadLine();
				// if (inp != null && inp.Equals("exit")) {
				// 	run = false;
				// }
				controller.Update();
				
				Raylib.EndMode2D();

				// * Draw menu here

				Raylib.EndDrawing();
			}

			Raylib.CloseWindow();
		}
	}
}