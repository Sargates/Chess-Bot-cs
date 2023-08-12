// using ChessBot.Application;
// using ChessBot.Helpers;

// namespace ChessBot.Engine {
// 	public static class Perft {
// 		public static int[] depthList = new int[23];
// 		static int maxDepth = 4;
// 		public static void Main() {
// 			GetDepth();
// 		}

// 		public static void GetDepth() {
// 			// Board board = new Board();
// 			Model model = new Model();
// 			// model.StartNewGame("rnbqkbnr/1ppppppp/p7/1B6/4P3/8/PPPP1PPP/RNBQK1NR b KQkq - 1 2");
// 			model.StartNewGame();

// 			depthList[0] = 1;
// 			CountMove(model, 1);


// 			for (int i=0; i<depthList.Length; i++) {
// 				Console.WriteLine($"Depth {i}: {depthList[i]}");
// 			}
// 		}

// 		public static int CountMove(Model model, int depth) {
// 			if (depth > maxDepth) { return 1; }
// 			int targetDepth = 2;
// 			List<Move> totalMoves = new List<Move>();
// 			int subsequentSum = 0;
// 			for (int i=0; i<64; i++) {
// 				Piece piece = model.board.GetSquare(i);
// 				if ((piece.IsNull) || piece.Color != model.board.activeColor) { continue; }
// 				totalMoves.AddRange(MoveGenerator.GetMoves(model.board, i));
// 			}

// 			int[] sumList = new int[totalMoves.Count];
// 			for (int i=0; i<totalMoves.Count; i++) {
// 				Move move = totalMoves[i];
// 				model.board.MakeMove(move);

// 				// If move is not illegal
// 				if (! MoveGenerator.IsSquareAttacked(model.board, model.board.activeColor!=Piece.White ? model.board.whiteKingPos : model.board.blackKingPos, model.board.opponentColour(model.board.activeColor))) {
// 					depthList[depth] += 1;
// 					// sum += 1;
// 					int x = CountMove(model, depth + 1);
// 					if (depth == targetDepth-1) {
// 						Console.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)}{BoardHelper.IndexToSquareName(move.TargetSquare)}: {x}");
// 					}
// 					subsequentSum += x;
// 				}
// 				model.SetPrevState();
// 			}


// 			return subsequentSum;
// 		}
// 	}
// }