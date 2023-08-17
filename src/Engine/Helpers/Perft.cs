// using ChessBot.Application;
// using ChessBot.Helpers;

// namespace ChessBot.Helpers;
// 	public static class Perft {
// 		public static int[] depthList = new int[23];
// 		static int maxDepth = 4;
// 		public static void Main() {
// 			GetDepth();
// 		}

// 		public static void GetDepth() {
// 			// Board board = new Board();
// 			Model model = new Model();
// 			model.StartNewGame("8/2p5/3p4/KP5r/1R3p1k/8/4P1P1/8 w - - 0 1");
// 			// model.StartNewGame("8/2p5/3p4/KP4kr/1R3pP1/4P3/8/8 b - g3 0 2");
// 			// model.board.MakeMove(new Move(BoardHelper.NameToSquareIndex("g2"), BoardHelper.NameToSquareIndex("g4"), Move.PawnTwoUpFlag));

// 			depthList[0] = 1;
// 			CountMove(model, 1);


// 			Console.WriteLine();
// 			for (int i=0; i<depthList.Length; i++) {
// 				Console.WriteLine($"Depth {i}: {depthList[i]}");
// 			}
// 		}

// 		public static int CountMove(Model model, int depth) {
// 			if (depth > maxDepth) { return 1; }
// 			List<Move> totalMoves = new List<Move>();
// 			int subsequentSum = 0;
// 			for (int i=0; i<64; i++) {
// 				Piece piece = model.board.GetSquare(i);
// 				if ((piece.IsNull) || piece.Color != model.board.activeColor) { continue; }
// 				totalMoves.AddRange(MoveGenerator.GetMoves(model.board, i));
// 			}

// 			for (int i=0; i<totalMoves.Count; i++) {
// 				Move move = totalMoves[i];
// 				model.board.MakeMove(move);

// 				depthList[depth] += 1;

// 				// Console.WriteLine();
// 				// ConsoleHelper.Write(BoardHelper.IndexToSquareName(move.StartSquare), ConsoleColor.Red);
// 				int x = CountMove(model, depth + 1);
// 				subsequentSum += x;
// 				// ConsoleHelper.Write(BoardHelper.IndexToSquareName(move.StartSquare), ConsoleColor.Red);
// 				if (depth == 1) {
// 					Console.WriteLine($"{BoardHelper.IndexToSquareName(move.StartSquare)}{BoardHelper.IndexToSquareName(move.TargetSquare)}: {x}");
// 				}

// 				model.SetPrevState();
// 			}


// 			return subsequentSum;
// 		}
// 	}
// }