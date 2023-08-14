
using Raylib_cs;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public enum SoundStates :int {
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
		public static Sound[] sounds = new Sound[0];

		public static Texture2D piecesTexture;
		public static int squareSize = 100;
		public static Vector2 squareSizeV = new Vector2(squareSize);
		static readonly int[] pieceImageOrder = { 5, 3, 2, 4, 1, 0 };

		public int oppMovedFrom = -1;
		public int oppMovedTo	= -1;

		public int selectedIndex = -1;
		public bool[] highlightedSquares = new bool[64];
		public Move[] movesForSelected = new Move[0];
		public bool isDraggingPiece = false;
		public List<PieceAnimation> activeAnimations = new List<PieceAnimation>();
		public bool isFlipped;


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
			int w = 12;
			DrawRectangleCentered(new Vector2(), new Vector2(8*squareSize+2*w), BoardTheme.borderCol);
		}

		public void DrawBoardSquares() {
			for (int i=0;i<64;i++) {	
				bool IsLight = IsLightSquare(i);			
				Vector2 squarePos = new Vector2(i & 0b111, 7-(i>>3));
				Vector2 temp = squareSize * (squarePos - new Vector2(3.5f));
				DrawRectangleCentered(temp, squareSizeV, IsLight ? BoardTheme.lightCol : BoardTheme.darkCol);
				Color textColor = (!IsLight) ? BoardTheme.lightCol : BoardTheme.darkCol;
				if (squarePos.Y == 7) { // If square is on the bottom edge, draw the file
					UIHelper.DrawText($"{BoardHelper.fileNames[(! isFlipped ? i&0b111 : 7-i&0b111)]}", temp+(squareSizeV/2)-(3*squareSizeV/128), squareSize/4, 0, textColor, UIHelper.AlignH.Right, UIHelper.AlignV.Bottom);
				}
				if (squarePos.X == 0) { // If square is on the left edge, draw the rank
					UIHelper.DrawText($"{BoardHelper.rankNames[(! isFlipped ? i>>3 : 7-(i>>3))]}", temp-(squareSizeV/2)+(3*squareSizeV/128), squareSize/4, 0, textColor, UIHelper.AlignH.Left, UIHelper.AlignV.Top);
				}
				if (highlightedSquares[isFlipped ? 63-i : i]) {
					DrawRectangleCentered(temp, squareSizeV, BoardTheme.selectedHighlight);
				}
			}
			foreach (Move move in movesForSelected) {
				Vector2 squarePos = new Vector2(move.TargetSquare%8, (7-move.TargetSquare/8));
				Vector2 temp = squareSize * (squarePos - new Vector2(3.5f));
				if (isFlipped) {
					temp *= -1;
				}
				// squareColors[move.TargetSquare] = IsLightSquare(move.TargetSquare) ? BoardTheme.legalLight : BoardTheme.legalDark;
				DrawRectangleCentered(temp, squareSizeV, BoardTheme.legalHighlight);
			}

			if (selectedIndex != -1) {
				Vector2 squarePos = new Vector2(selectedIndex%8, (7-selectedIndex/8));
				Vector2 temp = squareSize * (squarePos - new Vector2(3.5f));
				if (isFlipped) {
					temp *= -1;
				}
				// squareColors[selectedIndex] = IsLightSquare(selectedIndex) ? BoardTheme.selectedLight : BoardTheme.selectedLight; // TODO fix redundant line
				DrawRectangleCentered(temp, squareSizeV, BoardTheme.movedFromHighlight);
			}
		}

		public void DrawRectangleCentered(Vector2 position, Vector2 size, Color color) {
			Raylib.DrawRectangleV(position-size/2, size, color);
		}



		public void DrawPiecesOnBoard(Board board) {
			ulong animatedSquares = 0;
			foreach (PieceAnimation anim in activeAnimations) {
				animatedSquares |= anim.affectedSquares;
				if (anim.ShouldPlaySound) { Raylib.PlaySound(sounds[anim.soundEnum]); }
			}

			Vector2 cachedRenderPos = Vector2.Zero;
			for (int i=0; i<64;i++) {
				int x = i & 0b111; int y = 7-(i>>3);

				Vector2 indexVector = new Vector2(x, y);

				Vector2 renderPosition = (indexVector - new Vector2(3.5f)) * squareSize;
				if (isFlipped) {
					renderPosition *= -1;
				}

				if (selectedIndex == i && isDraggingPiece) {
					cachedRenderPos = renderPosition;
					continue;
				}

				// this is the control					if there is no animation render anyway
				//  vvv      vvv single out the 1st bit   					  vvv	
				if (1ul == (1ul & (animatedSquares>>i))) {
					continue;
				}

				DrawPiece(board.GetSquare(i), renderPosition);
			}

			for (int i=activeAnimations.Count-1;i>-1; i--) {
				PieceAnimation anim = activeAnimations[i];
				if (anim.HasFinished) { activeAnimations.RemoveAt(i); continue;}
				anim.Draw(isFlipped);
			}


			if (selectedIndex != -1 && isDraggingPiece) {
				Vector2 mousePos = Raylib.GetMousePosition() - View.screenSize/2; // Mouse position in camera space converted to worldspace (centered at the origin)
				Vector2 renderedPosition = cachedRenderPos; // center of selected square

				DrawPiece(board.GetSquare(selectedIndex), Vector2.Clamp(mousePos, -4*squareSizeV, 4*squareSizeV));

			}
		}

		public void DeselectActiveSquare() {
			selectedIndex = -1;
			movesForSelected = new Move[0];
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
			Raylib.UnloadTexture(piecesTexture);
		}

	}

	
}