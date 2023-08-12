using ChessBot.Engine;


namespace ChessBot.Application;

public class BoardAnimation {
	// public Piece[] StartIndex, EndIndex;
	// public Vector2 StartPos, EndPos;
	public float TotalTime, ElapsedTime;
	public bool HasFinished=false;

	public ulong identicalPieces;
	List<Animation> leftoverAnimations;


	public BoardAnimation(Piece[] oldBoard, Piece[] newBoard, float totalTime) {
		TotalTime = totalTime;
		List<(Piece piece, int index)> oldPiecesThatDontFit = new List<(Piece piece, int index)>();
		List<(Piece piece, int index)> newPiecesThatDontFit = new List<(Piece piece, int index)>();

		leftoverAnimations = new List<Animation>();

		for (int i=0; i<64; i++) { // Iterate over every square in board, if the piece on the old square matches the new square, cache that and don't do anything
			Piece pieceOnStart = oldBoard[i]; Piece pieceOnEnd = newBoard[i];

			if ((! pieceOnStart.IsNull) && pieceOnStart == pieceOnEnd) {
				identicalPieces |= (1ul << i);
				continue;
			}
			if (! pieceOnStart.IsNull) oldPiecesThatDontFit.Add((pieceOnStart, i));
			if (! pieceOnEnd.IsNull) newPiecesThatDontFit.Add((pieceOnEnd, i));
		}

		if (oldPiecesThatDontFit.Count == 0 && newPiecesThatDontFit.Count == 0) { // Board states are the same, no reason to animate
			HasFinished = true;
			return;
		}

		for (int i=oldPiecesThatDontFit.Count-1; i>-1; i--) {
			Piece startPiece = oldPiecesThatDontFit[i].piece;
			for (int j=newPiecesThatDontFit.Count-1; j>-1; j--) {
				Piece endPiece = newPiecesThatDontFit[j].piece;
				if (startPiece == endPiece) {
					leftoverAnimations.Add(new Animation(oldPiecesThatDontFit[i].index, newPiecesThatDontFit[j].index, startPiece, totalTime));
					newPiecesThatDontFit.RemoveAt(j);
					oldPiecesThatDontFit.RemoveAt(i);
					break;
				}
			}
		} //* All that's left now are pieces on the old board that arent on the new board, and vice versa

		

		//* Pieces on the old board that don't fit do not matter because they won't have a place at the end of the animation
		//* This iterates and adds new animations for "newly generated" pieces on the new state; Animates from a random index to add flavor
		Random r = new Random();
		if (newPiecesThatDontFit.Count > 0) {
			foreach ((Piece piece, int index) tup in newPiecesThatDontFit) {
				// identicalPieces |= (1ul << tup.index);
				leftoverAnimations.Add(new Animation(tup.index, tup.index, tup.piece, totalTime));
			}
		}
	}

	public float LerpTime => ElapsedTime/TotalTime;

	public void Draw(bool isFlipped) {
		// Draw piece at position
		foreach (Animation anim in leftoverAnimations) {
			anim.Draw(isFlipped);
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