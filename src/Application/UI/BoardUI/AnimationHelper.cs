using Raylib_cs;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application;

public static class AnimationHelper {
	public static List<PieceAnimation> FromGamestate(Gamestate gamestate, float lag=0) {
		// Interpolating animation between positions
		List<PieceAnimation> animations = new List<PieceAnimation>();
		float tTotal = ApplicationSettings.Get().uiMoveTime;

		animations.Add(new PieceAnimation(gamestate.moveMade.StartSquare, gamestate.moveMade.TargetSquare, gamestate.pieceMoved, tTotal, lag, soundEnum:gamestate.soundPlayed));

		if (gamestate.moveMade.Flag == Move.NoFlag) { return animations; }

		if (gamestate.moveMade.Flag == Move.CastleFlag) {
			switch (gamestate.moveMade.TargetSquare, gamestate.pieceMoved.Color == Piece.White) {
				case (BoardHelper.c1, true):
					animations.Add(new PieceAnimation(BoardHelper.a1, BoardHelper.d1, Piece.Rook|gamestate.pieceMoved.Color, tTotal, lag)); break;
				case (BoardHelper.g1, true):
					animations.Add(new PieceAnimation(BoardHelper.h1, BoardHelper.f1, Piece.Rook|gamestate.pieceMoved.Color, tTotal, lag)); break;
				case (BoardHelper.c8, false):
					animations.Add(new PieceAnimation(BoardHelper.a8, BoardHelper.d8, Piece.Rook|gamestate.pieceMoved.Color, tTotal, lag)); break;
				case (BoardHelper.g8, false):
					animations.Add(new PieceAnimation(BoardHelper.h8, BoardHelper.f8, Piece.Rook|gamestate.pieceMoved.Color, tTotal, lag)); break;
				default:
					throw new Exception("False castle flag in move");
			}
		}
		return animations;
	}
	public static List<PieceAnimation> ReverseFromGamestate(Gamestate gamestate, float lag=0) {
		// Interpolating animation between positions
		List<PieceAnimation> animations = new List<PieceAnimation>();
		float tTotal = ApplicationSettings.Get().uiMoveTime;
		int soundToPlay = gamestate.soundPlayed;

		// If the sound played is mate (of any kind) switch to a standard move sound when reversing
		if (gamestate.soundPlayed == (int)MoveSounds.Checkmate || gamestate.soundPlayed == (int)MoveSounds.Stalemate) {
			soundToPlay = (int)MoveSounds.Move;
		}
		

		animations.Add(new PieceAnimation(gamestate.moveMade.TargetSquare, gamestate.moveMade.StartSquare, gamestate.pieceMoved, tTotal, lag, soundEnum:soundToPlay));

		if (gamestate.moveMade.Flag == Move.NoFlag) { return animations; }

		if (gamestate.moveMade.Flag == Move.CastleFlag) {
			switch (gamestate.moveMade.TargetSquare, gamestate.pieceMoved.Color == Piece.White) {
				case (BoardHelper.c1, true):
					animations.Add(new PieceAnimation(BoardHelper.d1, BoardHelper.a1, Piece.Rook|gamestate.pieceMoved.Color, tTotal, lag)); break;
				case (BoardHelper.g1, true):
					animations.Add(new PieceAnimation(BoardHelper.f1, BoardHelper.h1, Piece.Rook|gamestate.pieceMoved.Color, tTotal, lag)); break;
				case (BoardHelper.c8, false):
					animations.Add(new PieceAnimation(BoardHelper.d8, BoardHelper.a8, Piece.Rook|gamestate.pieceMoved.Color, tTotal, lag)); break;
				case (BoardHelper.g8, false):
					animations.Add(new PieceAnimation(BoardHelper.f8, BoardHelper.h8, Piece.Rook|gamestate.pieceMoved.Color, tTotal, lag)); break;
				default:
					throw new Exception("False castle flag in move");
			}
		}
		// if (gamestate.moveMade.IsPromotion) {

		// }

		return animations;
	}
	public static List<PieceAnimation> FromBoardChange(Piece[] oldBoard, Piece[] newBoard, float totalTime) {
		List<(Piece piece, int index)> oldPiecesThatDontFit = new List<(Piece piece, int index)>();
		List<(Piece piece, int index)> newPiecesThatDontFit = new List<(Piece piece, int index)>();

		List<PieceAnimation> leftoverAnimations = new List<PieceAnimation>();

		for (int i=0; i<64; i++) { // Iterate over every square in board, if the piece on the old square matches the new square, cache that and don't do anything
			Piece pieceOnStart = oldBoard[i]; Piece pieceOnEnd = newBoard[i];

			if ((! pieceOnStart.IsNone) && pieceOnStart == pieceOnEnd) {
				continue;
			}
			if (! pieceOnStart.IsNone) oldPiecesThatDontFit.Add((pieceOnStart, i));
			if (! pieceOnEnd.IsNone) newPiecesThatDontFit.Add((pieceOnEnd, i));
		}

		if (oldPiecesThatDontFit.Count == 0 && newPiecesThatDontFit.Count == 0) { // Board states are the same, no reason to animate
			return leftoverAnimations;
		}

		for (int i=oldPiecesThatDontFit.Count-1; i>-1; i--) {
			Piece startPiece = oldPiecesThatDontFit[i].piece;
			for (int j=newPiecesThatDontFit.Count-1; j>-1; j--) {
				Piece endPiece = newPiecesThatDontFit[j].piece;
				if (startPiece == endPiece) {
					leftoverAnimations.Add(new PieceAnimation(oldPiecesThatDontFit[i].index, newPiecesThatDontFit[j].index, startPiece, totalTime));
					newPiecesThatDontFit.RemoveAt(j);
					oldPiecesThatDontFit.RemoveAt(i);
					break;
				}
			}
		} //* All that's left now are pieces on the old board that arent on the new board, and vice versa

		

		//* Pieces on the old board that don't fit do not matter because they won't have a place at the end of the animation
		//* This iterates and adds new animations for "newly generated" pieces on the new state; Animates from a random index to add flavor
		if (newPiecesThatDontFit.Count > 0) {
			foreach ((Piece piece, int index) tup in newPiecesThatDontFit) {
				// identicalPieces |= (1ul << tup.index);
				leftoverAnimations.Add(new PieceAnimation(tup.index, tup.index, tup.piece, totalTime));
			}
		}

		return leftoverAnimations;
	}
}
