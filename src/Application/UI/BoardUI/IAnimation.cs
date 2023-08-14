using Raylib_cs;
using ChessBot.Engine;
using System.Numerics;

namespace ChessBot.Application;

public interface IAnimation {
	public float TotalTime {
		get; set;
	}
	public float ElapsedTime {
		get; set;
	}

	public void Draw(bool isFlipped);
	public void Update(float dt);
}