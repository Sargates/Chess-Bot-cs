using System.Diagnostics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class Model {

		public Board board;
		public bool enforceColorToMove = false;
        public readonly string[] botMatchStartFens;

		public Piece[] oldBoard = new Piece[64];
		public void SetOldBoard() { oldBoard = board.board.ToArray(); }

		public int gameIndex = 0;

		public enum Gametype {
			HvH, // 	Human vs. Human
			HvC, // 	Human vs. Computer
			HvU, // 	Human vs. UCI
			CvC, //  Computer vs. Computer
			CvU, //  Computer vs. UCI
			UvU, // 	  UCI vs. UCI
		}

		public int humanColor = 0b00; // 0b10 for white, 0b01 for black, 0b11 for both

		public Gametype activeGameType = Gametype.HvH;

		public ChessPlayer whitePlayer = new ChessPlayer();
		public ChessPlayer blackPlayer = new ChessPlayer();
		public ChessPlayer ActivePlayer => board.whiteToMove ? whitePlayer : blackPlayer;

		public delegate void NewGameDel();
		public static NewGameDel NewGameCalls = () => {};

		public Model() {
			StartNewGame();
			Debug.Assert(board != null);

            botMatchStartFens = FileHelper.ReadResourceFile("Fens.txt").Split('\n');
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
			whitePlayer.RaiseExitFlag();
			blackPlayer.RaiseExitFlag();
			whitePlayer = (type, gameIndex%2==1) switch {
				(Gametype.HvH, true) => new ChessPlayer(new Player('w'), 300f),
				(Gametype.HvC, true) => new ChessPlayer(new Player('w'), 300f),
				(Gametype.HvU, true) => new ChessPlayer(new Player('w'), 300f),
				(Gametype.CvC, true) => new ChessPlayer(new ComputerPlayer('w', this), 60f),
				(Gametype.CvU, true) => new ChessPlayer(new ComputerPlayer('w', this), 60f),
				(Gametype.UvU, true) => new ChessPlayer(new UCIPlayer('w', this, "stockfish-windows-x86-64-avx2.exe"), 30f),
				(Gametype.HvH, false) => new ChessPlayer(new Player('b'), 300f),
				(Gametype.HvC, false) => new ChessPlayer(new ComputerPlayer('b', this), 60f),
				(Gametype.HvU, false) => new ChessPlayer(new UCIPlayer('b', this, "stockfish-windows-x86-64-avx2.exe"), 30f),
				(Gametype.CvC, false) => new ChessPlayer(new ComputerPlayer('b', this), 60f),
				(Gametype.CvU, false) => new ChessPlayer(new UCIPlayer('b', this, "stockfish-windows-x86-64-avx2.exe"), 30f),
				(Gametype.UvU, false) => new ChessPlayer(new UCIPlayer('b', this, "stockfish-windows-x86-64-avx2.exe"), 30f),
				_ => throw new Exception("Shut up compiler!")
			};
			blackPlayer = (type, gameIndex%2==1) switch {
				(Gametype.HvH, true) => new ChessPlayer(new Player('b'), 300f),
				(Gametype.HvC, true) => new ChessPlayer(new ComputerPlayer('b', this), 60f),
				(Gametype.HvU, true) => new ChessPlayer(new UCIPlayer('b', this, "stockfish-windows-x86-64-avx2.exe"), 30f),
				(Gametype.CvC, true) => new ChessPlayer(new ComputerPlayer('b', this), 60f),
				(Gametype.CvU, true) => new ChessPlayer(new UCIPlayer('b', this, "stockfish-windows-x86-64-avx2.exe"), 30f),
				(Gametype.UvU, true) => new ChessPlayer(new UCIPlayer('b', this, "stockfish-windows-x86-64-avx2.exe"), 30f),
				(Gametype.HvH, false) => new ChessPlayer(new Player('w'), 300f),
				(Gametype.HvC, false) => new ChessPlayer(new Player('w'), 300f),
				(Gametype.HvU, false) => new ChessPlayer(new Player('w'), 300f),
				(Gametype.CvC, false) => new ChessPlayer(new ComputerPlayer('w', this), 60f),
				(Gametype.CvU, false) => new ChessPlayer(new ComputerPlayer('w', this), 60f),
				(Gametype.UvU, false) => new ChessPlayer(new UCIPlayer('w', this, "stockfish-windows-x86-64-avx2.exe"), 30f),
				_ => throw new Exception("Shut up compiler!")
			};
			activeGameType = type;

			
			whitePlayer.StartThread();
			blackPlayer.StartThread();
			humanColor |= (whitePlayer.Player != null) ? 0b10 : 0b00;
			humanColor |= (blackPlayer.Player != null) ? 0b01 : 0b00;

			Console.WriteLine($"White: {whitePlayer}");
			Console.WriteLine($"Black: {blackPlayer}");
		}

		public void StartNewGame(Gametype type=Gametype.HvH) { StartNewGame(Fen.startpos, type); }
		public void StartNewGame(string fenString, Gametype type=Gametype.HvH) {
			//* Instantiate starting gamestate
			//* Instantiate new Board passing starting gamestate
			//* Recalc bitboards
			//* 
			//* 

			SetBoardPosition(fenString);
			ConsoleHelper.WriteLine($"\nGame number {gameIndex} started\nFEN: {fenString}", ConsoleColor.Yellow);
			// Console.WriteLine(activeGameType == type);
			SetPlayerTypes(type);
			NewGameCalls();
			gameIndex++;

		}


		
		public void SetPrevState() {
			if (board.currentStateNode.Previous == null) { Console.WriteLine("Cannot get previous state, is null"); return; }
			
			board.currentStateNode = board.currentStateNode.Previous;
			board.currentFen = board.currentStateNode.Value;
			board.UpdateFromState();
			whitePlayer.UCI?.RaiseManualUpdateFlag();
			blackPlayer.UCI?.RaiseManualUpdateFlag();

		}
		public void SetNextState() {
			if (board.currentStateNode.Next == null) { Console.WriteLine("Cannot get next state, is null"); return; }
			board.currentStateNode = board.currentStateNode.Next;
			board.currentFen = board.currentStateNode.Value;
			board.UpdateFromState();
			whitePlayer.UCI?.RaiseManualUpdateFlag();
			blackPlayer.UCI?.RaiseManualUpdateFlag();			
		}

		public void Update() {
			
		}

		


	}
}