using ChessBot.Engine;

namespace ChessBot.Helpers {
	public class BoardHelper {
		public const string fileNames = "abcdefgh";
		public const string rankNames = "12345678";

		public const int a1 = 0;
		public const int b1 = 1;
		public const int c1 = 2;
		public const int d1 = 3;
		public const int e1 = 4;
		public const int f1 = 5;
		public const int g1 = 6;
		public const int h1 = 7;

		public const int a8 = 56;
		public const int b8 = 57;
		public const int c8 = 58;
		public const int d8 = 59;
		public const int e8 = 60;
		public const int f8 = 61;
		public const int g8 = 62;
		public const int h8 = 63;


		public static int FileIndex(int index) {
			return index & 0b111;
		}
		public static int RankIndex(int index) {
			return index >> 3;
		}

		public static Coord NameToCoord(string square) {
			return new Coord(NameToSquareIndex(square));
		}

		public static int CoordToIndex(Coord coord) {
			return 8*coord.rank+coord.file;
		}

		public static string IndexToSquareName(int index) {
			return fileNames[FileIndex(index)]+""+rankNames[RankIndex(index)];
		}
		public static int NameToSquareIndex(string square) {
			if (square == "-") { return -1; }
			char file = square[0];
			char rank = square[1];
			return 8*rankNames.IndexOf(rank) + fileNames.IndexOf(file);

		}

		
		public static int BoardCharToEnum(char pieceEnum) {
			return pieceEnum switch {
				'p' => Piece.Black | Piece.Pawn,
				'n' => Piece.Black | Piece.Knight,
				'b' => Piece.Black | Piece.Bishop,
				'r' => Piece.Black | Piece.Rook,
				'q' => Piece.Black | Piece.Queen,
				'k' => Piece.Black | Piece.King,
				'P' => Piece.White | Piece.Pawn,
				'N' => Piece.White | Piece.Knight,
				'B' => Piece.White | Piece.Bishop,
				'R' => Piece.White | Piece.Rook,
				'Q' => Piece.White | Piece.Queen,
				'K' => Piece.White | Piece.King,
				_ => throw new Exception("Invalid piece character for board representation")
			};
		}

		public static char BoardEnumToChar(int pieceEnum) {
			return pieceEnum switch {
				Piece.Black | Piece.Pawn   => 'p',
				Piece.Black | Piece.Knight => 'n',
				Piece.Black | Piece.Bishop => 'b',
				Piece.Black | Piece.Rook   => 'r',
				Piece.Black | Piece.Queen  => 'q',
				Piece.Black | Piece.King   => 'k',
				Piece.White | Piece.Pawn   => 'P',
				Piece.White | Piece.Knight => 'N',
				Piece.White | Piece.Bishop => 'B',
				Piece.White | Piece.Rook   => 'R',
				Piece.White | Piece.Queen  => 'Q',
				Piece.White | Piece.King   => 'K',
				_ => throw new Exception("Invalid piece enum for board representation")
			};
		}

		
		public static void UpdateFenAttachedToBoard(Board board) {
			String o = "";
			int gap = 0;
			for (int i=0; i<8;i++) {
				for (int j=0; j<8; j++) {
					int index = 8*(7-i)+j;
					int pieceEnum = board.GetSquare(index);
					if (pieceEnum == Piece.None) {
						gap += 1;
						continue;
					} // Passes guard clause if square is not empty
					if (gap != 0) { o += $"{gap}"; }
					o += $"{BoardEnumToChar(pieceEnum)}";
					gap = 0;
				}
				if (gap != 0) { o += $"{gap}"; }
				if (i!=7) {
					o += '/';
					gap = 0;
				}
			}

			board.currentFen.fenBoard = o;
		}
	}
}