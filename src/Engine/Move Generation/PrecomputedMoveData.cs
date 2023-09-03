using ChessBot.Helpers;


namespace ChessBot.Engine;

public static class PrecomputedMoveData {
	public static ulong[][] pawnMoves = new ulong[2][];
	public static ulong[][] pawnAttacks = new ulong[2][];
	public static ulong[] knightMoves = new ulong[64];

	// Used to avoid having to remember `numSquaresToEdge` index
	private static readonly Dictionary<int, int> knightDeltaToIndex = new Dictionary<int, int>() { {6, 0}, {15, 1}, {17, 2}, {10, 3}, {-6, 4}, {-15, 5}, {-17, 6}, {-10, 7} };
	private static readonly Dictionary<int, int> slidingDeltaToIndex = new Dictionary<int, int>() { { 8, 0}, {-8, 1}, {1, 2}, {-1, 3}, {9, 4}, {-9, 5}, {7, 6}, {-7, 7} };
	public static int GetKnightInfo(int index, int direction) { return numSquaresToEdge[index][knightDeltaToIndex[direction]]; }
	public static int GetSlidingInfo(int index, int direction) { return numSquaresToEdge[index][knightDeltaToIndex[direction]]; }

	public static int[][] numSquaresToEdge = new int[64][];
	public static ulong[] rookMoves = new ulong[64];
	public static ulong[] bishopMoves = new ulong[64];

	public static ulong GetPawnMoves(int color, int index) { return pawnMoves[color][index]; }
	public static ulong GetPawnAttacks(int color, int index) { return pawnMoves[color][index]; }
	public static ulong GetKnightMoves(int index) { return knightMoves[index]; }
	public static ulong GetRookMoves(int index) { return rookMoves[index]; }
	public static ulong GetBishopMoves(int index) { return bishopMoves[index]; }

	static PrecomputedMoveData() {
		Console.WriteLine("Initializing precomputed move data");

		// Initialize jagged array
		for (int i=0; i<64; i++) { numSquaresToEdge[i] = new int[8]; }
		pawnMoves[0] = new ulong[64];
		pawnMoves[1] = new ulong[64];
		pawnAttacks[0] = new ulong[64];
		pawnAttacks[1] = new ulong[64];

		// Generate distance to board edge by index
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
		// 		continue;
		// 	}
		// 	Console.WriteLine("\nRook Moves");
		// 	BitboardHelper.PrintBitboard(rookMoves[iResponse]);
			
		// 	response = Console.ReadLine()?.Trim().Replace(Environment.NewLine, "") ?? "exit";
		// }
	}
}