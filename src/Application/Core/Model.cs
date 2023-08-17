using Raylib_cs;
using System.Diagnostics;
using System.Runtime.InteropServices;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application;
public class Model {

	public View view;
	public Board board;
	public bool enforceColorToMove = false;
	public readonly string[] botMatchStartFens;

	public Piece[] oldBoard = new Piece[64];
	public void SetOldBoard() { oldBoard = board.board.ToArray(); }



	public string stockfishExeExt;

	public int humanColor = 0b00; // 0b10 for white, 0b01 for black, 0b11 for both

	public bool SuspendPlay = false;

	public enum Gametype {
		HvH, // 	Human vs. Human
		HvC, // 	Human vs. Computer
		HvU, // 	Human vs. UCI
		CvC, //  Computer vs. Computer
		CvU, //  Computer vs. UCI
		UvU, // 	  UCI vs. UCI
	}
	public Gametype activeGameType = Gametype.HvH;
	public int gameIndex = 0;
	public ChessPlayer whitePlayer = new ChessPlayer();
	public ChessPlayer blackPlayer = new ChessPlayer();
	public ChessPlayer ActivePlayer => board.whiteToMove ? whitePlayer : blackPlayer;
	public ChessPlayer InactivePlayer => board.whiteToMove ? blackPlayer : whitePlayer;

	public Model(View view) {
		this.view = view;
		
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
			stockfishExeExt = "stockfish-windows-x86-64-avx2.exe";
		} else
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) {
			stockfishExeExt = "stockfish";
		} else {
			throw new Exception("This program can only run on Windows and Linux");
		}
		StartNewGame();
		Debug.Assert(board != null);
		AddButtons();

		botMatchStartFens = FileHelper.ReadResourceFile("Fens.txt").Split('\n');
	}

	public void AddButtons() {
		view.AddToPipeline(new Button(new Rectangle(40, 420, 210, 50), "Freeplay"					).SetCallback(delegate {
			StartNewGame();
		}));
		view.AddToPipeline(new Button(new Rectangle(40, 480, 210, 50), "Human vs. Gatesfish"		).SetCallback(delegate {
			StartNewGame(type:Gametype.HvC);
		}));
		view.AddToPipeline(new Button(new Rectangle(40, 540, 210, 50), "Human vs. Stockfish"		).SetCallback(delegate {
			StartNewGame(type:Gametype.HvU);
		}));
		view.AddToPipeline(new Button(new Rectangle(40, 600, 210, 50), "Stockfish vs. Stockfish"	).SetCallback(delegate {
			StartNewGame(type:Gametype.UvU);
		}));
	}


	// Below methods handle the changing state of the board
	public void MakeMoveOnBoard(Move move, bool animate=true) {
		if (move.IsNull) {
			Console.WriteLine("Null move was made, assumed checkmate");
			return;
		}

		view.ui.DeselectActiveSquare();
		ActivePlayer.IsSearching = false;


		Piece pieceMoved = board.GetSquare(move.StartSquare);
		board.MakeMove(move);
		view.TimeOfLastMove = view.fTimeElapsed;

		bool opponentInCheck = MoveGenerator.IsSquareAttacked(board, board.activeColor == Piece.White ? board.whiteKingPos : board.blackKingPos, board.activeColor);
		bool canOpponentRespond = MoveGenerator.GetAllMoves(board, board.activeColor).Length != 0; // Negated for readability

		if (board.currentStateNode.Previous == null) { throw new Exception("Something went wrong"); }
		Fen temp = board.currentStateNode.Previous.Value;

		if (opponentInCheck && canOpponentRespond) {
			temp.moveMade.moveSoundEnum = (int)SoundStates.Check;
		} else
		if (opponentInCheck && ! canOpponentRespond) {
			temp.moveMade.moveSoundEnum = 0; // Sound is played separately if game is over
		} else
		if (! opponentInCheck && ! canOpponentRespond) {
			temp.moveMade.moveSoundEnum = 0; // Sound is played separately if game is over
		}

		Debug.Assert(board.currentStateNode.Previous != null);
		board.currentStateNode.Previous.Value = temp;

		if (animate) { // Sounds are built in to animations, if move is not animated, play sound manually
			view.ui.activeAnimations.AddRange(AnimationHelper.FromMove(temp.moveMade, pieceMoved));
		} else {
			Raylib.PlaySound(BoardUI.sounds[board.currentStateNode.Previous.Value.moveMade.moveSoundEnum]);
		}

		// If opponent can't respond, fallthrough to game end handling
		if (canOpponentRespond) {
			return;
		}

		SuspendPlay = true;

		if (opponentInCheck) { // Checkmate
			Raylib.PlaySound(BoardUI.sounds[(int)SoundStates.Checkmate]);
			ConsoleHelper.WriteLine("Checkmate!", ConsoleColor.DarkBlue);
			ConsoleHelper.WriteLine($"Winner: {InactivePlayer.color}, Loser: {ActivePlayer.color}", ConsoleColor.DarkBlue);
		} else { // Stalemate
			Raylib.PlaySound(BoardUI.sounds[(int)SoundStates.Stalemate]);
			ConsoleHelper.WriteLine("Stalemate", ConsoleColor.DarkBlue);
			ConsoleHelper.WriteLine($"Draw.", ConsoleColor.DarkBlue);
		}

		// Handle Checkmate
	}
	public void ExitPlayerThreads() { whitePlayer.RaiseExitFlag(); blackPlayer.RaiseExitFlag(); }
	public void JoinPlayerThreads() { whitePlayer.Join(); blackPlayer.Join(); }
	public void SetBoardPosition() { SetBoardPosition(Fen.startpos); }
	public void SetBoardPosition(string fenString) {
		if (board != null) SetOldBoard();
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
		view.ui.isFlipped = humanColor == 0b01; // if white is not a player and black is a player
		gameIndex++;
		view.ui.activeAnimations.AddRange(AnimationHelper.FromBoardChange(oldBoard, board.board, 0.2f));
	}
	public void SetPrevState() {
		if (board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get previous state, is null"); return; }
		
		board.currentStateNode = board.currentStateNode.Previous;
		board.UpdateFromState();
		view.ui.boardToRender = board.board;
		whitePlayer.UCI?.RaiseManualUpdateFlag();
		blackPlayer.UCI?.RaiseManualUpdateFlag();
	}
	public void SetNextState() {
		if (board.currentStateNode.Next == null) { Console.WriteLine("Cannot get next state, is null"); return; }
		board.currentStateNode = board.currentStateNode.Next;
		board.UpdateFromState();
		view.ui.boardToRender = board.board;
		whitePlayer.UCI?.RaiseManualUpdateFlag();
		blackPlayer.UCI?.RaiseManualUpdateFlag();
	}
	public void SinglePrevState() {
		if (board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get first previous state, is null"); return; }
		Move move; Piece piece;

		SetPrevState();
		move = board.currentStateNode.Value.moveMade;
		piece = board.GetSquare(move.StartSquare);

		view.ui.activeAnimations.AddRange(AnimationHelper.ReverseFromMove(move, piece));
	}
	public void SingleNextState() {
		if (board.currentStateNode.Next == null) { Console.WriteLine("Cannot get first next state, is null"); return; }
		Move move; Piece piece;

		move = board.currentStateNode.Value.moveMade;
		piece = board.GetSquare(move.StartSquare);
		SetNextState();

		view.ui.activeAnimations.AddRange(AnimationHelper.FromMove(move, piece));
	}
	public void DoublePrevState() {
		if (board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get first previous state, is null"); return; }
		Move move; Piece piece;
		SetPrevState();
		move = board.currentStateNode.Value.moveMade;
		piece = board.GetSquare(move.StartSquare);
		view.ui.activeAnimations.AddRange(AnimationHelper.ReverseFromMove(move, piece));

		// Just mimics Single____State twice, but adding a lag to the second animation
		if (board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get second previous state, is null"); return; }
		SetPrevState();
		move = board.currentStateNode.Value.moveMade;
		piece = board.GetSquare(move.StartSquare);
		view.ui.activeAnimations.AddRange(AnimationHelper.ReverseFromMove(move, piece, lag:-0.04f));
	}
	public void DoubleNextState() {
		if (board.currentStateNode.Next == null) { Console.WriteLine("Cannot get first next state, is null"); return; }
		Move move; Piece piece;
		move = board.currentStateNode.Value.moveMade;
		piece = board.GetSquare(move.StartSquare);
		SetNextState();
		view.ui.activeAnimations.AddRange(AnimationHelper.FromMove(move, piece));

		// Just mimics Single____State twice, but adding a lag to the second animation
		if (board.currentStateNode.Next == null) { Console.WriteLine("Cannot get second next state, is null"); return; }
		move = board.currentStateNode.Value.moveMade;
		piece = board.GetSquare(move.StartSquare);
		SetNextState();
		view.ui.activeAnimations.AddRange(AnimationHelper.FromMove(move, piece, lag:-0.04f));
	}
}
