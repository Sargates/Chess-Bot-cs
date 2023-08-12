
using Raylib_cs;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class BoardUI {
		public static Texture2D piecesTexture;
		public static int squareSize = 100;
		public static Vector2 squareSizeV = new Vector2(squareSize);
		static readonly int[] pieceImageOrder = { 5, 3, 2, 4, 1, 0 };

		public int oppMovedFrom = -1;
		public int oppMovedTo	= -1;

		public int selectedIndex = -1;
		public bool[] highlightedSquares = new bool[64];
		public Move[] movesForSelected = new Move[0];
		public Color[] squareColors = new Color[64];
		public bool isDraggingPiece = false;
		public BoardAnimation? activeAnimation;
		public bool isFlipped;


		public BoardUI() {
			piecesTexture = Raylib.LoadTexture(FileHelper.GetResourcePath("Pieces.png"));
            Raylib.GenTextureMipmaps(ref piecesTexture);
            Raylib.SetTextureWrap(piecesTexture, TextureWrap.TEXTURE_WRAP_CLAMP);
            Raylib.SetTextureFilter(piecesTexture, TextureFilter.TEXTURE_FILTER_BILINEAR);

			for (int i=0;i<64;i++) {
				squareColors[i] = IsLightSquare(i) ? BoardTheme.lightCol : BoardTheme.darkCol;
			}
		}

		public bool IsLightSquare(int i) => (((i&0b111) + (i>>3)) % 2 == 1);


		public void DrawBoardBorder() {
			int w = 12;
			DrawRectangleCentered(new Vector2(), new Vector2(8*squareSize+2*w), BoardTheme.borderCol);
		}

		public void DrawBoardSquares() {
			for (int i=0;i<64;i++) {				
				Vector2 squarePos = new Vector2(i & 0b111, 7-(i>>3));
				Vector2 temp = squareSize * (squarePos - new Vector2(3.5f));
				DrawRectangleCentered(temp, squareSizeV, squareColors[i]);
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

		public void ResetBoardColors() {
			for (int i=0;i<64;i++) { squareColors[i] = IsLightSquare(i) ? BoardTheme.lightCol : BoardTheme.darkCol; }
		}


		public void DrawPiecesOnBoard(Board board) {
			//* This is kind of nasty to have this inside of the draw method but it's the only way to 
			//* add some QOL functionality without cluttering up other things
			// float snappingFactor = 0.675f; // Domain: [0, 1]; 0 for no snaping, 1 for snapping within 1 board square
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
				if (0ul == ((1ul) & ((activeAnimation?.identicalPieces>>i) ?? 1ul))) {
					continue;
				}

				DrawPiece(board.GetSquare(i), renderPosition);
			}
			if (selectedIndex != -1 && isDraggingPiece) {
				Vector2 mousePos = Raylib.GetMousePosition() - View.screenSize/2; // Mouse position in camera space converted to worldspace (centered at the origin)
				Vector2 renderedPosition = cachedRenderPos; // center of selected square
				
				// Checking if either X or Y is greater than the snappingFactor, in terms of half the square size
				// if (Math.Max(Math.Abs((mousePos - renderedPosition).X), Math.Abs((mousePos - renderedPosition).Y)) < (squareSize/2) * snappingFactor) {
				// 	DrawPiece(board.GetSquare(selectedIndex), cachedRenderPos);
				// 	return;
				// }

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