using ChessBot.Engine;


namespace ChessBot.Application;

public class BoardAnimation {
	// public Piece[] StartIndex, EndIndex;
	// public Vector2 StartPos, EndPos;
	public float TotalTime, ElapsedTime;
	public bool HasFinished=false;

	public ulong identicalPieces;
	List<Animation> leftoverAnimations;


	public BoardAnimation(Piece[] start, Piece[] end, float totalTime) {
		TotalTime = totalTime;
		List<(Piece piece, int index)> startPiecesThatDontFit = new List<(Piece piece, int index)>();
		List<(Piece piece, int index)> endPiecesThatDontFit = new List<(Piece piece, int index)>();

		leftoverAnimations = new List<Animation>();

		for (int i=0; i<64; i++) { // Iterate over each array to 
			Piece pieceOnStart = start[i]; Piece pieceOnEnd = end[i];
			if ((! pieceOnStart.IsNull) && pieceOnStart == pieceOnEnd) {
				identicalPieces |= (1ul << i);
				continue;
			}
			startPiecesThatDontFit.Add((pieceOnStart, i));
			endPiecesThatDontFit.Add((pieceOnEnd, i));
		}
		// for (int k=0; k<64; k++) {
		// 	ulong square = 1ul << k;
		// 	if ((square & identicalPieces) != 0) {
		// 		// Console.WriteLine($"Ident square {k}");
		// 	}
		// }

		for (int i=startPiecesThatDontFit.Count-1; i>-1; i--) {
			Piece startPiece = startPiecesThatDontFit[i].piece;
			for (int j=endPiecesThatDontFit.Count-1; j>-1; j--) {
				Piece endPiece = endPiecesThatDontFit[j].piece;
				if (startPiece == endPiece) {
					leftoverAnimations.Add(new Animation(startPiecesThatDontFit[i].index, endPiecesThatDontFit[j].index, startPiece, totalTime));
					endPiecesThatDontFit.RemoveAt(j);
					startPiecesThatDontFit.RemoveAt(i);
					break;
				}
			}
		}
		if (endPiecesThatDontFit.Count > 0) {
			
			foreach ((Piece piece, int index) tup in endPiecesThatDontFit) {
				Random r = new Random();
				leftoverAnimations.Add(new Animation((int)(r.NextDouble()*64), tup.index, tup.piece, totalTime));
			}
		}
	}

	public float LerpTime => ElapsedTime/TotalTime;

	public void Draw() {
		// Draw piece at position
		foreach (Animation anim in leftoverAnimations) {
			anim.Draw();
		}
	}

	public void Update(float dt) {
		// Update T [0, 1]
		foreach (Animation anim in leftoverAnimations) {
			anim.Update(dt);
		}
		ElapsedTime += dt;
		if (LerpTime >= 1f) {
			HasFinished = true;
		}
	}
}