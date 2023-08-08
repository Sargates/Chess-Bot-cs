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
// 			Board board = new Board("r3k2r/p1ppqpb1/bn2pnp1/3PN3/1p2P3/2N2Q1p/PPPBBPPP/R3K2R w KQkq - 0 1");

// 			depthList[0] = 1;
// 			CountMove(board, 1);


// 			for (int i=0; i<depthList.Length; i++) {
// 				Console.WriteLine($"Depth {i}: {depthList[i]}");
// 			}
// 		}

// 		public static int CountMove(Board board, int depth) {
// 			if (depth > maxDepth) { return 1; }
// 			int targetDepth = 2;
// 			List<Move> totalMoves = new List<Move>();
// 			int sum = 0;
// 			for (int i=0; i<64; i++) {
// 				Piece piece = board.GetSquare(i);
// 				if ((piece.IsNull) || piece.Color != board.activeColor) { continue; }
// 				totalMoves.AddRange(MoveGenerator.GetMoves(board, i));
// 			}

// 			int[] sumList = new int[totalMoves.Count];
// 			for (int i=0; i<totalMoves.Count; i++) {
// 				Move move = totalMoves[i];
// 				board.MakeMove(move, true);

// 				// If move is not illegal
// 				if (! MoveGenerator.IsSquareAttacked(board, (board.activeColor!=Piece.White ? board.whiteKingPos : board.blackKingPos), board.opponentColour(board.activeColor))) {
// 					depthList[depth] += 1;
// 					// sum += 1;
// 					sum += CountMove(board, depth + 1);
// 					if (depth == targetDepth-1) {
// 						// Console.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)}{BoardHelper.IndexToSquareName(move.TargetSquare)} {subsequentSum}");
// 					}
// 				}

// 				board.UnmakeMove();
// 			}


// 			return sum;
// 		}
// 	}
// }