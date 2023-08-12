using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class View {
		public BoardUI ui;
		public static Vector2 screenSize;
		public Model model;
		public Camera2D camera;

		public float fTimeElapsed = 0.0f;
		public float TimeOfLastMove = 0.0f;

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
			fTimeElapsed += dt;

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

			DrawPlayerInfo();

			Raylib.EndMode2D();


			foreach (IInteractable asset in pipeline) {
				asset.Update();
				asset.Draw();
			}
		}




		public void DrawPlayerInfo() { // Draw this in Camera space
			// Draw player timer


			// Draw icon for player state (isSearching, hasStarted)
			// TODO: Improve this logic
			for (int i=0; i<2; i++) {
				ChessPlayer player = i==0 ? model.whitePlayer : model.blackPlayer;
				Vector2 displayPosition = new Vector2(500f, 350f * (ui.isFlipped ? -1 : 1) * (player == model.whitePlayer ? 1 : -1));
				Color playerInfoColor = ColorHelper.HexToColor("#2c2c2c"); // inactive color
				if (model.ActivePlayer == player) {
					playerInfoColor = ColorHelper.HexToColor("#79ff2b");
				}
				if (player.IsThreaded) { // Player is a computer
					Debug.Assert(player.Computer != null);
					if ((!player.Computer.HasStarted)) {
						playerInfoColor = ColorHelper.HexToColor("#ff4a4a"); // inactive color
					}
				}
				if (player.IsSearching) {
					int period = 1; // in seconds
					float bias = 0.2f; // percentage of period where output => 0
					// Neat little math function to set a bias of how long a value will be 0, bias of 0.2 means it will be 0 for 20% of the perios
					int output = (int) Math.Floor(1+(1/period)*((fTimeElapsed-TimeOfLastMove)%period) - bias); // No idea why Math.floor returns a double
					// With a bias of 0.2, the displayed color will be the first color, 20% of the time
					string[] sequencedColors = { "#2c2c2c", "#ffe553"};
					playerInfoColor = ColorHelper.HexToColor(sequencedColors[output]);
				}
				ui.DrawRectangle(displayPosition.X, displayPosition.Y, 0.8f*BoardUI.squareSize, 0.8f*BoardUI.squareSize, playerInfoColor);
			}
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