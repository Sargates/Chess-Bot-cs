using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application;
public class View : IView {
	public BoardUI ui;
	public static Vector2 screenSize;
	public Camera2D camera;
	public List<ScreenObject> pipeline = new List<ScreenObject>();
	public float fTimeElapsed = 0.0f;
	public float TimeOfLastMove = 0.0f;

	public List<(int tail, int head)> drawnArrows = new List<(int tail, int head)>();



		
	public View(Vector2 screenSize, Camera2D cam) {
		ui = new BoardUI();
		this.camera = cam;
	}

	public void AddToPipeline(ScreenObject interactable) {
		pipeline.Add(interactable);
	}

	public void Update(float dt) {

		foreach (PieceAnimation anim in ui.activeAnimations) {
			anim.Update(dt);
		}


		foreach (ScreenObject asset in pipeline) {
			asset.Update();
		}

	}

	public void Draw() {

		Raylib.BeginMode2D(camera);

		ui.DrawBoardBorder();
		ui.DrawBoardSquares();
		ui.DrawPiecesOnBoard(); // Includes Animations

		DrawPlayerInfo();

		Raylib.EndMode2D();


		foreach (ScreenObject asset in pipeline) {
			asset.Draw();
		}
	}

	public void DrawPlayerInfo() { // Draw this in Camera space
		// Draw player timer


		// Draw icon for player state (isSearching, hasStarted)
		// TODO: Improve this logic
		for (int i=0; i<2; i++) {
			ChessPlayer player = i==0 ? MainController.Instance.model.whitePlayer : MainController.Instance.model.blackPlayer;
			Vector2 displayPosition = new Vector2(500f, 350f * (ui.isFlipped ? -1 : 1) * (player == MainController.Instance.model.whitePlayer ? 1 : -1));
			Color playerInfoColor = ColorHelper.HexToColor("#2c2c2c"); // inactive color
			if (MainController.Instance.model.ActivePlayer == player) {
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
				string[] sequencedColors = { "#2c2c2c", "#ffff00"};
				playerInfoColor = ColorHelper.HexToColor(sequencedColors[output]);
			}
			ui.DrawRectangleCentered(displayPosition, new Vector2(0.8f*BoardUI.squareSize), playerInfoColor);
		}
	}

	public void AddNewAnimations(List<PieceAnimation> l) {
		ui.activeAnimations.AddRange(l);
	}

	// public void AddButtons() {
		
	// }
	// public Sound SoundFromState(int state) {
	// 	if (MoveGenerator.IsSquareAttacked(model.board, model.board.activeColor == Piece.White ? model.board.whiteKingPos : model.board.blackKingPos, model.board.activeColor)) {
	// 		return sounds[(int)StateSounds.Check];
	// 	} else
	// 	if (wasPieceCaptured) {
	// 		return sounds[(int)StateSounds.Capture];
	// 	} else
	// 	if (move.MoveFlag == Move.CastleFlag){
	// 		return sounds[(int)StateSounds.Castle];
	// 	} else {
	// 		return sounds[(int)StateSounds.Move];
	// 	}
	// }
	
	public void Release() {
		ui.Release();
	}
}