
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
		public Move[] movesForSelected = new Move[0];
		public Color[] squareColors = new Color[64];


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

		public Color GetSquareColor(int index) {
			Color color = IsLightSquare(index) ? BoardTheme.lightCol : BoardTheme.darkCol;

			if (index == selectedIndex) {
				color = IsLightSquare(index) ? BoardTheme.selectedLight : BoardTheme.selectedDark;
			}


			return color;
		}

		public void DrawBoardSquares() {

			foreach (Move move in movesForSelected) {
				squareColors[move.TargetSquare] = IsLightSquare(move.TargetSquare) ? BoardTheme.legalLight : BoardTheme.legalDark;
			}

			if (selectedIndex != -1) {
				squareColors[selectedIndex] = IsLightSquare(selectedIndex) ? BoardTheme.selectedLight : BoardTheme.selectedLight; // TODO fix redundant line
			}

			for (int i=0;i<64;i++) {
				Vector2 squarePos = new Vector2(i%8, (7-i/8));
				// squareColors[i] = GetSquareColor(i);
				Raylib.DrawRectangle((int)( squareSize * (squarePos.X-4) ), (int)( squareSize * (squarePos.Y-4) ), squareSize, squareSize, squareColors[i]);
			}
		}

		public void ResetBoardColors() {
			for (int i=0;i<64;i++) {
				squareColors[i] = IsLightSquare(i) ? BoardTheme.lightCol : BoardTheme.darkCol;
			}
		}


		public void DrawPiecesOnBoard(Board board) {
			//	This is kind of nasty to have this inside of the draw method but it's the only way to 
			//	add some QOL functionality without cluttering up other things
			bool isMousePressed = Raylib.IsMouseButtonDown(MouseButton.MOUSE_BUTTON_LEFT);
			for (int i=0; i<64;i++) {
				int x = i & 0b111; int y = (7-i >> 3);

				Vector2 indexVector = new Vector2(x, y);

				Vector2 renderPosition = (indexVector - new Vector2(4, -3)) * squareSize;

				if (selectedIndex == i && isMousePressed) {
					continue;
				}

				DrawPiece(board.GetSquare(i), renderPosition);
			}
			if (selectedIndex != -1 && isMousePressed) {
				DrawPiece(board.GetSquare(selectedIndex), Raylib.GetMousePosition()-View.screenSize/2-new Vector2(squareSize)/2);
			}
		}

		public void DrawPiece(int piece, Vector2 posTopLeft, float alpha = 1) { //	Copied from SebLague
            if (piece != PieceHelper.None)
            {
                int type = PieceHelper.GetType(piece);
                bool white = PieceHelper.GetColor(piece) == PieceHelper.White;
                Rectangle srcRect = GetPieceTextureRect(type, white);
                Rectangle targRect = new Rectangle((int)posTopLeft.X, (int)posTopLeft.Y, squareSize, squareSize);

                Color tint = new Color(255, 255, 255, (int)MathF.Round(255 * alpha));
                Raylib.DrawTexturePro(piecesTexture, srcRect, targRect, new Vector2(0, 0), 0, tint);
            }
        }
		static Rectangle GetPieceTextureRect(int pieceType, bool isWhite) //	Copied from SebLague
        {
            const int size = 666;
            return new Rectangle(size * pieceImageOrder[pieceType - 1], isWhite ? 0 : size, size, size);
        }

	}
}