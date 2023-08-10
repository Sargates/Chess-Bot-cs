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

			foreach (IInteractable asset in pipeline) {
				asset.Update();
				asset.Draw();
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

			Button button1 = new Button(new Rectangle(40, 600, 200, 50), "Reset Board");
			button1.OnLeftPressed = () => {
				Piece[] oldBoard = model.board.board.ToArray();
				model.StartNewGame(Fen.startpos);
				ui.activeAnimation = new BoardAnimation(oldBoard, model.board.board, 0.2f);
			};
			AddToPipeline(button1);
			Button button2 = new Button(new Rectangle(40, 540, 200, 50), "Random Position");
			button2.OnLeftPressed = () => {
				Piece[] oldBoard = model.board.board.ToArray();
				model.StartNewGame(model.botMatchStartFens[Controller.random.Next(model.botMatchStartFens.Length)]);
				ui.activeAnimation = new BoardAnimation(oldBoard, model.board.board, 0.2f);
			};
			AddToPipeline(button2);

			Button button3 = new Button(new Rectangle(40, 480, 200, 50), "Flip Board");
			button3.OnLeftPressed = () => {
				ui.isFlipped = ! ui.isFlipped;
			};
			AddToPipeline(button3);

		}
		public void Release() {
			ui.Release();
		}
	}
}