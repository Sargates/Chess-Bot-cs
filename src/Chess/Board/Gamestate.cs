using ChessBot.Engine;
using ChessBot.Helpers;

public class Gamestate {

	private static Stack<Gamestate> history = new Stack<Gamestate>();

	public Board board { get; }
	public Move? move { get; }
	public string boardRepr { get; }
	public char colorToMove { get; }
	public string castles { get; }
	public string enpassantSquare { get; }
	public int halfMoveCount { get; }
	public int fullMoveCount { get; }

	public const int whiteKingCastle  = 0b1000;
	public const int whiteQueenCastle = 0b0100;
	public const int blackKingCastle  = 0b0010;
	public const int blackQueenCastle = 0b0001;



	private Gamestate(Board board, Move recentMove) { // Should be assembled after moving and changing `whiteToMove`
		//	Assemble FEN notation
		//	Example:
		//	rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1
		this.board = board;
		this.move = recentMove;

		/*
		This class is just going to be a FEN class, it's called Gamestate but it's 
		just going to be a class that holds the fen string of the current board state
		No dealing with bullshit like when to instantiate the gamestate
		Before the move is made on the board, the current gamestate will be given the move that is to be made
		Call `currentGamestate.SetMove()` before making the move, update the gamestate within each call of `MovePiece()`
		The number of gamestates will be ordered like this:
		StateHistory	0 1 2 3 4 5 6 7 8 9  10
						 / / / / / / / / /  /
		MoveHistory		1 2 3 4 5 6 7 8 9 10

		0th gamestate is the initial gamestate, in Controller.cs or Model.cs: StartNewGame clears the FEN history (logging to file is optional)
		and instantiates the board with the given gamestate.
		1st move is the move made on the 0th gamestate, i.e making the 1st Move on the 0th Gamestate will yield the 1st Gamestate

		I am going to bed
		*/


		
		// boardRepr = GenBoardRepr(board);
		// colorToMove = !board.whiteToMove ? 'w' : 'b';
		
		// //	Handle Castles
		// int allCastles = 0;
		// foreach (char c in history.Peek().castles.ToCharArray()) {
		// 	allCastles += FENCastleToEnum(c);
		// }



		Gamestate.PushHistory(this);
	}

	public static int FENCastleToEnum(char castle) {
		return castle switch {
			'W' => whiteKingCastle,
			'B' => whiteQueenCastle,
			'w' => blackKingCastle,
			'b' => blackQueenCastle,
			_ => throw new Exception("Shut up compiler!")
		};
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