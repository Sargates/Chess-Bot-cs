using ChessBot.Engine;
using ChessBot.Helpers;

namespace ChessBot.Application {
	public class Model {

		public Board board;
		public bool enforceColorToMove = false;

		public Model() {
			board = new Board("rnbqkbnr/pppppppp/8/8/8/8/PPPPPPPP/RNBQKBNR w KQkq - 0 1");
			// board = new Board("rnb1nrk1/1pq3bp/p2p2p1/2pPp3/P1P1Pp2/2NBBN2/1PQ2PPP/R4RK1 w - - 0 13");
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