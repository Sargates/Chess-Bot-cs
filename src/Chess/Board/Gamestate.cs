using ChessBot.Engine;
using ChessBot.Helpers;

class Gamestate {

	private static Stack<Gamestate> history = new Stack<Gamestate>();

	public Board board { get; }
	public Move move { get; }
	public string boardRepr { get; }
	public char colorToMove { get; }
	public string castles { get; }
	public string enpassantSquare { get; }
	public int halfMoveCount { get; }
	public int fullMoveCount { get; }



	private Gamestate(Board board, Move recentMove) { // Should be assembled before moving piece
		//	Assemble FEN notation
		//	Example:
		//	rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
		this.board = board;
		this.move = recentMove;
		
		boardRepr = GenBoardRepr(board);
		colorToMove = board.whiteToMove ? 'w' : 'b';
		
		//	Handle Castles
		int allCastles = 0;
		foreach (char c in history.Peek().castles.ToCharArray()) {
			allCastles += c switch {
				'W' => 0b1000,
				'B' => 0b0100,
				'w' => 0b0010,
				'b' => 0b0001,
			};
		}

		// if (recentMove.StartSquare)



		Gamestate.PushHistory(this);
	}

	public static string GenBoardRepr(Board board) {
		char PieceToChar(int pieceEnum) {
			return pieceEnum switch {
				PieceHelper.Black | PieceHelper.Pawn => 'p',
				PieceHelper.Black | PieceHelper.Knight => 'n',
				PieceHelper.Black | PieceHelper.Bishop => 'b',
				PieceHelper.Black | PieceHelper.Rook => 'r',
				PieceHelper.Black | PieceHelper.Queen => 'q',
				PieceHelper.Black | PieceHelper.King => 'k',
				PieceHelper.White | PieceHelper.Pawn => 'P',
				PieceHelper.White | PieceHelper.Knight => 'N',
				PieceHelper.White | PieceHelper.Bishop => 'B',
				PieceHelper.White | PieceHelper.Rook => 'R',
				PieceHelper.White | PieceHelper.Queen => 'Q',
				PieceHelper.White | PieceHelper.King => 'K'
			};
		}
		
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
			o += $"{PieceToChar(pieceEnum)}";
			gap = 0;
		}

		return o;
	}

	public static string ToFEN(Board board, Move recentMove) {
		Gamestate temp = new Gamestate(board, recentMove);
		// manipulate temp to get fen notation

		return "";
	}

	public static void PushHistory(Gamestate i) {
		history.Push(i);
	}

	public static Gamestate PeekHistory() {
		return history.Peek();
	}

	public static Gamestate PopHistory() {
		return history.Pop();
	}
}