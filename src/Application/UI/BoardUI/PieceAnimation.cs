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
	public int soundEnum;
	public bool ShouldPlaySound = false;

	public PieceAnimation(int start, int end, Piece piece, float tTotal, float lag=0.0f, int soundEnum=0) {
		StartPos = BoardUI.squareSize*(new Vector2(start & 0b111, (7-(start>>3))) - new Vector2(3.5f));
		EndPos = BoardUI.squareSize*(new Vector2(end & 0b111, (7-(end>>3))) - new Vector2(3.5f));
		TotalTime = tTotal;
		ElapsedTime = lag; // lag is the delay before the animation starts. Can be any number, but is meant to be negative
		this.piece = piece;
		this.soundEnum = soundEnum;
		affectedSquares = (1ul << end); // Doesnt add start so that if the move is reversed, it will render the piece underneath the square it's moving from
	}
	public virtual void Draw(bool isFlipped) {
		float t = LerpTime;
		BoardUI.DrawPiece(piece, (isFlipped ? -1 : 1) * ((t*EndPos)+((1-t)*StartPos)));
	}
	public virtual void Update(float dt) {
		ShouldPlaySound = ElapsedTime <= 0 && 0f < ElapsedTime+dt;
		// if (ShouldPlaySound) {
		// 	Console.WriteLine("Gaming like fortnite!!!");
		// }
		ElapsedTime += dt;
	}
}