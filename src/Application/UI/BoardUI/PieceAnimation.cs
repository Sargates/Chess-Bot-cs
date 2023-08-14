using Raylib_cs;
using ChessBot.Engine;
using System.Numerics;

namespace ChessBot.Application;

public class PieceAnimation {
	public Vector2 StartPos, EndPos;
	public float TotalTime, ElapsedTime=0;
	public float LerpTime => Math.Clamp(ElapsedTime/TotalTime, 0, 1);
	public bool HasFinished=> LerpTime==1;
	public Piece piece;
	public ulong affectedSquares;
	public PieceAnimation(int start, int end, Piece piece, float tTotal, float lag=0.0f) {
		StartPos = BoardUI.squareSize*(new Vector2(start & 0b111, (7-(start>>3))) - new Vector2(3.5f));
		EndPos = BoardUI.squareSize*(new Vector2(end & 0b111, (7-(end>>3))) - new Vector2(3.5f));
		TotalTime = tTotal;
		ElapsedTime = lag; // lag is the delay before the animation starts. Can be any number, but is meant to be negative
		this.piece = piece;
		affectedSquares = (1ul << start) | (1ul << end);
	}
	public virtual void Draw(bool isFlipped) {
		float t = LerpTime;
		BoardUI.DrawPiece(piece, (isFlipped ? -1 : 1) * ((t*EndPos)+((1-t)*StartPos)));
	}
	public virtual void Update(float dt) {
		ElapsedTime += dt;
	}

	public virtual List<PieceAnimation> ToPieceAnimationList() {
		return new PieceAnimation[] {this}.ToList();
	}
}