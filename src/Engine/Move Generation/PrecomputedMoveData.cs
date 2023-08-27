using ChessBot.Helpers;


namespace ChessBot.Engine;

public static class PrecomputedMoveData {
	public static ulong[][] pawnMoves = new ulong[2][];
	public static ulong[][] pawnAttacks = new ulong[2][];
	public static ulong[] knightMoves = new ulong[64];

	public static int[][] numSquaresToEdge = new int[64][];
	public static ulong[] rookMoves = new ulong[64];
	public static ulong[] bishopMoves = new ulong[64];

	static PrecomputedMoveData() {
		Console.WriteLine("Initializing precomputed move data");

		// Initialize jagged array
		for (int i=0; i<64; i++) { numSquaresToEdge[i] = new int[8]; }
		pawnMoves[0] = new ulong[64];
		pawnMoves[1] = new ulong[64];
		pawnAttacks[0] = new ulong[64];
		pawnAttacks[1] = new ulong[64];

		// Generate distance to board edge by 
		for (int i=0; i<64; i++) {
			int y = i/8;
			int x = i%8;

			int north = 7 - y; // Squares to the top
			int south = 	y; // Squares to the bottom
			int east = 	7 - x; // Squares to the right
			int west = 		x; // Squares to the left

			numSquaresToEdge[i][0] = north;
			numSquaresToEdge[i][1] = south;
			numSquaresToEdge[i][2] = east;
			numSquaresToEdge[i][3] = west;
			numSquaresToEdge[i][4] = Math.Min(north, east); // Diagonal distances are just the min of the two components
			numSquaresToEdge[i][5] = Math.Min(south, west);
			numSquaresToEdge[i][6] = Math.Min(north, west);
			numSquaresToEdge[i][7] = Math.Min(south, east);
		}
		
		// int[] knightMoves = new int[] { 15, 17, 17, 15, 10, -6, 6, 10 };
		//* Generate Non-sliding piece bitboards
		for (int i=0; i<64; i++) {
			ref ulong wpM = ref pawnMoves[0][i]; 	// White Pawn moves
			ref ulong wpA = ref pawnAttacks[0][i]; 	// White Pawn moves
			ref ulong bpM = ref pawnMoves[1][i]; 	// Black Pawn moves
			ref ulong bpA = ref pawnAttacks[1][i]; 	// Black Pawn moves
			ref ulong kM = ref knightMoves[i]; 		// Knight moves (included here just for cleanliness)

			Coord position = new Coord(i);
			Coord PawnMove, PositivePawnAttack, NegativePawnAttack;
			// Generate White Pawn Moves
			if (i < 56) {
				PawnMove = new Coord(0, 1);
				PositivePawnAttack = new Coord( 1, 1);
				NegativePawnAttack = new Coord(-1, 1);

				wpM |= 1ul<<(position+PawnMove).SquareIndex;

				if (8<=i && i<16) {
					wpM |= 1ul<<(position+(PawnMove*2)).SquareIndex;
				}
				if (position.file != 7) {
					wpA |= 1ul<<(position+PositivePawnAttack).SquareIndex;
				}
				if (position.file != 0) {
					wpA |= 1ul<<(position+NegativePawnAttack).SquareIndex;
				}
			}
			// Generate Black Pawn Moves
			if (i >= 8) {
				PawnMove = new Coord(0, -1);
				PositivePawnAttack = new Coord( 1, -1);
				NegativePawnAttack = new Coord(-1, -1);

				bpM |= 1ul<<(position+PawnMove).SquareIndex;

				if (48<=i && i<56) {
					bpM |= 1ul<<(position+(PawnMove*2)).SquareIndex;
				}
				if (position.file != 7) {
					bpA |= 1ul<<(position+PositivePawnAttack).SquareIndex;
				}
				if (position.file != 0) {
					bpA |= 1ul<<(position+NegativePawnAttack).SquareIndex;
				}
			}
			
			// Generate knightMoves
			Coord location = new Coord(i);
			foreach (int knightDelta in new int[] { 6, 15, 17, 10, -6, -15, -17, -10 }) {
				Coord knightCoord = BoardHelper.GetAbsoluteDirection(knightDelta);
				if ((-numSquaresToEdge[i][1] <= knightCoord.rank && knightCoord.rank <= numSquaresToEdge[i][0]) && (-numSquaresToEdge[i][3] <= knightCoord.file && knightCoord.file <= numSquaresToEdge[i][2])) {
					kM |= 1ul<<(location+knightCoord).SquareIndex;
				}
			}
		}


		//* Generate Sliding piece bitboards
		// [0, 3] is rook, [4, 7] is bishop
		int[] directions = { 8, -8, 1, -1, 9, -9, 7, -7};
		for (int i=0; i<64; i++) {
			for (int j=0; j<8; j++) {
				// ulong bitboard = 0ul;
				ulong bitboard = (j<4) ? ref rookMoves[i] : ref bishopMoves[i];
				for (int k=1; k<=numSquaresToEdge[i][j]-1; k++) {
					// The coefficient k should be multiplied after generating the absolute direction to prevent wrong values caused by big numbers passed to the method
					Coord position = new Coord(i)+BoardHelper.GetAbsoluteDirection(directions[j])*k;
					bitboard |= 1ul<<position.SquareIndex;
				}
				if (j<4) {
					rookMoves[i] |= bitboard;
				} else {
					bishopMoves[i] |= bitboard;
				}
			}
		}

		// string response = "12";
		// while (true) {
		// 	if (response == "exit") { break; }
		// 	int iResponse;
		// 	try {
		// 		iResponse = int.Parse(response);
		// 	} catch (Exception e) {
		// 		Console.WriteLine(e);
		// 		break;
		// 	}
		// 	Console.WriteLine("\nWhite Pawn Moves");
		// 	for (int i=7;i>-1;i--) {
		// 		for (int j=0; j<8; j++) {
		// 			int index = 8*i+j;
		// 			string bit = Convert.ToString((long)((pawnMoves[0][iResponse] >> index) & 1ul), 2);
		// 			Console.Write(bit);
		// 		}
		// 		Console.WriteLine();
		// 	}
		// 	Console.WriteLine("\nWhite Pawn Attacks");
		// 	for (int i=7;i>-1;i--) {
		// 		for (int j=0; j<8; j++) {
		// 			int index = 8*i+j;
		// 			string bit = Convert.ToString((long)((pawnAttacks[0][iResponse] >> index) & 1ul), 2);
		// 			Console.Write(bit);
		// 		}
		// 		Console.WriteLine();
		// 	}
		// 	Console.WriteLine("\nBlack Pawn Moves");
		// 	for (int i=7;i>-1;i--) {
		// 		for (int j=0; j<8; j++) {
		// 			int index = 8*i+j;
		// 			string bit = Convert.ToString((long)((pawnMoves[1][iResponse] >> index) & 1ul), 2);
		// 			Console.Write(bit);
		// 		}
		// 		Console.WriteLine();
		// 	}
		// 	Console.WriteLine("\nBlack Pawn Attacks");
		// 	for (int i=7;i>-1;i--) {
		// 		for (int j=0; j<8; j++) {
		// 			int index = 8*i+j;
		// 			string bit = Convert.ToString((long)((pawnAttacks[1][iResponse] >> index) & 1ul), 2);
		// 			Console.Write(bit);
		// 		}
		// 		Console.WriteLine();
		// 	}

		// 	response = Console.ReadLine()?.Trim().Replace(Environment.NewLine, "") ?? "exit";
		// }
	}
}