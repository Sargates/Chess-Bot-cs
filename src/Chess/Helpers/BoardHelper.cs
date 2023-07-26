namespace ChessBot.Engine {	
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
			char file = square[0];
			char rank = square[1];
			return 8*rankNames.IndexOf(rank) + fileNames.IndexOf(file);

		}
	}
}