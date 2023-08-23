using System.Diagnostics;
using ChessBot.Application;
using ChessBot.Helpers;

namespace ChessBot.Engine;

public class Board {

	public Piece[] board;


	// Color Info /////////////////////////////////
	public int ActiveColor => whiteToMove ? Piece.White : Piece.Black;
	public int InactiveColor => whiteToMove ? Piece.Black : Piece.White;
	public int forwardDir(int color) => color == Piece.White ? 8 : -8;
	public bool whiteToMove;

	public int enPassantIndex;

	public int whiteKingPos;
	public int blackKingPos;



	// State System ///////////////////////////////
	public LinkedList<Gamestate> stateHistory;
	public LinkedListNode<Gamestate> currentStateNode;
	public Gamestate currentState {
		get {
			return currentStateNode.Value;
		}
	}

	public Board(string fenString=Fen.startpos) {
		Fen fen = new Fen(fenString);

		stateHistory = new LinkedList<Gamestate>();
		currentStateNode = new LinkedListNode<Gamestate>(new Gamestate());
		stateHistory.AddLast(currentStateNode);
		currentState.ID = 0;


		// TODO: Add FEN string loading, StartNewGame should be in Controller/cs, board should just be able to load a fen string in place
		BoardHelper.UpdateFromState(this, fen);
		// board = FenToBoard(fenString);
		Debug.Assert(board!=null);
		// board = FenToBoard(this.currentFen.fenBoard);
		for (int i = 0; i < board.Length; i++) {
			if (board[i] == (Piece.WhiteKing)) { whiteKingPos = i; }
			if (board[i] == (Piece.BlackKing)) { blackKingPos = i; }
		}
	}


	// public string GetUCIGameFormat() {

	// 	LinkedListNode<Gamestate>? currNode = stateHistory.First;
	// 	string o = "";
	// 	if (currNode is null) {
	// 		Console.WriteLine("`GetUCIGameFormat` returned default");
	// 		return "position startpos";
	// 	}

	// 	if (currNode.Value.isStartPos) {
	// 		o += $"position startpos ";
	// 	} else if (! currNode.Value.isStartPos) {
	// 		o += $"position fen {currNode.Value.ToFEN()} ";
	// 	}
	// 	if (! currNode.Value.moveMade.IsNull) {
	// 		o += $"moves {currNode.Value.moveMade}";
	// 	}
	// 	currNode = currNode.Next;

	// 	while (currNode != null) {
	// 		if (currNode.Value.moveMade.IsNull) break;

	// 		o += $" {currNode.Value.moveMade}";
	// 		currNode = currNode.Next;
	// 	}

	// 	return o;
	// }

	public void MakeMove(Move move, bool quiet=false) { //* Wrapper method for MovePiece, calls MovePiece and handles things like board history, 50 move rule, 3 move repition, 
		int movedFrom = move.StartSquare;
		int movedTo = move.TargetSquare;
		int moveFlag = move.Flag;


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

		int castlesToKeep = 0b1111;
		// If piece moved is a king or a rook, update castle perms
		if (pieceMoved.Type == Piece.King) {
			if (pieceMoved.Color == Piece.White) {
				castlesToKeep -= Fen.whiteKingCastle;
				castlesToKeep -= Fen.whiteQueenCastle;
			}
			if (pieceMoved.Color == Piece.Black) {
				castlesToKeep -= Fen.blackKingCastle;
				castlesToKeep -= Fen.blackQueenCastle;
			}
		}
		if (pieceMoved.Type == Piece.Rook) {
			if (pieceMoved.Color == Piece.White) {
				if (movedFrom == BoardHelper.a1) {
					castlesToKeep -= Fen.whiteQueenCastle;
				}
				if (movedFrom == BoardHelper.h1) {
					castlesToKeep -= Fen.whiteKingCastle;
				}
			}
			if (pieceMoved.Color == Piece.Black) {
				if (movedFrom == BoardHelper.a8) {
					castlesToKeep -= Fen.blackQueenCastle;
				}
				if (movedFrom == BoardHelper.h8) {
					castlesToKeep -= Fen.blackKingCastle;
				}
			}
		}
		
		whiteToMove = !whiteToMove; // ForwardDir / anything related to the active color will be the same up until this point


		if (! quiet) {
			Gamestate newGamestate = new Gamestate();
			currentState.moveMade = move;
			currentState.pieceTaken = pieceTaken;
			currentState.pieceMoved = pieceMoved;
			newGamestate.enPassantIndex = enPassantIndex;
			newGamestate.ID = stateHistory.Count;
			newGamestate.castleRights = currentState.castleRights & castlesToKeep;
			PushNewState(newGamestate);
		}

		// if (quiet) { // TODO: Change whiteToMove behavior below
		// 	return;
		// }

		// if (pieceTaken != Piece.None) { sound = MoveSounds.Capture; } else
		// if (move.MoveFlag == Move.CastleFlag) { sound = MoveSounds.Castle; }


		// Fen temp = currentStateNode.Value;
		// temp.moveMade = move;
		// currentStateNode.Value = temp;

		// temp = new Fen(temp.ToFEN());

		// temp.castlePrivsBin &= castlesToRemove;
		// temp.enpassantSquare = (enPassantIndex==-1) ? "-" : BoardHelper.IndexToSquareName(enPassantIndex);
		// if (pieceTaken != Piece.None || pieceMoved.Type == Piece.Pawn) { temp.halfMoveCount = 0; }
		// else { temp.halfMoveCount += 1; }
		// if (tempWhiteToMove) temp.fullMoveCount += 1;

		// temp.fenColor = tempWhiteToMove ? 'w' : 'b';
		// String o = "";
		// int gap = 0;
		// for (int i=0; i<8;i++) {
		// 	for (int j=0; j<8; j++) {
		// 		int index = 8*(7-i)+j;
		// 		int pieceEnum = GetSquare(index);
		// 		if (pieceEnum == Piece.None) {
		// 			gap += 1;
		// 			continue;
		// 		} // Passes guard clause if square is not empty
		// 		if (gap != 0) { o += $"{gap}"; }
		// 		o += $"{BoardHelper.PieceEnumToFenChar(pieceEnum)}";
		// 		gap = 0;
		// 	}
		// 	if (gap != 0) { o += $"{gap}"; }
		// 	if (i!=7) {
		// 		o += '/';
		// 		gap = 0;
		// 	}
		// }
		// temp.fenBoard = o;

		// PushNewState(temp);
		
		// whiteToMove = tempWhiteToMove; // Need to change whiteToMove after pushing state to fix threading issues between two computer opponents
	}
	public void MovePiece(Piece piece, int movedFrom, int movedTo) {
		//* modify bitboards here

		if (board[movedFrom] == (Piece.WhiteKing)) { whiteKingPos = movedTo; }
		if (board[movedFrom] == (Piece.BlackKing)) { blackKingPos = movedTo; }
		board[movedTo] = board[movedFrom];
		board[movedFrom] = Piece.None;
	}
	public void UndoMove() { // Undoes move attached to previous board state, requires prelimiary check that current state is not beginning of history
		if (currentStateNode.Previous == null) { throw new Exception("Tried to get previous gamestate when undoing move but previous state is null"); }
		currentStateNode = currentStateNode.Previous;

		Move move = currentState.moveMade;

		int movedFrom = move.TargetSquare;
		int movedTo = move.StartSquare;
		int moveFlag = move.Flag;

		Piece pieceMoved = currentState.pieceMoved;
		Piece pieceTaken = currentState.pieceTaken;

		
		currentState.moveMade = move;
		currentState.pieceTaken = pieceTaken;
		currentState.pieceMoved = pieceMoved;
		MovePiece(pieceMoved, movedFrom, movedTo);

		if (currentState.pieceTaken != Piece.None && moveFlag != Move.EnPassantCaptureFlag) {
			board[movedFrom] = pieceTaken;
		}
		
		// If the move was an enpassant capture
		enPassantIndex = -1;
		if (moveFlag == Move.EnPassantCaptureFlag) {
			board[movedFrom - forwardDir(pieceMoved.Color)] = currentState.pieceTaken;
			enPassantIndex = currentState.enPassantIndex;
		}

		if (move.IsPromotion) {
			board[movedFrom] = 0;
			board[movedTo] = currentState.pieceMoved;
		}

		// If move is a castle, move rook
		if (moveFlag == Move.CastleFlag) {
			if (pieceMoved.Color == Piece.White) {
				switch (movedFrom) {
					case BoardHelper.c1:
						MovePiece(Piece.Rook, BoardHelper.d1, BoardHelper.a1);
						break;
					case BoardHelper.g1:
						MovePiece(Piece.Rook, BoardHelper.f1, BoardHelper.h1);
						break;
				}
			}
			if (pieceMoved.Color == Piece.Black) {
				switch (movedFrom) {
					case BoardHelper.c8:
						MovePiece(Piece.Rook, BoardHelper.d8, BoardHelper.a8);
						break;
					case BoardHelper.g8:
						MovePiece(Piece.Rook, BoardHelper.f8, BoardHelper.h8);
						break;
				}
			}
		}
		whiteToMove = !whiteToMove;

		// Current state already caches castle rights so there's no need to handlethem when undoing

		// int castlesToKeep = 0b1111;
		// // If piece moved is a king or a rook, update castle perms
		// if (pieceMoved.Type == Piece.King) {
		// 	if (pieceMoved.Color == Piece.White) {
		// 		castlesToKeep -= Fen.whiteKingCastle;
		// 		castlesToKeep -= Fen.whiteQueenCastle;
		// 	}
		// 	if (pieceMoved.Color == Piece.Black) {
		// 		castlesToKeep -= Fen.blackKingCastle;
		// 		castlesToKeep -= Fen.blackQueenCastle;
		// 	}
		// }
		// if (pieceMoved.Type == Piece.Rook) {
		// 	if (pieceMoved.Color == Piece.White) {
		// 		if (movedFrom == BoardHelper.a1) {
		// 			castlesToKeep -= Fen.whiteQueenCastle;
		// 		}
		// 		if (movedFrom == BoardHelper.h1) {
		// 			castlesToKeep -= Fen.whiteKingCastle;
		// 		}
		// 	}
		// 	if (pieceMoved.Color == Piece.Black) {
		// 		if (movedFrom == BoardHelper.a8) {
		// 			castlesToKeep -= Fen.blackQueenCastle;
		// 		}
		// 		if (movedFrom == BoardHelper.h8) {
		// 			castlesToKeep -= Fen.blackKingCastle;
		// 		}
		// 	}
		// }

	}
	public void PushNewState(Gamestate state) {
		if (stateHistory.Last == null) {
			throw new Exception("`stateHistory.Last` is null");
		}
		while (stateHistory.Last != currentStateNode) { stateHistory.RemoveLast(); }
		stateHistory.AddLast(state);
		currentStateNode = stateHistory.Last;
	}
	public void SetPrevState() {
		UndoMove();
	}
	public void SetNextState() {
		if ( currentStateNode.Next == null ) throw new Exception("Cannot set to next state, is null");
		MakeMove(currentState.moveMade, true);
		currentStateNode = currentStateNode.Next;
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

	
	public Piece GetSquare(int index) {
		if (! (0 <= index && index < 64) ) { throw new Exception("Board index out of bounds"); }
		return board[index];
	}
}