using ChessBot.Engine;
using ChessBot.Helpers;


namespace ChessBot.Engine;
public struct Fen {

	public const string startpos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";


	public string fenBoard;
	public char fenColor;
	public int castleRights;

	public string castleRightsString {
		get {
			string o = "";
			if ((castleRights & whiteKingCastle)  == whiteKingCastle)  o += "K";
			if ((castleRights & whiteQueenCastle) == whiteQueenCastle) o += "Q";
			if ((castleRights & blackKingCastle)  == blackKingCastle)  o += "k";
			if ((castleRights & blackQueenCastle) == blackQueenCastle) o += "q";

			return (o == "") ? "-" : o;
		}
	}

	public string enpassantSquare;
	public int halfMoveCount;
	public int fullMoveCount;

	public const int whiteKingCastle  = 0b1000;
	public const int whiteQueenCastle = 0b0100;
	public const int blackKingCastle  = 0b0010;
	public const int blackQueenCastle = 0b0001;

	public Fen(string fenString=startpos) {
		String[] splitFenString = fenString.Split(' ');

		try {
			fenBoard = splitFenString[0];
			fenColor = splitFenString[1][0];

			castleRights = 0;
			foreach (char c in splitFenString[2]) {
				castleRights += CastleCharToEnum(c);
			}
			enpassantSquare = splitFenString[3];
			halfMoveCount = Int32.Parse(splitFenString[4]);
			fullMoveCount = Int32.Parse(splitFenString[5]);
		} catch {
			throw new Exception("Invalid Fen string");
		}
	}
	public Fen(Board board) {
		fenBoard = BoardHelper.BoardToFenBoard(board);
		fenColor = board.ActiveColor==Piece.White ? 'w' : 'b';

		castleRights = board.currentState.castleRights;
		enpassantSquare = "-";
		if (board.currentState.enPassantIndex != -1) {
			enpassantSquare = BoardHelper.IndexToSquareName(board.currentState.enPassantIndex);
		}
		halfMoveCount = board.currentState.halfMoveCount;
		fullMoveCount = board.currentState.fullMoveCount;
	}

	public static int CastleCharToEnum(char castle) {
		return castle switch {
			'K' => whiteKingCastle,
			'Q' => whiteQueenCastle,
			'k' => blackKingCastle,
			'q' => blackQueenCastle,
			'-' => 0,
			_ => throw new Exception("Invalid castle priviledge character for castling priviledges")
		};
	}

	public static int CastleEnumToChar(int Enum) {
		return Enum switch {
			whiteKingCastle  => 'K',
			whiteQueenCastle => 'Q',
			blackKingCastle  => 'k',
			blackQueenCastle => 'q',
			0 => '-',
			_ => throw new Exception("Invalid castle enum character for castling priviledges")
		};
	}

	public override string ToString() {
		string colorToMove = this.fenColor.ToString();
		string halfMoveCount = $"{this.halfMoveCount}";
		string fullMoveCount = $"{this.fullMoveCount}";

		return $"{fenBoard} {colorToMove} {castleRightsString} {enpassantSquare} {halfMoveCount} {fullMoveCount}";
	}
}
