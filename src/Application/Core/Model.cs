using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class Model {

		public Board board;
		public bool enforceColorToMove = false;

		public Model() {
			board = new Board();
			// board = new Board("rnbqkbnr/ppp1pppp/8/3P4/8/8/PPPP1PPP/RNBQKBNR b KQkq e6 0 5");
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