using ChessBot.Engine;
using ChessBot.Helpers;


namespace ChessBot.Engine {
	public struct Gamestate {

		private static Stack<Gamestate> history = new Stack<Gamestate>();

		public Move? move { get; private set; }
		public string boardRepr { get; private set; }
		public char colorToMove { get; private set; }
		public int castlePrivsBin { get; private set; }
		public string castlePrivs { get; private set; }
		public string enpassantSquare { get; private set; }
		public int halfMoveCount { get; private set; }
		public int fullMoveCount { get; private set; }

		public const int whiteKingCastle  = 0b1000;
		public const int whiteQueenCastle = 0b0100;
		public const int blackKingCastle  = 0b0010;
		public const int blackQueenCastle = 0b0001;



		// public Gamestate(Board board) { // Should be assembled after moving and changing `whiteToMove`
		// 	//* Assemble FEN notation
		// 	//* Example:
		// 	//* rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
		// 	// this.move = recentMove;

		// 	/*
		// 	* This class is just going to be a FEN class, it's called Gamestate but it's 
		// 	* just going to be a class that holds the fen string of the current board state
		// 	* No dealing with bullshit like when to instantiate the gamestate
		// 	* Before the move is made on the board, the current gamestate will be given the move that is to be made
		// 	* Call `currentGamestate.SetMove()` before making the move, update the gamestate within each call of `MovePiece()`
		// 	* The number of gamestates will be ordered like this:
		// 	* StateHistory	0 1 2 3 4 5 6 7 8 9  10
		// 	* 				 / / / / / / / / /  /
		// 	* MoveHistory	1 2 3 4 5 6 7 8 9 10
		// 	* 
		// 	* 0th gamestate is the initial gamestate, in Controller.cs or Model.cs: StartNewGame clears the FEN history (logging to file is optional)
		// 	* and instantiates the board with the given gamestate.
		// 	* 1st move is the move made on the 0th gamestate, i.e making the 1st Move on the 0th Gamestate will yield the 1st Gamestate
		// 	*
		// 	* I am going to bed
		// 	*/

		// 	if (history.Count == 0) throw new Exception("State history is empty, cannot carryover castling priviledges"); 



		// 	boardRepr = GenBoardRepr(board);
		// 	colorToMove = !board.whiteToMove ? 'w' : 'b';

		// 	castlePrivsBin = history.Peek().castlePrivsBin;


		// 	Gamestate.PushHistory(this);
		// }

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
				UpdateCastlePrivs();
				enpassantSquare = splitFenString[3];
				halfMoveCount = Int32.Parse(splitFenString[4]);
				fullMoveCount = Int32.Parse(splitFenString[5]);
			// } catch {
			// 	throw new Exception("Invalid Fen string");
			// }

			PushHistory(this);
		}

		public void SetRecentMove(Move recentMove) {
			this.move = recentMove;
		}

		public void RemoveCastle(int Enum) {
			castlePrivsBin &= ~Enum;
		}

		private void UpdateCastlePrivs() {
			castlePrivs = "";
			if ((castlePrivsBin & whiteKingCastle) == whiteKingCastle) castlePrivs += "K";
			if ((castlePrivsBin & whiteQueenCastle) == whiteQueenCastle) castlePrivs += "Q";
			if ((castlePrivsBin & blackKingCastle) == blackKingCastle) castlePrivs += "k";
			if ((castlePrivsBin & blackQueenCastle) == blackQueenCastle) castlePrivs += "q";
		}




		public static string GenBoardRepr(Board board) {
			
			String o = "";
			int gap = 0;
			for (int i=0; i<64;i++) {
				if (i%8==0 && i!=0) {
					if (gap != 0) {
						o += $"{gap}";
					}
					o += '/';
					gap = 0;
				}
				int pieceEnum = board.GetSquare(i);
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

			return o;
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

			return $"{boardRepr} {colorToMove} {castlePrivs} {enpassantSquare} {halfMoveCount} {fullMoveCount}";
		}

		public static void PushHistory(Gamestate i) {
			if (i.move == null)
			history.Push(i);
		}

		public static Gamestate PeekHistory() {
			return history.Peek();
		}

		public static Gamestate PopHistory() {
			return history.Pop();
		}
	}
}