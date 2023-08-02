
using Raylib_cs;
using System.Numerics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class BoardUI {
		Texture2D piecesTexture;
		public int squareSize = 100;
		static readonly int[] pieceImageOrder = { 5, 3, 2, 4, 1, 0 };

		public int oppMovedFrom = -1;
		public int oppMovedTo	= -1;

		public int selectedIndex = -1;
		public bool[] selectedSquares = new bool[64];
		public Move[] movesForSelected = new Move[0];
		public Color[] squareColors = new Color[64];
		public bool isDraggingPiece = false;


		public BoardUI() {
			piecesTexture = Raylib.LoadTexture(UIHelper.GetResourcePath("Pieces.png"));
            Raylib.GenTextureMipmaps(ref piecesTexture);
            Raylib.SetTextureWrap(piecesTexture, TextureWrap.TEXTURE_WRAP_CLAMP);
            Raylib.SetTextureFilter(piecesTexture, TextureFilter.TEXTURE_FILTER_BILINEAR);
			
			for (int i=0;i<64;i++) {
				squareColors[i] = IsLightSquare(i) ? BoardTheme.lightCol : BoardTheme.darkCol;
			}
		}

		public bool IsLightSquare(int i) => ((i%8 + i/8) % 2 == 0);


		public void DrawBoardBorder() {
			int boardStartX = -squareSize * 4;
			int boardStartY = -squareSize * 4;
			int w = 12;
			Raylib.DrawRectangle(boardStartX-w, boardStartY - w, 8*squareSize+2*w, 8*squareSize+2*w, BoardTheme.borderCol);
		}

		public void DrawBoardSquares() {


			for (int i=0;i<64;i++) {
				Vector2 squarePos = new Vector2(i%8, (7-i/8));
				Raylib.DrawRectangle((int)( squareSize * (squarePos.X-4) ), (int)( squareSize * (squarePos.Y-4) ), squareSize, squareSize, squareColors[i]);
				if (selectedSquares[i]) {
					Raylib.DrawRectangle((int)( squareSize * (squarePos.X-4) ), (int)( squareSize * (squarePos.Y-4) ), squareSize, squareSize, BoardTheme.selectedHighlight);
				}
			}
			foreach (Move move in movesForSelected) {
				Vector2 squarePos = new Vector2(move.TargetSquare%8, (7-move.TargetSquare/8));
				// squareColors[move.TargetSquare] = IsLightSquare(move.TargetSquare) ? BoardTheme.legalLight : BoardTheme.legalDark;
				Raylib.DrawRectangle((int)( squareSize * (squarePos.X-4) ), (int)( squareSize * (squarePos.Y-4) ), squareSize, squareSize, BoardTheme.legalHighlight);
			}

			if (selectedIndex != -1) {
				Vector2 squarePos = new Vector2(selectedIndex%8, (7-selectedIndex/8));
				// squareColors[selectedIndex] = IsLightSquare(selectedIndex) ? BoardTheme.selectedLight : BoardTheme.selectedLight; // TODO fix redundant line
				Raylib.DrawRectangle((int)( squareSize * (squarePos.X-4) ), (int)( squareSize * (squarePos.Y-4) ), squareSize, squareSize, BoardTheme.movedFromHighlight);
			}
		}

		public void ResetBoardColors() {
			for (int i=0;i<64;i++) {
				squareColors[i] = IsLightSquare(i) ? BoardTheme.lightCol : BoardTheme.darkCol;
			}
		}


		public void DrawPiecesOnBoard(Board board) {
			//* This is kind of nasty to have this inside of the draw method but it's the only way to 
			//* add some QOL functionality without cluttering up other things
			for (int i=0; i<64;i++) {
				int x = i & 0b111; int y = (7-i >> 3);

				Vector2 indexVector = new Vector2(x, y);

				Vector2 renderPosition = (indexVector - new Vector2(4, -3)) * squareSize;

				if (selectedIndex == i && isDraggingPiece) {
					continue;
				}

				DrawPiece(board.GetSquare(i), renderPosition);
			}
			if (selectedIndex != -1 && isDraggingPiece) {
				DrawPiece(board.GetSquare(selectedIndex), Raylib.GetMousePosition()-View.screenSize/2-new Vector2(squareSize)/2);
			}
		}

		public void DeselectActiveSquare() {
			selectedIndex = -1;
			movesForSelected = new Move[0];
		}

		public void DrawPiece(Piece piece, Vector2 posTopLeft, float alpha = 1) { //* Copied from SebLague
            if (piece != Piece.None) {
                int type = piece.Type;
                bool white = piece.Color == Piece.White;
                Rectangle srcRect = GetPieceTextureRect(type, white);
                Rectangle targRect = new Rectangle((int)posTopLeft.X, (int)posTopLeft.Y, squareSize, squareSize);

                Color tint = new Color(255, 255, 255, (int)MathF.Round(255 * alpha));
                Raylib.DrawTexturePro(piecesTexture, srcRect, targRect, new Vector2(0, 0), 0, tint);
            }
        }
		static Rectangle GetPieceTextureRect(int pieceType, bool isWhite) { //* Copied from SebLague
            const int size = 666;
            return new Rectangle(size * pieceImageOrder[pieceType - 1], isWhite ? 0 : size, size, size);
        }

	}
}