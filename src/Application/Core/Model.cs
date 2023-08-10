using System.Diagnostics;
using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class Model {

		public Board board;
		public bool enforceColorToMove = false;
        public readonly string[] botMatchStartFens;


		public Model() {
			StartNewGame();

			Debug.Assert(board != null);
            botMatchStartFens = FileHelper.ReadResourceFile("Fens.txt").Split('\n');
		}


		public void StartNewGame() { StartNewGame(Fen.startpos); }
		public void StartNewGame(string fenString) {
			//* Instantiate starting gamestate
			//* Instantiate new Board passing starting gamestate
			//* Recalc bitboards
			//* 
			//* 

			board = new Board(fenString);
			ConsoleHelper.WriteLine($"\nNew game started\nFEN: {fenString}", ConsoleColor.Yellow);
			Controller.whitePlayer.UCI?.RaiseManualUpdateFlag();
			Controller.blackPlayer.UCI?.RaiseManualUpdateFlag();
			
			
		}

		public void Update() {
			
		}

		


	}
}