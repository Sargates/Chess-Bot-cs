using System.Numerics;
using ChessBot.Engine;

namespace ChessBot.Application;


public class MoveAnimation : IAnimation {
	
	public int StartIndex, EndIndex;
	public Vector2 StartPos, EndPos;
	public float ElapsedTime { get; set; }
	public float TotalTime { get; set; }
	public Piece piece;
	public bool HasFinished=false;
	public float LerpTime => ElapsedTime/TotalTime;
	public MoveAnimation(Move move, Piece piece, float t) {
		// Interpolating animation between positions
		StartIndex = move.StartSquare;
		EndIndex = move.TargetSquare;

		StartPos = BoardUI.squareSize*(new Vector2(StartIndex & 0b111, (7-(StartIndex>>3))) - new Vector2(3.5f));
		EndPos = BoardUI.squareSize*(new Vector2(EndIndex & 0b111, (7-(EndIndex>>3))) - new Vector2(3.5f));

		TotalTime = t;
		ElapsedTime = 0f;
		this.piece = piece;
	}
	public void Draw(bool isFlipped) {
		// Draw piece at position
		float t = LerpTime;
		Vector2 end = EndPos;
		Vector2 start = StartPos;
		// if (isFlipped) { start *= -1; end *= -1; }
		// TODO: finish implementing move animations based on moveflag

		BoardUI.DrawPiece(piece, (isFlipped ? -1 : 1) * ((t*end)+((1-t)*start)));
	}

	public void Update(float dt) {
		// Update T [0, 1]
		ElapsedTime += dt;
		if (LerpTime >= 1f) {
			HasFinished = true;
		}
	}
}