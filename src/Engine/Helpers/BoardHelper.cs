using ChessBot.Engine;

namespace ChessBot.Helpers;
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
	public static void UpdateFromState(Board board, Fen state) {
		board.board = new Piece[64];
		// Populate board with pieces according to FEN string
		Piece file = 0; Piece rank = 7;
		foreach (char c in state.fenBoard) {
			if (c == '/') { rank--; file = 0; continue; }
			Piece index = 8*rank+file;

			if (char.IsNumber(c)) {

				file += int.Parse($"{c}");
				continue;
			}

			Piece piece = BoardHelper.FenCharToPieceEnum(c);
			
			board.board[index] = piece;
			// BitboardHelper.SetSquare(ref board.piecesByType[piece.Type-1], index);
			// BitboardHelper.SetSquare(ref board.piecesByColor[piece.ColorAsBinary], index);
			BitboardHelper.SetSquare(ref board.GetPieceBBoard(piece), index);
			// Console.WriteLine($"{index}: {board.GetBBoardByType(piece.Type)}");
			
			file += 1;
		}

		board.whiteToMove = state.fenColor == 'w';
		board.currentState.enPassantIndex = BoardHelper.NameToSquareIndex(state.enpassantSquare);
		board.currentState.castleRights = state.castleRights;
	}
	public static Piece[] FenToBoard(string fen) {
		Piece[] board = new Piece[64];
		Piece file = 0; Piece rank = 7;
		foreach (char c in fen) {
			if (c == '/') {rank -= 1; file = 0; continue;}
			Piece index = 8*rank+file;

			if (char.IsNumber(c)) {

				file += int.Parse($"{c}");
				continue;
			}

			board[index] = BoardHelper.FenCharToPieceEnum(c);
			file += 1;
		}

		return board;
	}
	public static string BoardToFen(Board board) {
		string o = "";
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
				o += $"{BoardHelper.PieceEnumToFenChar(pieceEnum)}";
				gap = 0;
			}
			if (gap != 0) { o += $"{gap}"; }
			if (i!=7) {
				o += '/';
				gap = 0;
			}
		}
		return o;
	}
	public static Board GetBoardCopy(Board board) {
		return new Board(new Fen(board).ToFEN());
	}
	public static Coord GetAbsoluteDirection(int index) {
		return new Coord(index+26)-new Coord(26); // get relative delta from index that isnt on an edge, 26 is arbitrary
	}
	public static void PrintBoard(Piece[] board) {
		Console.WriteLine(" +---+---+---+---+---+---+---+---+");
		for(int i=7; i>-1; i--) {
			string line = "|";
			for(int j=7; j>-1; j--) {
				line += $" {BoardHelper.PieceEnumToFenChar(board[8*i+j])} |";
			}
			Console.WriteLine($" {line} {i+1}");
			Console.WriteLine(" +---+---+---+---+---+---+---+---+");
		}
		Console.WriteLine("   a   b   c   d   e   f   g   h");
	}

	public static void PrintGamestateHistory(Board board, int maxOut) {
		LinkedListNode<Gamestate>? currNode = board.stateHistory.First;
		if (board.stateHistory.Count > maxOut) {
			for (int i=0; i<board.stateHistory.Count-maxOut; i++) {
				if (currNode==null) break; // Never happens, compiler gives warning
				currNode = currNode.Next;
			}
		}

		Console.WriteLine();
		while (currNode != null) {
			if (currNode == board.currentStateNode) {
				ConsoleHelper.WriteLine($"{currNode.Value}", ConsoleColor.Red);
			} else
			if (currNode == board.currentStateNode.Previous || currNode == board.currentStateNode.Next) {
				ConsoleHelper.WriteLine($"{currNode.Value}", ConsoleColor.Yellow);
			} else {
				ConsoleHelper.WriteLine($"{currNode.Value}");
			}
			currNode = currNode.Next;
		}
	}



	
	public static int FenCharToPieceEnum(char pieceEnum) {
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

	public static char PieceEnumToFenChar(int pieceEnum) {
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
			_ => ' '
		};
	}
}
