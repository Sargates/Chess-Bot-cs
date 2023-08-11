using Raylib_cs;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class View {
		public BoardUI ui;
		public static Vector2 screenSize;
		public Model model;
		public Camera2D camera;

		public List<(int tail, int head)> drawnArrows = new List<(int tail, int head)>();

		public List<IInteractable> pipeline;


		public View(Vector2 screenSize, Model model, Camera2D cam) {
			ui = new BoardUI();
			View.screenSize = screenSize;
			this.model = model;
			this.model.enforceColorToMove = true;
			this.camera = cam;

			pipeline = new List<IInteractable>();

			AddButtons();
		}

		public void AddToPipeline(IInteractable interactable) {
			pipeline.Add(interactable);
		}

		public void Update(float dt) {


			Raylib.BeginMode2D(camera);

			ui.DrawBoardBorder();
			ui.DrawBoardSquares();
			ui.DrawPiecesOnBoard(model.board);
			ui.ResetBoardColors();

			if (ui.activeAnimation != null) {
				ui.activeAnimation.Update(dt);
				ui.activeAnimation.Draw(ui.isFlipped);
				if (ui.activeAnimation.HasFinished) {
					ui.activeAnimation = null;
				}
			}
			Raylib.EndMode2D();

			DrawPlayerInfo();

			foreach (IInteractable asset in pipeline) {
				asset.Update();
				asset.Draw();
			}
		}

		public void DrawPlayerInfo() {
			// Draw player timer

			// Draw icon for player state (thinking, )
		}

		public void GetAttackedSquares() {
			string o = "";

			for (int i=0;i<8;i++) {
				for (int j=0;j<8;j++) {
					int index = 8*(7-i)+j;

					Piece piece = model.board.GetSquare(index);

					string symbol = MoveGenerator.IsSquareAttacked(model.board, index, Piece.White) ? "**" : "--";
					
					o += $"{symbol} ";
				}
				Console.WriteLine(o);
				o = "";
			}
		}

		public void AddButtons() {

			// AddToPipeline(new Button(new Rectangle(40, 300, 210, 50), "Set Fish Elo 3000").SetCallback(() => {
			// 	model.whitePlayer.UCI?.engine.SetElo(3000);
			// 	model.blackPlayer.UCI?.engine.SetElo(3000);
			// })); //* I don't really have a way to tell if `SetElo` even works until I make my own bot and test it
			// AddToPipeline(new Button(new Rectangle(40, 360, 210, 50), "Set Fish Elo 600").SetCallback(() => {
			// 	model.whitePlayer.UCI?.engine.SetElo(600);
			// 	model.blackPlayer.UCI?.engine.SetElo(600);
			// }));
			AddToPipeline(new Button(new Rectangle(40, 420, 210, 50), "Freeplay").SetCallback(() => {
				model.StartNewGame();
				// ui.activeAnimation = new BoardAnimation(model.oldBoard, model.board.board, 0.2f);
			}));
			AddToPipeline(new Button(new Rectangle(40, 480, 210, 50), "Human vs. Gatesfish").SetCallback(() => {
				model.StartNewGame(Model.Gametype.HvC);
				// ui.activeAnimation = new BoardAnimation(model.oldBoard, model.board.board, 0.2f);
			}));
			AddToPipeline(new Button(new Rectangle(40, 540, 210, 50), "Human vs. Stockfish").SetCallback(() => {
				model.StartNewGame(Model.Gametype.HvU);
				// ui.activeAnimation = new BoardAnimation(model.oldBoard, model.board.board, 0.2f);
			}));
			AddToPipeline(new Button(new Rectangle(40, 600, 210, 50), "Stockfish vs. Stockfish").SetCallback(() => {
				model.StartNewGame(Model.Gametype.UvU);
				// ui.activeAnimation = new BoardAnimation(model.oldBoard, model.board.board, 0.2f);
			}));

			// AddToPipeline(new Button(new Rectangle(40, 420, 210, 50), "Flip Board").SetCallback(() => {
			// 	ui.isFlipped = ! ui.isFlipped;
			// }));

			// // 2x3 array of buttons for player types
			// AddToPipeline(new Button(new Rectangle(40,  660, 60, 50), "HvH").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.HvH);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(115, 660, 60, 50), "HvC").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.HvC);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(190, 660, 60, 50), "HvU").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.HvU);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(40,  720, 60, 50), "CvC").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.CvC);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(115, 720, 60, 50), "CvU").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.CvU);
			// 	UpdateIsFlipped();
			// }));
			// AddToPipeline(new Button(new Rectangle(190, 720, 60, 50), "UvU").SetCallback(() => {
			// 	model.SetPlayerTypes(Model.Gametype.UvU);
			// 	UpdateIsFlipped();
			// }));
		}
		public void Release() {
			ui.Release();
		}
	}
}