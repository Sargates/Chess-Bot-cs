using Raylib_cs;
using ChessBot.Engine;
using System.Numerics;

namespace ChessBot.Application {
	public class Animation {
		public int StartIndex, EndIndex;
		public Vector2 StartPos, EndPos;
		public float TotalTime, ElapsedTime;
		public Piece piece;
		public bool HasFinished=false;
		public Animation(int start, int end, Piece piece, float t) {
			// Interpolating animation between positions
			StartIndex = start;
			EndIndex = end;

			StartPos = BoardUI.squareSize*(new Vector2(start & 0b111, (7-(start>>3))) - new Vector2(3.5f));
			EndPos = BoardUI.squareSize*(new Vector2(end & 0b111, (7-(end>>3))) - new Vector2(3.5f));

			TotalTime = t;
			ElapsedTime = 0f;
			this.piece = piece;
		}

		public float LerpTime => ElapsedTime/TotalTime;

		public void Draw(bool isFlipped) {
			// Draw piece at position
			float t = LerpTime;
			Vector2 end = EndPos;
			Vector2 start = StartPos;
			if (isFlipped) { start *= -1; end *= -1; }

			
			BoardUI.DrawPiece(piece, (t*end)+((1-t)*start));
		}
		public void Update(float dt) {
			// Update T [0, 1]
			ElapsedTime += dt;
			if (LerpTime >= 1f) {
				HasFinished = true;
			}
		}
	}
}