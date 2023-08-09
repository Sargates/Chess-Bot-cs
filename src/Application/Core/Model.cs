using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class Model {

		public Board board;
		public bool enforceColorToMove = false;

		public Model() {
			board = new Board("r3k2r/Pppp1ppp/1b3nbN/nP6/BBP1P3/q4N2/Pp1P2PP/R2Q1RK1 w kq - 0 1");

			Player whitePlayer = new Player('w');
			Player blackPlayer = new Player('b');
		}

		



		public void StartNewGame(string fenString) {
			//* Instantiate starting gamestate
			//* Instantiate new Board passing starting gamestate
			//* Recalc bitboards
			//* 
			//* 

			
			board = new Board(fenString);
			
		}


	}
}