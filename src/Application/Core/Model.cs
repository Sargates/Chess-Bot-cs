using Raylib_cs;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application;
using static Zobrist;
public class Model {

	public View view;
	public Board board;
	public bool enforceColorToMove = true;
	public readonly string[] botMatchStartFens;

	public Piece[] oldBoard = new Piece[64];
	public void SetOldBoard() { oldBoard = board.board.ToArray(); }



	public string stockfishExeExt;

	public int humanColor = 0b00; // 0b10 for white, 0b01 for black, 0b11 for both

	public bool SuspendPlay = false;



	// Player System //////////////////////////////

	// Human, Computer, UCI
	public enum Gametype { HvH, HvC, HvU, CvC, CvU, UvU }
	public Gametype activeGameType = Gametype.HvH;
	public int gameIndex = 0;
	public ChessPlayer whitePlayer = new ChessPlayer();
	public ChessPlayer blackPlayer = new ChessPlayer();
	public ChessPlayer ActivePlayer => board.whiteToMove ? whitePlayer : blackPlayer;
	public ChessPlayer InactivePlayer => board.whiteToMove ? blackPlayer : whitePlayer;

	
	public Model(View view) {
		this.view = view;

		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			stockfishExeExt = "stockfish.exe";
		} else
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			stockfishExeExt = "stockfish";
		} else {
			throw new Exception("This program can only run on Windows and Linux");
		}
		// StartNewGame("rnbqkbnr/p1pppppp/8/1p6/P7/8/1PPPPPPP/RNBQKBNR w KQkq - 0 1");
		StartNewGame();
		Debug.Assert(board != null);
		AddButtons();

		botMatchStartFens = FileHelper.ReadResourceFile("Fens.txt").Split('\n');
	}

	public void AddButtons() {
		view.AddToPipeline(new Button(new Rectangle(40, 420, 210, 50), "Freeplay").SetLeftCallback(delegate {
			StartNewGame();
		}));
		view.AddToPipeline(new Button(new Rectangle(40, 480, 210, 50), "Human vs. Gatesfish").SetLeftCallback(delegate {
			StartNewGame(type:Gametype.HvC);
		}));
		view.AddToPipeline(new Button(new Rectangle(40, 540, 210, 50), "Human vs. Stockfish").SetLeftCallback(delegate {
			StartNewGame(type:Gametype.HvU);
		}));
		view.AddToPipeline(new Button(new Rectangle(40, 600, 210, 50), "Stockfish vs. Stockfish").SetLeftCallback(delegate {
			StartNewGame(type:Gametype.UvU);
		}));
		view.AddToPipeline(new Button(new Rectangle(40, 660, 210, 50), "Reset settings").SetLeftCallback(delegate {
			ApplicationSettings.ResetDefaultSettings();
		}));
		view.AddToPipeline(new Button(new Rectangle(View.screenSize.X-250, 300, 210, 50), "Test").SetLeftCallback(delegate {
			// ulong test = 0x2316476349473769;
			// while (test != 0) {
			// 	Console.WriteLine(test);
			// 	BitboardHelper.PopLSB(ref test);
			// }
			// Console.WriteLine("End");
		}));
		view.AddToPipeline(new Button(new Rectangle(View.screenSize.X-250, 360, 210, 50), "Perft Test").SetLeftCallback(delegate {
			Perft.GetDepth();
		}));
		view.AddToPipeline(new Button(new Rectangle(View.screenSize.X-250, 420, 210, 50), "Left/Right to Inc/Dec\nPerft depth").SetLeftCallback(delegate {
			Perft.maxDepth++;
		}).SetRightCallback(delegate {
			Perft.maxDepth--;
		}));
		view.AddToPipeline(new Button(new Rectangle(View.screenSize.X-250, 480, 210, 50), "Undo Test (Fast suite)").SetLeftCallback(delegate {
			UnmakeMoveHelper.Fast();
		}));
		view.AddToPipeline(new Button(new Rectangle(View.screenSize.X-250, 540, 210, 50), "Undo Test (Full suite)").SetLeftCallback(delegate {
			UnmakeMoveHelper.FullSuite();
		}));

		view.AddToPipeline(new Button(new Rectangle(View.screenSize.X-250, 600, 210, 50), "Print bitboards").SetLeftCallback(delegate {

			Console.WriteLine();
			Console.WriteLine("Starting print");
			string[] b = new string[64];
			foreach (Piece piece in Piece.pieceArray) {
				ulong bitboard = board.GetPieceBBoard(piece);
				for (int i=0;i<64;i++) {
					if (b[i] == "  " || b[i] == null) b[i] = "  ";
					if (1 == (1 & (bitboard >> i))) {
						b[i] = piece.ToString();
					}
				}
			}
			for (int i=7;i>-1;i--) {
				for (int j=0;j<8;j++) {
					int index = 8*i+j;
					Console.Write($"{b[index]} ");
				}
				Console.WriteLine();
			}
			Console.WriteLine("Finishing print");
		}));
	}


	// Below methods handle the changing state of the board
	public void MakeMoveOnBoard(Move move, bool animate=true) {
		bool opponentInCheck;
		if (! move.IsNull) {
			Gamestate newGamestate = new Gamestate();
			view.ui.DeselectActiveSquare();
			ActivePlayer.IsSearching = false;

			Piece pieceMoved = board.GetSquare(move.StartSquare);
			Piece pieceTaken = (move.Flag==Move.EnPassantCaptureFlag) ? (board.InactiveColor|Piece.Pawn) : board.GetSquare(move.TargetSquare);

			board.MakeMove(move);

			opponentInCheck = MoveGenerator.IsSquareAttacked(board, board.ActiveColor == Piece.White ? board.whiteKingPos : board.blackKingPos, board.ActiveColor);
			bool canOpponentRespond = MoveGenerator.GetAllMoves(board, board.ActiveColor).Length != 0; // Negated for readability

			if (board.currentStateNode.Previous == null) { throw new Exception("Something went wrong"); }

			board.currentStateNode.Previous.Value.soundPlayed = GetMoveSound(move, pieceTaken, opponentInCheck, canOpponentRespond);

			// Debug.Assert(currentBoard.currentStateNode.Previous != null);
			// currentBoard.currentStateNode.Previous.Value = currentBoard.currentState;

			if (animate) { // Sounds are built in to animations, if move is not animated, play sound manually
				view.ui.activeAnimations.AddRange(AnimationHelper.FromGamestate(board.currentStateNode.Previous.Value));
			} else {
				Raylib.PlaySound(BoardUI.sounds[board.currentStateNode.Previous.Value.soundPlayed]);
			}

			// If opponent can't respond, fallthrough to game end handling
			if (canOpponentRespond) {
				return;
			}
		} else { // If the move is null it's assumed it checkmate
			opponentInCheck = true;
			ConsoleHelper.WriteLine("Null move was made, assumed checkmate", ConsoleColor.DarkBlue);
		}


		SuspendPlay = true;

		if (opponentInCheck) { // Checkmate
			ConsoleHelper.WriteLine("Checkmate!", ConsoleColor.DarkBlue);
			ConsoleHelper.WriteLine($"Winner: {InactivePlayer.color}, Loser: {ActivePlayer.color}", ConsoleColor.DarkBlue);
		} else { // Stalemate
			ConsoleHelper.WriteLine("Stalemate", ConsoleColor.DarkBlue);
			ConsoleHelper.WriteLine($"Draw.", ConsoleColor.DarkBlue);
		}

		// Handle Checkmate
	}
	public int GetMoveSound(Move move, Piece pieceTaken, bool opponentInCheck, bool canOpponentRespond) {
			MoveSounds sound = MoveSounds.Move;
			if (pieceTaken != Piece.None) { sound = MoveSounds.Capture; } else
			if (move.Flag == Move.CastleFlag) { sound = MoveSounds.Castle; }

			if (opponentInCheck && canOpponentRespond) {
				sound = MoveSounds.Check;
			} else
			if (opponentInCheck && ! canOpponentRespond) {
				sound = MoveSounds.Checkmate; // Sound is played separately if game is over
			} else
			if (! opponentInCheck && ! canOpponentRespond) {
				sound = MoveSounds.Stalemate; // Sound is played separately if game is over
			}

			return (int) sound;
	}

	public void ExitPlayerThreads() { whitePlayer.RaiseExitFlag(); blackPlayer.RaiseExitFlag(); }
	public void JoinPlayerThreads() { whitePlayer.Join(); blackPlayer.Join(); }
	public void SetBoardPosition() { SetBoardPosition(Fen.startpos); }
	public void SetBoardPosition(string fenString) {
		// if (currentBoard != null) SetOldBoard();
		board = new Board(fenString);

		whitePlayer.UCI?.RaiseManualUpdateFlag();
		blackPlayer.UCI?.RaiseManualUpdateFlag();
	}
	public void SetPlayerTypes(Gametype type) {
		if (activeGameType != type) {
		}
		humanColor = 0b00;
		ExitPlayerThreads();
		// JoinPlayerThreads();
		whitePlayer = (type, gameIndex%2==1) switch {
			(Gametype.HvH, true) => new ChessPlayer(new Player('w'), 300f),
			(Gametype.HvC, true) => new ChessPlayer(new Player('w'), 300f),
			(Gametype.HvU, true) => new ChessPlayer(new Player('w'), 300f),
			(Gametype.CvC, true) => new ChessPlayer(new ComputerPlayer('w', this), 60f),
			(Gametype.CvU, true) => new ChessPlayer(new ComputerPlayer('w', this), 60f),
			(Gametype.UvU, true) => new ChessPlayer(new UCIPlayer('w', this, stockfishExeExt), 30f),
			(Gametype.HvH, false) => new ChessPlayer(new Player('w'), 300f),
			(Gametype.HvC, false) => new ChessPlayer(new ComputerPlayer('w', this), 60f),
			(Gametype.HvU, false) => new ChessPlayer(new UCIPlayer('w', this, stockfishExeExt), 30f),
			(Gametype.CvC, false) => new ChessPlayer(new ComputerPlayer('w', this), 60f),
			(Gametype.CvU, false) => new ChessPlayer(new UCIPlayer('w', this, stockfishExeExt), 30f),
			(Gametype.UvU, false) => new ChessPlayer(new UCIPlayer('w', this, stockfishExeExt), 30f),
			_ => throw new Exception("Shut up compiler!")
		};
		blackPlayer = (type, gameIndex%2==1) switch {
			(Gametype.HvH, true) => new ChessPlayer(new Player('b'), 300f),
			(Gametype.HvC, true) => new ChessPlayer(new ComputerPlayer('b', this), 60f),
			(Gametype.HvU, true) => new ChessPlayer(new UCIPlayer('b', this, stockfishExeExt), 30f),
			(Gametype.CvC, true) => new ChessPlayer(new ComputerPlayer('b', this), 60f),
			(Gametype.CvU, true) => new ChessPlayer(new UCIPlayer('b', this, stockfishExeExt), 30f),
			(Gametype.UvU, true) => new ChessPlayer(new UCIPlayer('b', this, stockfishExeExt), 30f),
			(Gametype.HvH, false) => new ChessPlayer(new Player('b'), 300f),
			(Gametype.HvC, false) => new ChessPlayer(new Player('b'), 300f),
			(Gametype.HvU, false) => new ChessPlayer(new Player('b'), 300f),
			(Gametype.CvC, false) => new ChessPlayer(new ComputerPlayer('b', this), 60f),
			(Gametype.CvU, false) => new ChessPlayer(new ComputerPlayer('b', this), 60f),
			(Gametype.UvU, false) => new ChessPlayer(new UCIPlayer('b', this, stockfishExeExt), 30f),
			_ => throw new Exception("Shut up compiler!")
		};
		activeGameType = type;

		
		whitePlayer.StartThread();
		blackPlayer.StartThread();
		humanColor |= (whitePlayer.Computer == null && whitePlayer.Player != null) ? 0b10 : 0b00;
		humanColor |= (blackPlayer.Computer == null && blackPlayer.Player != null) ? 0b01 : 0b00;
		// ConsoleHelper.WriteLine($"{System.Convert.ToString(humanColor, 2)}", ConsoleColor.DarkYellow);

		ConsoleHelper.WriteLine($"White: {whitePlayer}", ConsoleColor.DarkYellow);
		ConsoleHelper.WriteLine($"Black: {blackPlayer}", ConsoleColor.DarkYellow);
	}
	public void StartNewGame(Gametype type) { StartNewGame(Fen.startpos, type); }
	public void StartNewGame(string fenString=Fen.startpos, Gametype type=Gametype.HvH) {
		//* 
		//* Instantiate starting gamestate
		//* Instantiate new Board passing starting gamestate
		//* Recalc bitboards
		//* 

		SetBoardPosition(fenString);
		ConsoleHelper.WriteLine($"\nGame number {gameIndex} started\nFEN: {fenString}", ConsoleColor.DarkYellow);
		SetPlayerTypes(type);
		view.ui.boardToRender = board.board;
		view.ui.IsFlipped = humanColor == 0b01; // if white is not a player and black is a player
		gameIndex++;
		view.ui.activeAnimations.AddRange(AnimationHelper.FromBoardChange(oldBoard, board.board, 0.2f));
		SuspendPlay = false;
	}
	public void AnimateSingleRewind() {
		if (board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get first previous state, is null"); return; }

		board.SetPrevState();
		view.ui.boardToRender = board.board;
		whitePlayer.UCI?.RaiseManualUpdateFlag();
		blackPlayer.UCI?.RaiseManualUpdateFlag();

		view.ui.activeAnimations.AddRange(AnimationHelper.ReverseFromGamestate(board.currentState));
	}
	public void AnimateSingleForward() {
		if (board.currentStateNode.Next == null) { Console.WriteLine("Cannot get first next state, is null"); return; }

		view.ui.activeAnimations.AddRange(AnimationHelper.FromGamestate(board.currentState));
		board.SetNextState();
		view.ui.boardToRender = board.board;
		whitePlayer.UCI?.RaiseManualUpdateFlag();
		blackPlayer.UCI?.RaiseManualUpdateFlag();

	}
	public void AnimatedDoubleRewind() {
		if (board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get first previous state, is null"); return; }
		board.SetPrevState();
		view.ui.activeAnimations.AddRange(AnimationHelper.ReverseFromGamestate(board.currentState));

		// Just mimics Single____State twice, but adding a lag to the second animation
		if (board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get second previous state, is null"); return; }
		board.SetPrevState();
		view.ui.activeAnimations.AddRange(AnimationHelper.ReverseFromGamestate(board.currentState, lag:-0.04f));
	}
	public void AnimateDoubleForward() {
		if (board.currentStateNode.Next == null) { Console.WriteLine("Cannot get first next state, is null"); return; }
		view.ui.activeAnimations.AddRange(AnimationHelper.FromGamestate(board.currentState));
		board.SetNextState();

		// Just mimics Single____State twice, but adding a lag to the second animation
		if (board.currentStateNode.Next == null) { Console.WriteLine("Cannot get second next state, is null"); return; }
		view.ui.activeAnimations.AddRange(AnimationHelper.FromGamestate(board.currentState, lag:-0.04f));
		board.SetNextState();
	}
}
