using ChessBot.Engine;
using ChessBot.Helpers;


namespace ChessBot.Engine;
public struct Fen {

	public const string startpos = "rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1";
	public readonly bool isStartPos = false;

	public Move moveMade; // Move made on current state, current state + moveMade yields new FEN string

	public string fenBoard;
	public char fenColor;
	public int castlePrivsBin;

	public string castlePrivs {
		get {
			string o = "";
			if ((castlePrivsBin & whiteKingCastle)  == whiteKingCastle)  o += "K";
			if ((castlePrivsBin & whiteQueenCastle) == whiteQueenCastle) o += "Q";
			if ((castlePrivsBin & blackKingCastle)  == blackKingCastle)  o += "k";
			if ((castlePrivsBin & blackQueenCastle) == blackQueenCastle) o += "q";

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

	public Fen(string fenString) {
		if (fenString == startpos) isStartPos = true;
		String[] splitFenString = fenString.Split(' ');

		try {
			fenBoard = splitFenString[0];
			fenColor = splitFenString[1][0];

			castlePrivsBin = 0;
			foreach (char c in splitFenString[2]) {
				castlePrivsBin += CastleCharToEnum(c);
			}
			enpassantSquare = splitFenString[3];
			halfMoveCount = Int32.Parse(splitFenString[4]);
			fullMoveCount = Int32.Parse(splitFenString[5]);
			moveMade = new Move(0);
		} catch {
			throw new Exception("Invalid Fen string");
		}
	}
	public Fen(string fenString, Move move) : this(fenString) { this.moveMade = move; }

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


	public string ToFEN() {
		string colorToMove = this.fenColor.ToString();
		string halfMoveCount = $"{this.halfMoveCount}";
		string fullMoveCount = $"{this.fullMoveCount}";

		return $"{fenBoard} {colorToMove} {castlePrivs} {enpassantSquare} {halfMoveCount} {fullMoveCount}";
	}

	public override string ToString() {
		return ToFEN();
	}
}
