using Raylib_cs;
using System.Diagnostics;
using System.Numerics;
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
	// public void SetOldBoard() { oldBoard = board.board.ToArray(); }



	public static string stockfishExeExt = "STOCKFISH_PATH_NOT_SET";

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
			Model.stockfishExeExt = "stockfish.exe";
		} else
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			Model.stockfishExeExt = "stockfish";
		} else {
			throw new Exception("This program can only run on Windows and Linux");
		}
		StartNewGame();
		// StartNewGame("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
		Debug.Assert(board != null);
		AddButtons();

		botMatchStartFens = FileHelper.ReadResourceFile("Fens.txt").Split('\n');
	}

	public void AddButtons() {
		view.AddToPipeline(new StackRenderer(new Vector2(40, 420), 
			new Button(new Rectangle(0, 0, 210, 50), "Freeplay").SetLeftCallback(delegate {
				StartNewGame();
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Human vs. Gatesfish").SetLeftCallback(delegate {
				StartNewGame(type:Gametype.HvC);
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Human vs. Stockfish").SetLeftCallback(delegate {
				StartNewGame(type:Gametype.HvU);
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Stockfish vs. Stockfish").SetLeftCallback(delegate {
				StartNewGame(type:Gametype.UvU);
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Reset settings").SetLeftCallback(delegate {
				ApplicationSettings.ResetDefaultSettings();
			}))
		);

		view.AddToPipeline(new StackRenderer(new Vector2(View.screenSize.X-250, 240), 
			new Button(new Rectangle(0, 0, 210, 50), "Test").SetLeftCallback(delegate {
				var kvPair = Perft.testedFens.ElementAt(new Random().Next(Perft.testedFens.Count));
				string fen = kvPair.Key;
				SetBoardPosition(fen);
				WaveFunctionCollapse.CalculateMoveDiscrepancy(new Fen(board).ToString(), int.Parse(Perft.testedFens[fen].Last().Key));
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Get discrepancy").SetLeftCallback(delegate {
				// Perft.TestSpecific(6);
				foreach (string fen in Perft.testedFens.Keys) {
					int depth = int.Parse(Perft.testedFens[fen].Last().Key);
					ConsoleHelper.WriteLine($"Testing Fen: {fen}", ConsoleColor.Magenta);
					WaveFunctionCollapse.CalculateMoveDiscrepancy(fen, depth);
				}
			}),
			// new Button(new Rectangle(0, 0, 210, 50), "Print RookMoves (d4)").SetLeftCallback(delegate {
				
			// }),
			new Button(new Rectangle(0, 0, 210, 50), "Perft Test (Single)").SetLeftCallback(delegate {
				var kvPair = Perft.testedFens.ElementAt(3);
				new Perft(kvPair.Key, int.Parse(kvPair.Value.Last().Key));
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Perft Test (Full)").SetLeftCallback(delegate {
				foreach (var kvPair in Perft.testedFens) {
					new Perft(kvPair.Key, int.Parse(kvPair.Value.Last().Key));
				}
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Undo Test (Fast suite)").SetLeftCallback(delegate {
				UnmakeMoveHelper.Fast();
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Undo Test (Full suite)").SetLeftCallback(delegate {
				UnmakeMoveHelper.FullSuite();
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Print FEN").SetLeftCallback(delegate {
				Console.WriteLine(new Fen(board));
			}),
			new Button(new Rectangle(0, 0, 210, 50), "Print bitboards").SetLeftCallback(delegate {
				BoardHelper.PrintBoard(board);
			}))
		);

		// var temp = new StackRenderer(new Vector2(View.screenSize.X-250, 200), 
		// 	new Button(new Rectangle(0, 0, 210, 50), "Test").SetLeftCallback(delegate {
		// 		string fen = Perft.testedFens.ElementAt(new Random().Next(Perft.testedFens.Count)).Key;
		// 		SetBoardPosition(fen);
		// 		view.Draw();
		// 		WaveFunctionCollapse.CalculateMoveDiscrepancy(new Fen(board).ToString(), 6);
		// 	}),
		// 	new Button(new Rectangle(0, 0, 210, 50), "Get discrepancy").SetLeftCallback(delegate {
		// 		// Perft.TestSpecific(6);
		// 		foreach (string fen in Perft.testedFens.Keys) {
		// 			int depth = int.Parse(Perft.testedFens[fen].Last().Key);
		// 			ConsoleHelper.WriteLine($"Testing Fen: {fen}", ConsoleColor.Magenta);
		// 			WaveFunctionCollapse.CalculateMoveDiscrepancy(fen, depth);
		// 		}
		// 	})
		// );

		// temp.AddElements(
		// 	new Button(new Rectangle(0, 0, 210, 50), "Pop last button").SetLeftCallback(delegate {
		// 		temp.RemoveElement(^1);
		// 	}),
		// 	new Button(new Rectangle(0, 0, 210, 50), "Add a Button").SetLeftCallback(delegate {
		// 		temp.AddElement(
		// 			new Button(new Rectangle(0, 0, 210, 50), "Added Button").SetLeftCallback(delegate {
						
		// 			})
		// 		);
		// 	})
		// );

		// view.AddToPipeline(temp);


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
	public void ResetBoardPosition() { SetBoardPosition(Fen.startpos); }
	public void SetBoardPosition(string fenString) {
		// if (currentBoard != null) SetOldBoard();
		board = new Board(fenString);
		whitePlayer.UCI?.RaiseManualUpdateFlag();
		blackPlayer.UCI?.RaiseManualUpdateFlag();
		
		view.ui.pieceBitboards = board.pieces;
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
		view.ui.IsFlipped = humanColor == 0b01; // if white is not a player and black is a player
		gameIndex++;
		// view.ui.activeAnimations.AddRange(AnimationHelper.FromBoardChange(oldBoard, board.board, 0.2f));
		SuspendPlay = false;
	}
	public void AnimateSingleRewind() {
		if (board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get first previous state, is null"); return; }

		board.SetPrevState();
		whitePlayer.UCI?.RaiseManualUpdateFlag();
		blackPlayer.UCI?.RaiseManualUpdateFlag();

		view.ui.activeAnimations.AddRange(AnimationHelper.ReverseFromGamestate(board.currentState));
	}
	public void AnimateSingleForward() {
		if (board.currentStateNode.Next == null) { Console.WriteLine("Cannot get first next state, is null"); return; }

		view.ui.activeAnimations.AddRange(AnimationHelper.FromGamestate(board.currentState));
		board.SetNextState();
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
