using ChessBot.Engine;
using ChessBot.Helpers;


namespace ChessBot.Engine {
	public struct Gamestate {

		private static Stack<Gamestate> history = new Stack<Gamestate>();

		public Move? move;
		public string boardRepr;
		public char colorToMove;
		public int castlePrivsBin;
		public string castlePrivs;
		public string enpassantSquare;
		public int halfMoveCount;
		public int fullMoveCount;

		public const int whiteKingCastle  = 0b1000;
		public const int whiteQueenCastle = 0b0100;
		public const int blackKingCastle  = 0b0010;
		public const int blackQueenCastle = 0b0001;

		public Gamestate(string fenString) { // Should be assembled after moving and changing `whiteToMove`
			String[] splitFenString = fenString.Split(' ');

			// Console.WriteLine(fenString);
			// Console.WriteLine(splitFenString);

			// try {
				boardRepr = splitFenString[0];
				colorToMove = splitFenString[1][0];
				castlePrivsBin = 0;
				castlePrivs = "";
				foreach (char c in splitFenString[2]) {
					castlePrivsBin += CastleCharToEnum(c);
				}
				castlePrivs = GetCastlePrivs();
				enpassantSquare = splitFenString[3];
				halfMoveCount = Int32.Parse(splitFenString[4]);
				fullMoveCount = Int32.Parse(splitFenString[5]);
			// } catch {
			// 	throw new Exception("Invalid Fen string");
			// }

			PushHistory();
		}

		public void SetRecentMove(Move recentMove) {
			this.move = recentMove;
		}

		public void RemoveCastle(int Enum) {
			castlePrivsBin &= ~Enum;
		}

		public string GetCastlePrivs() {
			string o = "";
			if ((castlePrivsBin & whiteKingCastle) == whiteKingCastle) o += "K";
			if ((castlePrivsBin & whiteQueenCastle) == whiteQueenCastle) o += "Q";
			if ((castlePrivsBin & blackKingCastle) == blackKingCastle) o += "k";
			if ((castlePrivsBin & blackQueenCastle) == blackQueenCastle) o += "q";

			return (o == "") ? "-" : o;
		}



		public void UpdateBoardRepr(Board board) {
			String o = "";
			int gap = 0;
			for (int i=0; i<8;i++) {
				for (int j=0; j<8; j++) {
					int index = 8*(7-i)+j;
					if (index%8==0 && index!=56) {
						if (gap != 0) {
							o += $"{gap}";
						}
						o += '/';
						gap = 0;
					}
					int pieceEnum = board.GetSquare(index);
					if (pieceEnum == PieceHelper.None) {
						gap += 1;
						continue;
					}
					if (gap != 0) {
						o += $"{gap}";
					}
					o += $"{BoardEnumToChar(pieceEnum)}";
					gap = 0;

				}
			}

			boardRepr = o;
		}

		public static int BoardCharToEnum(char pieceEnum) {
			return pieceEnum switch {
				'p' => PieceHelper.Black | PieceHelper.Pawn,
				'n' => PieceHelper.Black | PieceHelper.Knight,
				'b' => PieceHelper.Black | PieceHelper.Bishop,
				'r' => PieceHelper.Black | PieceHelper.Rook,
				'q' => PieceHelper.Black | PieceHelper.Queen,
				'k' => PieceHelper.Black | PieceHelper.King,
				'P' => PieceHelper.White | PieceHelper.Pawn,
				'N' => PieceHelper.White | PieceHelper.Knight,
				'B' => PieceHelper.White | PieceHelper.Bishop,
				'R' => PieceHelper.White | PieceHelper.Rook,
				'Q' => PieceHelper.White | PieceHelper.Queen,
				'K' => PieceHelper.White | PieceHelper.King,
				_ => throw new Exception("Invalid piece character for board representation")
			};
		}

		public static char BoardEnumToChar(int pieceEnum) {
			return pieceEnum switch {
				PieceHelper.Black | PieceHelper.Pawn   => 'p',
				PieceHelper.Black | PieceHelper.Knight => 'n',
				PieceHelper.Black | PieceHelper.Bishop => 'b',
				PieceHelper.Black | PieceHelper.Rook   => 'r',
				PieceHelper.Black | PieceHelper.Queen  => 'q',
				PieceHelper.Black | PieceHelper.King   => 'k',
				PieceHelper.White | PieceHelper.Pawn   => 'P',
				PieceHelper.White | PieceHelper.Knight => 'N',
				PieceHelper.White | PieceHelper.Bishop => 'B',
				PieceHelper.White | PieceHelper.Rook   => 'R',
				PieceHelper.White | PieceHelper.Queen  => 'Q',
				PieceHelper.White | PieceHelper.King   => 'K',
				_ => throw new Exception("Invalid piece enum for board representation")
			};
		}

		public static int CastleCharToEnum(char castle) {
			return castle switch {
				'K' => whiteKingCastle,
				'Q' => whiteQueenCastle,
				'k' => blackKingCastle,
				'q' => blackQueenCastle,
				'-' => 0,
				_ => throw new Exception("Invalid castle priviledge characater for castling priviledges")
			};
		}

		public static int CastleEnumToChar(int Enum) {
			return Enum switch {
				whiteKingCastle  => 'K',
				whiteQueenCastle => 'Q',
				blackKingCastle  => 'k',
				blackQueenCastle => 'q',
				0 => '-',
				_ => throw new Exception("Invalid castle enum characater for castling priviledges")
			};
		}


		public string ToFEN() {
			string colorToMove = this.colorToMove.ToString();
			string halfMoveCount = $"{this.halfMoveCount}";
			string fullMoveCount = $"{this.fullMoveCount}";

			return $"{boardRepr} {colorToMove} {GetCastlePrivs()} {enpassantSquare} {halfMoveCount} {fullMoveCount}";
		}

		public void PushHistory() {
			if (this.move == null)
			history.Push(this);
		}

		public static Gamestate PeekHistory() {
			return history.Peek();
		}

		public static Gamestate PopHistory() {
			return history.Pop();
		}
	}
}