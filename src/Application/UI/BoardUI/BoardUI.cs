
using Raylib_cs;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application;
public enum MoveSounds :int {
	None=0,
	Move=1,
	Capture=2,
	Check=3,
	Castle=4,
	Checkmate=5,
	Stalemate=6,
	GameEnd=7,
}
public class BoardUI {
	// public Piece[] boardToRender = new Piece[64];
	public ulong[,] pieceBitboards = new ulong[6,2];

	public static int squareSize = 100;
	public static Sound[] sounds = new Sound[0];
	public static Texture2D piecesTexture;
	public static Vector2 squareSizeV = new Vector2(squareSize);
	public bool IsFlipped;
	static readonly int[] pieceImageOrder = { 5, 3, 2, 4, 1, 0 };

	public int OppMovedFrom = -1;
	public int OppMovedTo	= -1;

	public int SelectedIndex = -1;
	public Move[] MovesForSelected = new Move[0];
	public bool IsDraggingPiece = false;

	public HashSet<(int tail, int head)> ArrowsOnBoard = new HashSet<(int tail, int head)>();
	public bool[] highlightedSquares = new bool[64];
	
	public List<PieceAnimation> activeAnimations = new List<PieceAnimation>();



	public BoardUI() {
		if (sounds.Length == 0) {
			BoardUI.sounds = new Sound[] {
				new Sound(), 
				Raylib.LoadSound(FileHelper.GetResourcePath("sounds/move-self.mp3")),
				Raylib.LoadSound(FileHelper.GetResourcePath("sounds/capture.mp3")),
				Raylib.LoadSound(FileHelper.GetResourcePath("sounds/move-check.mp3")),
				Raylib.LoadSound(FileHelper.GetResourcePath("sounds/castle.mp3")),
				Raylib.LoadSound(FileHelper.GetResourcePath("sounds/checkmate.mp3")),
				Raylib.LoadSound(FileHelper.GetResourcePath("sounds/stalemate.mp3")),
				Raylib.LoadSound(FileHelper.GetResourcePath("sounds/game-end.mp3"))
			};
		}
		piecesTexture = Raylib.LoadTexture(FileHelper.GetResourcePath("Pieces.png"));
		Raylib.GenTextureMipmaps(ref piecesTexture);
		Raylib.SetTextureWrap(piecesTexture, TextureWrap.TEXTURE_WRAP_CLAMP);
		Raylib.SetTextureFilter(piecesTexture, TextureFilter.TEXTURE_FILTER_BILINEAR);
	}

	public bool IsLightSquare(int i) => (((i&0b111) + (i>>3)) % 2 == 1);


	public void DrawBoardBorder() {
		UIHelper.DrawRectangleAsBorder(new Rectangle(-4*squareSize, -4*squareSize, 8*squareSize, 8*squareSize), BoardTheme.borderCol, 12);
	}

	public void DrawBoardSquares() {
		for (int i=0;i<64;i++) {	
			bool IsLight = IsLightSquare(i);			
			Vector2 squarePos = new Vector2(i & 0b111, 7-(i>>3));
			Vector2 temp = squareSize * (squarePos - new Vector2(3.5f));
			UIHelper.DrawRectangleCentered(temp, squareSizeV, IsLight ? BoardTheme.lightCol : BoardTheme.darkCol);
			Color textColor = (!IsLight) ? BoardTheme.lightCol : BoardTheme.darkCol;
			if (squarePos.Y == 7) { // If square is on the bottom edge, draw the file
				UIHelper.DrawText($"{BoardHelper.fileNames[(! IsFlipped ? i&0b111 : 7-i&0b111)]}", temp+(squareSizeV/2)-(3*squareSizeV/128), squareSize/4, 0, textColor, UIHelper.AlignH.Right, UIHelper.AlignV.Bottom);
			}
			if (squarePos.X == 0) { // If square is on the left edge, draw the rank
				UIHelper.DrawText($"{BoardHelper.rankNames[(! IsFlipped ? i>>3 : 7-(i>>3))]}", temp-(squareSizeV/2)+(3*squareSizeV/128), squareSize/4, 0, textColor, UIHelper.AlignH.Left, UIHelper.AlignV.Top);
			}
			if (highlightedSquares[IsFlipped ? 63-i : i]) {
				UIHelper.DrawRectangleCentered(temp, squareSizeV, BoardTheme.selectedHighlight);
			}
		}
		foreach (Move move in MovesForSelected) {
			Vector2 squarePos = new Vector2(move.TargetSquare%8, (7-move.TargetSquare/8));
			Vector2 temp = squareSize * (squarePos - new Vector2(3.5f));
			if (IsFlipped) {
				temp *= -1;
			}
			// squareColors[move.TargetSquare] = IsLightSquare(move.TargetSquare) ? BoardTheme.legalLight : BoardTheme.legalDark;
			UIHelper.DrawRectangleCentered(temp, squareSizeV, BoardTheme.legalHighlight);
		}

		if (SelectedIndex != -1) {
			Vector2 squarePos = new Vector2(SelectedIndex%8, (7-SelectedIndex/8));
			Vector2 temp = squareSize * (squarePos - new Vector2(3.5f));
			if (IsFlipped) {
				temp *= -1;
			}
			// squareColors[selectedIndex] = IsLightSquare(selectedIndex) ? BoardTheme.selectedLight : BoardTheme.selectedLight; // TODO fix redundant line
			UIHelper.DrawRectangleCentered(temp, squareSizeV, BoardTheme.movedFromHighlight);
		}
	}

	public void DrawPiecesOnBoard() {
		ulong animatedSquares = 0;
		foreach (PieceAnimation anim in activeAnimations) {
			animatedSquares |= anim.affectedSquares;
			if (anim.ShouldPlaySound) { Raylib.PlaySound(sounds[anim.soundEnum]); }
		}

		Piece draggedPiece = 0;
		for (int i=0; i<64;i++) {
			int x = i & 0b111; int y = 7-(i>>3);

			Vector2 indexVector = new Vector2(x, y);

			Vector2 renderPosition = (indexVector - new Vector2(3.5f)) * squareSize;
			if (IsFlipped) {
				renderPosition *= -1;
			}
			//	this is the control
			//	vvv     vvv single out the 1st bit
			// Console.WriteLine(i);
			if (BitboardHelper.IsSquareSet(animatedSquares, i)) { continue; }

			Piece pieceToDraw = Piece.None;
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Pawn-1, 0], i)) { pieceToDraw = Piece.WhitePawn; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Knight-1, 0], i)) { pieceToDraw = Piece.WhiteKnight; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Bishop-1, 0], i)) { pieceToDraw = Piece.WhiteBishop; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Rook-1, 0], i)) { pieceToDraw = Piece.WhiteRook; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Queen-1, 0], i)) { pieceToDraw = Piece.WhiteQueen; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.King-1, 0], i)) { pieceToDraw = Piece.WhiteKing; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Pawn-1, 1], i)) { pieceToDraw = Piece.BlackPawn; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Knight-1, 1], i)) { pieceToDraw = Piece.BlackKnight; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Bishop-1, 1], i)) { pieceToDraw = Piece.BlackBishop; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Rook-1, 1], i)) { pieceToDraw = Piece.BlackRook; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.Queen-1, 1], i)) { pieceToDraw = Piece.BlackQueen; } else
			if (BitboardHelper.IsSquareSet(pieceBitboards[Piece.King-1, 1], i)) { pieceToDraw = Piece.BlackKing; }

			if (SelectedIndex == i && IsDraggingPiece) {
				draggedPiece = pieceToDraw;
				continue;
			}
			
			

			DrawPiece(pieceToDraw, renderPosition);
		}

		for (int i=activeAnimations.Count-1;i>-1; i--) {
			PieceAnimation anim = activeAnimations[i];
			if (anim.HasFinished) { activeAnimations.RemoveAt(i); continue;}
			anim.Draw(IsFlipped);
		}


		if (SelectedIndex != -1 && IsDraggingPiece) {
			Vector2 mousePos = Raylib.GetMousePosition() - View.screenSize/2; // Mouse position in camera space converted to worldspace (centered at the origin)

			DrawPiece(draggedPiece, Vector2.Clamp(mousePos, -4*squareSizeV, 4*squareSizeV));

		}
	}

	public void DrawArrowsOnBoard() {
		float triangleHeight = 3.0f/8.0f;
		float triangleBaseWidth = 1.0f/2.0f;


		float widthFactor = ApplicationSettings.Get().uiArrowWidthFactor;
		foreach ((int tail, int head) arrow in ArrowsOnBoard) {
			int tail = arrow.tail; int head = arrow.head;


			Vector2 tailPos = new Vector2(tail & 0b111, 7-(tail>>3));						// All in board space [0, 8)
			Vector2 headPos = new Vector2(head & 0b111, 7-(head>>3));						// All in board space [0, 8)
			Vector2 deltaV = headPos-tailPos;
			bool isKnight = false;
			int sign = (Math.Sign(head-tail) == -1 ? 1 : -1);

			if (new int[] { 6, 15, 17, 10, -6, -15, -17, -10 }.Contains(head-tail)) { // Is a Knight move, draw differently
				isKnight = true;
				Vector2 newTail = new Vector2();
				Vector2 newHead = new Vector2();
				switch (head-tail) {
					case   6:
						newTail = tailPos - new Vector2(triangleHeight, 0);
						newHead = new Vector2(headPos.X, tailPos.Y) - new Vector2(widthFactor/2, 0);
						tailPos = new Vector2(headPos.X, tailPos.Y) - new Vector2(0, widthFactor/2);
						break;
					case  15:
						newTail = tailPos - new Vector2(0, triangleHeight);
						newHead = new Vector2(tailPos.X, headPos.Y) - new Vector2(0, widthFactor/2);
						tailPos = new Vector2(tailPos.X, headPos.Y) - new Vector2(widthFactor/2, 0);
						break;
					case  17:
						newTail = tailPos - new Vector2(0, triangleHeight);
						newHead = new Vector2(tailPos.X, headPos.Y) - new Vector2(0, widthFactor/2);
						tailPos = new Vector2(tailPos.X, headPos.Y) + new Vector2(widthFactor/2, 0);
						break;
					case  10:
						newTail = tailPos + new Vector2(triangleHeight, 0);
						newHead = new Vector2(headPos.X, tailPos.Y) + new Vector2(widthFactor/2, 0);
						tailPos = new Vector2(headPos.X, tailPos.Y) - new Vector2(0, widthFactor/2);
						break;
					case  -6:
						newTail = tailPos + new Vector2(triangleHeight, 0);
						newHead = new Vector2(headPos.X, tailPos.Y) + new Vector2(widthFactor/2, 0);
						tailPos = new Vector2(headPos.X, tailPos.Y) + new Vector2(0, widthFactor/2);
						break;
					case -15:
						newTail = tailPos + new Vector2(0, triangleHeight);
						newHead = new Vector2(tailPos.X, headPos.Y) + new Vector2(0, widthFactor/2);
						tailPos = new Vector2(tailPos.X, headPos.Y) + new Vector2(widthFactor/2, 0);
						break;
					case -17:
						newTail = tailPos + new Vector2(0, triangleHeight);
						newHead = new Vector2(tailPos.X, headPos.Y) + new Vector2(0, widthFactor/2);
						tailPos = new Vector2(tailPos.X, headPos.Y) - new Vector2(widthFactor/2, 0);
						break;
					case -10:
						newTail = tailPos - new Vector2(triangleHeight, 0);
						newHead = new Vector2(headPos.X, tailPos.Y) - new Vector2(widthFactor/2, 0);
						tailPos = new Vector2(headPos.X, tailPos.Y) + new Vector2(0, widthFactor/2);
						break;
				}
				newTail = squareSize * (newTail - new Vector2(3.5f)) * (IsFlipped ? -1 : 1);
				newHead = squareSize * (newHead - new Vector2(3.5f)) * (IsFlipped ? -1 : 1);

				Raylib.DrawLineEx(newTail, newHead, squareSize*widthFactor, BoardTheme.arrowOutlineHighlight);
			}





			Vector2 basisHeadWing1 = new Vector2(triangleHeight, +triangleBaseWidth/2);	// All in board space [0, 8) (Technically this isn't)
			Vector2 basisHeadWing2 = new Vector2(triangleHeight, -triangleBaseWidth/2);	// All in board space [0, 8)

			float fullArrowMag = Vector2.Distance(tailPos, headPos);
			double angle = Math.Acos(Vector2.Dot(tailPos-headPos, new Vector2(1, 0)) / fullArrowMag) * (head - tail < 0 ? -1 : 1);

			double sinTheta = Math.Sin(angle);
			double cosTheta = Math.Cos(angle);

			Vector2 headWing1 = new Vector2((float)(basisHeadWing1.X * cosTheta - basisHeadWing1.Y * sinTheta), (float)(basisHeadWing1.X * sinTheta + basisHeadWing1.Y * cosTheta));
			Vector2 headWing2 = new Vector2((float)(basisHeadWing2.X * cosTheta - basisHeadWing2.Y * sinTheta), (float)(basisHeadWing2.X * sinTheta + basisHeadWing2.Y * cosTheta));

			Vector2 finalTailPos 	= squareSize * (  tailPos - new Vector2(3.5f)) * (IsFlipped ? -1 : 1);
			Vector2 finalHeadPos 	= squareSize * (  headPos - new Vector2(3.5f)) * (IsFlipped ? -1 : 1);
			Vector2 finalHeadWing1 	= finalHeadPos + squareSize * headWing1 * (IsFlipped ? -1 : 1);
			Vector2 finalHeadWing2 	= finalHeadPos + squareSize * headWing2 * (IsFlipped ? -1 : 1);


			if (isKnight) Raylib.DrawLineEx(finalTailPos, (finalHeadWing2 + finalHeadWing1)/2, squareSize*widthFactor, BoardTheme.arrowOutlineHighlight);
			else Raylib.DrawLineEx(finalTailPos+(finalHeadPos-(finalHeadWing2 + finalHeadWing1)/2), (finalHeadWing2 + finalHeadWing1)/2, squareSize*widthFactor, BoardTheme.arrowOutlineHighlight);
			Raylib.DrawTriangle(finalHeadPos, finalHeadWing1, finalHeadWing2, BoardTheme.arrowOutlineHighlight);
		}
	}

	public List<Vector2> GetVerticesToBoardSquare(int start, int end) { // Does not compose the arrow on the end
		List<Vector2> vertexList = new List<Vector2>();


		return vertexList;
	}

	public void DeselectActiveSquare() {
		SelectedIndex = -1;
		MovesForSelected = new Move[0];
	}

	public static void DrawPiece(Piece piece, Vector2 posAbsCenter, float alpha = 1) { //* Copied from SebLague
		if (piece != Piece.None) {
			int type = piece.Type;
			bool white = piece.Color == Piece.White;
			Rectangle srcRect = GetPieceTextureRect(type, white);
			Rectangle targRect = new Rectangle((int)posAbsCenter.X-(squareSize/2), (int)posAbsCenter.Y-(squareSize/2), squareSize, squareSize);

			Color tint = new Color(255, 255, 255, (int)MathF.Round(255 * alpha));
			Raylib.DrawTexturePro(piecesTexture, srcRect, targRect, new Vector2(0, 0), 0, tint);
		}
	}
	static Rectangle GetPieceTextureRect(int pieceType, bool isWhite) { //* Copied from SebLague
		const int size = 666;
		return new Rectangle(size * pieceImageOrder[pieceType - 1], isWhite ? 0 : size, size, size);
	}

	public void Release() {
		foreach (Sound sound in BoardUI.sounds) {
			Raylib.UnloadSound(sound);
		}
		Raylib.UnloadTexture(piecesTexture);
	}
}
