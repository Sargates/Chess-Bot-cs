using System.Diagnostics;
using ChessBot.Application;
using ChessBot.Helpers;

namespace ChessBot.Engine;

public class Board {

	public Piece[] board;

	public bool whiteToMove;
	public int enPassantIndex;

	public int whiteKingPos;
	public int blackKingPos;

	public LinkedList<Fen> stateHistory;
	public LinkedListNode<Fen> currentStateNode;
	public Fen currentFen {
		get {
			return currentStateNode.Value;
		}
	}



	public Board(string fen="rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1") {
		if (fen == "") fen = Fen.startpos;

		stateHistory = new LinkedList<Fen>();
		currentStateNode = new LinkedListNode<Fen>(new Fen(fen));
		stateHistory.AddLast(currentStateNode);

		// TODO: Add FEN string loading, StartNewGame should be in Controller/cs, board should just be able to load a fen string in place
		UpdateFromState();
		Debug.Assert(board!=null);
		// board = FenToBoard(this.currentFen.fenBoard);
		for (int i = 0; i < board.Length; i++) {
			if (board[i] == (Piece.White | Piece.King)) { whiteKingPos = i; }
			if (board[i] == (Piece.Black | Piece.King)) { blackKingPos = i; }
		}
		whiteToMove = this.currentFen.fenColor == 'w';

	}


	public void UpdateFromState() {
		this.board = FenToBoard(currentFen.fenBoard);
		for (int i = 0; i < board.Length; i++) {
			if (board[i] == (Piece.White | Piece.King)) { whiteKingPos = i; }
			if (board[i] == (Piece.Black | Piece.King)) { blackKingPos = i; }
		}
		whiteToMove = currentFen.fenColor == 'w';
		enPassantIndex = BoardHelper.NameToSquareIndex(currentFen.enpassantSquare);
	}

	public void UpdateFromState(Fen state) {
		board = FenToBoard(state.fenBoard);
		for (int i = 0; i < board.Length; i++) {
			if (board[i] == (Piece.White | Piece.King)) { whiteKingPos = i; }
			if (board[i] == (Piece.Black | Piece.King)) { blackKingPos = i; }
		}
		whiteToMove = state.fenColor == 'w';
		enPassantIndex = BoardHelper.NameToSquareIndex(state.enpassantSquare);

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

	public int ActiveColor => whiteToMove ? Piece.White : Piece.Black;

	public int InactiveColor => whiteToMove ? Piece.Black : Piece.White;
	public int forwardDir(int color) => color == Piece.White ? 8 : -8;

	public string GetUCIGameFormat() {

		LinkedListNode<Fen>? currNode = stateHistory.First;
		string o = "";
		if (currNode is null) {
			Console.WriteLine("`GetUCIGameFormat` returned default");
			return "position startpos";
		}

		if (currNode.Value.isStartPos) {
			o += $"position startpos ";
		} else if (! currNode.Value.isStartPos) {
			o += $"position fen {currNode.Value.ToFEN()} ";
		}
		if (! currNode.Value.moveMade.IsNull) {
			o += $"moves {currNode.Value.moveMade}";
		}
		currNode = currNode.Next;

		while (currNode != null) {
			if (currNode.Value.moveMade.IsNull) break;

			o += $" {currNode.Value.moveMade}";
			currNode = currNode.Next;
		}

		return o;
	}

	public void MakeMove(Move move) { //* Wrapper method for MovePiece, calls MovePiece and handles things like board history, 50 move rule, 3 move repition, 
		int movedFrom = move.StartSquare;
		int movedTo = move.TargetSquare;
		int moveFlag = move.MoveFlag;

		Piece pieceMoved = GetSquare(movedFrom);
		Piece pieceTaken = (moveFlag==Move.EnPassantCaptureFlag) ? (InactiveColor|Piece.Pawn) : GetSquare(movedTo);

		MovePiece(pieceMoved, movedFrom, movedTo);

		// If the move was an enpassant capture
		if (moveFlag == Move.EnPassantCaptureFlag) {
			board[enPassantIndex - forwardDir(pieceMoved.Color)] = 0;
		}

		enPassantIndex = -1; // Ok to set this to -1 here because of how En-Passant works
		// Set Enpassant square
		if (moveFlag == Move.PawnTwoUpFlag) {
			enPassantIndex = movedFrom + forwardDir(pieceMoved.Color);
		}

		// Is a promotion
		if (moveFlag == Move.PromoteToQueenFlag) {
			board[movedTo] = pieceMoved.Color|Piece.Queen;
		}
		if (moveFlag == Move.PromoteToKnightFlag) {
			board[movedTo] = pieceMoved.Color|Piece.Knight;
		}
		if (moveFlag == Move.PromoteToRookFlag) {
			board[movedTo] = pieceMoved.Color|Piece.Rook;
		}
		if (moveFlag == Move.PromoteToBishopFlag) {
			board[movedTo] = pieceMoved.Color|Piece.Bishop;
		}

		// If move is a castle, move rook
		if (moveFlag == Move.CastleFlag) {
			if (pieceMoved.Color == Piece.White) {
				switch (movedTo) {
					case BoardHelper.c1:
						MovePiece(Piece.Rook, BoardHelper.a1, BoardHelper.d1);
						break;
					case BoardHelper.g1:
						MovePiece(Piece.Rook, BoardHelper.h1, BoardHelper.f1);
						break;
				}
			}
			if (pieceMoved.Color == Piece.Black) {
				switch (movedTo) {
					case BoardHelper.c8:
						MovePiece(Piece.Rook, BoardHelper.a8, BoardHelper.d8);
						break;
					case BoardHelper.g8:
						MovePiece(Piece.Rook, BoardHelper.h8, BoardHelper.f8);
						break;
				}
			}
		}

		int castlesToRemove = 0b1111;
		// If piece moved is a king or a rook, update castle perms
		if (pieceMoved.Type == Piece.King) {
			if (pieceMoved.Color == Piece.White) {
				castlesToRemove -= Fen.whiteKingCastle;
				castlesToRemove -= Fen.whiteQueenCastle;
			}
			if (pieceMoved.Color == Piece.Black) {
				castlesToRemove -= Fen.blackKingCastle;
				castlesToRemove -= Fen.blackQueenCastle;
			}
		}
		if (pieceMoved.Type == Piece.Rook) {
			if (pieceMoved.Color == Piece.White) {
				if (movedFrom == BoardHelper.a1) {
					castlesToRemove -= Fen.whiteQueenCastle;
				}
				if (movedFrom == BoardHelper.h1) {
					castlesToRemove -= Fen.whiteKingCastle;
				}
			}
			if (pieceMoved.Color == Piece.Black) {
				if (movedFrom == BoardHelper.a8) {
					castlesToRemove -= Fen.blackQueenCastle;
				}
				if (movedFrom == BoardHelper.h8) {
					castlesToRemove -= Fen.blackKingCastle;
				}
			}
		}

		bool tempWhiteToMove = !whiteToMove; // ForwardDir / anything related to the active color will be the same up until this point
		// if (quiet) { // TODO: Change whiteToMove behavior below
		// 	return;
		// }

		SoundStates sound = SoundStates.Move;
		if (pieceTaken != Piece.None) { sound = SoundStates.Capture; } else
		if (move.MoveFlag == Move.CastleFlag) { sound = SoundStates.Castle; }


		Fen temp = currentStateNode.Value;
		move.moveSoundEnum = (int)sound;
		temp.moveMade = move;
		currentStateNode.Value = temp;

		temp = new Fen(temp.ToFEN());

		temp.castlePrivsBin &= castlesToRemove;
		temp.enpassantSquare = (enPassantIndex==-1) ? "-" : BoardHelper.IndexToSquareName(enPassantIndex);
		if (pieceTaken != Piece.None || pieceMoved.Type == Piece.Pawn) { temp.halfMoveCount = 0; }
		else { temp.halfMoveCount += 1; }
		if (tempWhiteToMove) temp.fullMoveCount += 1;

		temp.fenColor = tempWhiteToMove ? 'w' : 'b';
		String o = "";
		int gap = 0;
		for (int i=0; i<8;i++) {
			for (int j=0; j<8; j++) {
				int index = 8*(7-i)+j;
				int pieceEnum = GetSquare(index);
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
		temp.fenBoard = o;

		PushNewState(temp);
		
		whiteToMove = tempWhiteToMove; // Need to change whiteToMove after pushing state to fix threading issues between two computer opponents
	}
	public void MovePiece(Piece piece, int movedFrom, int movedTo) {
		//* modify bitboards here

		if (board[movedFrom] == (Piece.White | Piece.King)) { whiteKingPos = movedTo; }
		if (board[movedFrom] == (Piece.Black | Piece.King)) { blackKingPos = movedTo; }
		board[movedTo] = board[movedFrom];
		board[movedFrom] = Piece.None;
	}

	// public void UnmakeMove(Move move) {
	// 	if (currentStateNode.Previous == null) {
	// 		throw new Exception("Tried to unmake move with no previous board state");
	// 	}
	// 	int movedFrom = move.TargetSquare;
	// 	int movedTo = move.StartSquare;
	// 	int moveFlag = move.MoveFlag;

	// 	Piece pieceMoved = GetSquare(movedFrom);

	// 	MovePiece(pieceMoved, movedFrom, movedTo);

	// 	enPassantIndex = -1;
	// 	if (moveFlag == Move.EnPassantCaptureFlag) { // Replace take piece and set en-passant index
	// 		board[movedFrom-forwardDir(ActiveColor)] = Piece.Pawn|ActiveColor;
	// 	}

	// 	if (moveFlag == Move.PawnTwoUpFlag) {

	// 	}

	// 	// Is a promotion
	// 	if (move.IsPromotion) {
	// 		board[movedTo] = Piece.Pawn|pieceMoved.Color;
	// 	}

	// 	currentStateNode = currentStateNode.Previous;
	// 	whiteToMove = ! whiteToMove;


	// }
	public void PushNewState(Fen newFen) {
		if (stateHistory.Last == null) {
			throw new Exception("`stateHistory.Last` is null");
		}
		while (stateHistory.Last != currentStateNode) { stateHistory.RemoveLast(); }
		stateHistory.AddLast(newFen);
		currentStateNode = stateHistory.Last;
	}

	
	public Piece GetSquare(int index) {
		if (! (0 <= index && index < 64) ) { throw new Exception("Board index out of bounds"); }
		return board[index];
	}
}