using System.Diagnostics;
using ChessBot.Application;
using ChessBot.Helpers;

namespace ChessBot.Engine;

public class Board {


	public ulong[,] pieces = new ulong[,] { 
		{ 0ul, 0ul },	// pawns
		{ 0ul, 0ul },	// knights
		{ 0ul, 0ul },	// bishops
		{ 0ul, 0ul },	// rooks
		{ 0ul, 0ul },	// queens
		{ 0ul, 0ul }	// kings
	};

	public ref ulong GetPieceBBoard(Piece piece) { return ref pieces[piece.Type-1, piece.ColorAsBinary]; } // Minus 1 because We don't keep track of Piece.None types

	// Color Info /////////////////////////////////
	public int ActiveColor => whiteToMove ? Piece.White : Piece.Black;
	public int InactiveColor => whiteToMove ? Piece.Black : Piece.White;
	public int forwardDir(int color) => color == Piece.White ? 8 : -8;
	public bool whiteToMove;

	public List<int> GetPiecePositions(Piece piece) {
		List<int> o = new List<int>();
		ulong pieceLocations = GetPieceBBoard(piece);
		
		while (pieceLocations != 0) {
			int exp = BitboardHelper.PopLSB(ref pieceLocations);
			o.Add(exp);
		}
		// Console.WriteLine($"{pieceLocations}, {string.Join(", ", o)}");
		return o;
	}
	// public List<int> GetPiecePositions(Piece piece) => piecePositions[piece.ColorAsBinary][piece.Type-1]; // Minus 1 because We don't keep track of Piece.None types


	public int whiteKingPos {
		get {
			ulong bitboard = GetPieceBBoard(Piece.WhiteKing);
			if (bitboard == 0) {
				BoardHelper.PrintBoard(this);
				throw new Exception("No white king on the board");
			}
			int o = BitboardHelper.PopLSB(ref bitboard);
			return o;
		}
	}
	public int blackKingPos {
		get {
			ulong bitboard = GetPieceBBoard(Piece.BlackKing);
			if (bitboard == 0) {
				BoardHelper.PrintBoard(this);
				throw new Exception("No black king on the board");
			}
			int o = BitboardHelper.PopLSB(ref bitboard);
			return o;
		}
	}


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
		// Debug.Assert(board!=null);
		// board = FenToBoard(this.currentFen.fenBoard);
	}


	public string GetMoveHistory() {

		LinkedListNode<Gamestate>? currNode = stateHistory.First;
		if (currNode == null) {
			Console.WriteLine("`GetUCIGameFormat` returned default");
			return "";
		}

		List<Move> history = new List<Move>();

		while (currNode != null) {
			if (currNode.Value.moveMade.IsNull) break;
			history.Add(currNode.Value.moveMade);
			currNode = currNode.Next;
		}

		return string.Join(" ", history);
	}

	public void MakeMove(Move move, bool quiet=false) { //* Wrapper method for MovePiece, calls MovePiece and handles things like board history, 50 move rule, 3 move repition, 
		int movedFrom = move.StartSquare;
		int movedTo = move.TargetSquare;
		int moveFlag = move.Flag;


		Piece pieceMoved = GetSquare(movedFrom);
		Piece pieceTaken = (moveFlag==Move.EnPassantCaptureFlag) ? (InactiveColor|Piece.Pawn) : GetSquare(movedTo);

		if (pieceTaken.Type == Piece.King) { throw new Exception("King was taken"); }


		MovePiece(pieceMoved, movedFrom, movedTo);
		// Console.WriteLine($"{pieceMoved}, {pieceTaken}");

		if (pieceTaken != Piece.None && moveFlag != Move.EnPassantCaptureFlag) {
			BitboardHelper.ClearSquare(ref GetPieceBBoard(pieceTaken), movedTo);
		}

		// If the move was an enpassant capture
		if (moveFlag == Move.EnPassantCaptureFlag) {
			int captureIndex = currentState.enPassantIndex - forwardDir(pieceMoved.Color);

			BitboardHelper.ClearSquare(ref GetPieceBBoard(pieceTaken), captureIndex);

			// board[captureIndex] = 0;
		}

		int enPassantIndex = -1; // Ok to set this to -1 here because of how En-Passant works
		// Set Enpassant square
		if (moveFlag == Move.PawnTwoUpFlag) {
			enPassantIndex = movedFrom + forwardDir(pieceMoved.Color);
		}

		// Is a promotion
		if (move.IsPromotion) {
			Piece promotedTo = (moveFlag) switch {
				Move.PromoteToQueenFlag => pieceMoved.Color|Piece.Queen,
				Move.PromoteToKnightFlag => pieceMoved.Color|Piece.Knight,
				Move.PromoteToRookFlag => pieceMoved.Color|Piece.Rook,
				Move.PromoteToBishopFlag => pieceMoved.Color|Piece.Bishop,
				_ => throw new Exception("Something went wrong")
			};
			// board[movedTo] = promotedTo;

			BitboardHelper.ClearSquare(ref GetPieceBBoard(pieceMoved), movedTo);

			BitboardHelper.SetSquare(ref GetPieceBBoard(promotedTo), movedTo);
		}

		// If move is a castle, move rook
		if (moveFlag == Move.CastleFlag) {
			if (pieceMoved.Color == Piece.White) {
				switch (movedTo) {
					case BoardHelper.c1:
						MovePiece(Piece.WhiteRook, BoardHelper.a1, BoardHelper.d1);
						break;
					case BoardHelper.g1:
						MovePiece(Piece.WhiteRook, BoardHelper.h1, BoardHelper.f1);
						break;
				}
			}
			if (pieceMoved.Color == Piece.Black) {
				switch (movedTo) {
					case BoardHelper.c8:
						MovePiece(Piece.BlackRook, BoardHelper.a8, BoardHelper.d8);
						break;
					case BoardHelper.g8:
						MovePiece(Piece.BlackRook, BoardHelper.h8, BoardHelper.f8);
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
		if (pieceTaken.Type == Piece.Rook) {
			if (movedTo == BoardHelper.a1) {
				castlesToKeep -= Fen.whiteQueenCastle;
			}
			if (movedTo == BoardHelper.h1) {
				castlesToKeep -= Fen.whiteKingCastle;
			}
			if (movedTo == BoardHelper.a8) {
				castlesToKeep -= Fen.blackQueenCastle;
			}
			if (movedTo == BoardHelper.h8) {
				castlesToKeep -= Fen.blackKingCastle;
			}
		}
		
		whiteToMove = !whiteToMove; // ForwardDir / anything related to the active color will be the same up until this point


		if (! quiet) {
			Gamestate newGamestate = new Gamestate();
			currentState.moveMade = move;
			currentState.pieceTaken = pieceTaken;
			currentState.pieceMoved = pieceMoved;
			if (pieceTaken == Piece.None && pieceMoved.Type != Piece.Pawn) { newGamestate.halfMoveCount = currentState.halfMoveCount+1; }
			newGamestate.fullMoveCount = currentState.fullMoveCount+1;
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

		// temp = new Fen(temp.ToString());

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
		BitboardHelper.ToggleSquares(ref GetPieceBBoard(piece), movedFrom, movedTo);


		// board[movedTo] = board[movedFrom];
		// board[movedFrom] = Piece.None;
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

		MovePiece(pieceMoved, movedFrom, movedTo);


		
		// Console.WriteLine(Convert.ToString((long)GetPieceBBoard(pieceMoved), 2));
		if (move.IsPromotion) {
			// Console.WriteLine("Here 3");
			Piece promotedTo = (moveFlag) switch {
				Move.PromoteToQueenFlag => pieceMoved.Color|Piece.Queen,
				Move.PromoteToKnightFlag => pieceMoved.Color|Piece.Knight,
				Move.PromoteToRookFlag => pieceMoved.Color|Piece.Rook,
				Move.PromoteToBishopFlag => pieceMoved.Color|Piece.Bishop,
				_ => throw new Exception("Something went wrong")
			};
			BitboardHelper.ClearSquare(ref GetPieceBBoard(promotedTo), movedFrom);
			BitboardHelper.ClearSquare(ref GetPieceBBoard(pieceMoved), movedFrom); // Needed because pawn isn't on square when move is undone
			// board[movedTo] = currentState.pieceMoved;
		}
		if (currentState.pieceTaken != Piece.None && moveFlag != Move.EnPassantCaptureFlag) {
			// Console.WriteLine("Here 1");

			BitboardHelper.SetSquare(ref GetPieceBBoard(pieceTaken), movedFrom);
			// board[movedFrom] = pieceTaken;
			// Console.WriteLine($"{pieceTaken}, {board[movedFrom]}");
		}
		// If the move was an enpassant capture
		if (moveFlag == Move.EnPassantCaptureFlag) {
			// Console.WriteLine("Here 2");
			int captureIndex = movedFrom - forwardDir(pieceMoved.Color);
			BitboardHelper.SetSquare(ref GetPieceBBoard(pieceTaken), captureIndex);

			// board[captureIndex] = currentState.pieceTaken;
		}


		// If move is a castle, move rook
		if (moveFlag == Move.CastleFlag) {
			// Console.WriteLine("Here 4");
			if (pieceMoved.Color == Piece.White) {
				switch (movedFrom) {
					case BoardHelper.c1:
						MovePiece(Piece.WhiteRook, BoardHelper.d1, BoardHelper.a1);
						break;
					case BoardHelper.g1:
						MovePiece(Piece.WhiteRook, BoardHelper.f1, BoardHelper.h1);
						break;
				}
			}
			if (pieceMoved.Color == Piece.Black) {
				switch (movedFrom) {
					case BoardHelper.c8:
						MovePiece(Piece.BlackRook, BoardHelper.d8, BoardHelper.a8);
						break;
					case BoardHelper.g8:
						MovePiece(Piece.BlackRook, BoardHelper.f8, BoardHelper.h8);
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
		Piece pieceToDraw = Piece.None;
		if (BitboardHelper.IsSquareSet(pieces[Piece.Pawn-1, 0], index)) { pieceToDraw = Piece.WhitePawn; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.Knight-1, 0], index)) { pieceToDraw = Piece.WhiteKnight; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.Bishop-1, 0], index)) { pieceToDraw = Piece.WhiteBishop; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.Rook-1, 0], index)) { pieceToDraw = Piece.WhiteRook; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.Queen-1, 0], index)) { pieceToDraw = Piece.WhiteQueen; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.King-1, 0], index)) { pieceToDraw = Piece.WhiteKing; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.Pawn-1, 1], index)) { pieceToDraw = Piece.BlackPawn; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.Knight-1, 1], index)) { pieceToDraw = Piece.BlackKnight; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.Bishop-1, 1], index)) { pieceToDraw = Piece.BlackBishop; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.Rook-1, 1], index)) { pieceToDraw = Piece.BlackRook; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.Queen-1, 1], index)) { pieceToDraw = Piece.BlackQueen; } else
		if (BitboardHelper.IsSquareSet(pieces[Piece.King-1, 1], index)) { pieceToDraw = Piece.BlackKing; }
		return pieceToDraw;
	}
}