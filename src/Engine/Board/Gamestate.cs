using ChessBot.Engine;
using ChessBot.Helpers;


namespace ChessBot.Engine {
	public struct Gamestate {

		private Stack<Gamestate> history = new Stack<Gamestate>();
		private Stack<Gamestate> future = new Stack<Gamestate>();

		public Move? move;
		public string fenBoard;
		public char fenColor;
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
				fenBoard = splitFenString[0];
				fenColor = splitFenString[1][0];
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
					if (pieceEnum == Piece.None) {
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

			fenBoard = o;
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
			string colorToMove = this.fenColor.ToString();
			string halfMoveCount = $"{this.halfMoveCount}";
			string fullMoveCount = $"{this.fullMoveCount}";

			return $"{fenBoard} {colorToMove} {GetCastlePrivs()} {enpassantSquare} {halfMoveCount} {fullMoveCount}";
		}

		public override string ToString() {
			return ToFEN();
		}

		public Stack<Gamestate> GetHistory() {
			return history;
		}
		public Stack<Gamestate> GetFuture() {
			return future;
		}

		public void PushHistory() {
			// if (this.move == null)
			history.Push(this);
		}

		public Gamestate PeekHistory() {
			return history.Peek();
		}

		public Gamestate PopHistory() {
			return history.Pop();
		}

		public void PushFuture() {
			future.Push(this);
		}

		public Gamestate PeekFuture() {
			return future.Peek();
		}

		public Gamestate PopFuture() {
			return future.Pop();
		}
	}
}