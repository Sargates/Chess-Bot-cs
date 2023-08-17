using System;
namespace ChessBot.Engine; //* Copied from SebLague
	// Structure for representing squares on the chess board as file/rank integer pairs.
	// (0, 0) = a1, (7, 7) = h8.
	// Coords can also be used as offsets. For example, while a Coord of (-1, 0) is not
	// a valid square, it can be used to represent the concept of moving 1 square left.

	public readonly struct Coord : IComparable<Coord> {
		public readonly int file;
		public readonly int rank;

		public Coord(int fileIndex, int rankIndex) {
			this.file = fileIndex;
			this.rank = rankIndex;
		}

		public Coord(int squareIndex) {
			this.file = squareIndex & 0b111;
			this.rank = squareIndex >> 3;
		}

		public bool IsLightSquare() {
			return (file + rank) % 2 != 0;
		}

		public int CompareTo(Coord other) {
			return (file == other.file && rank == other.rank) ? 0 : 1;
		}

		public static Coord operator +(Coord a, Coord b) => new Coord(a.file + b.file, a.rank + b.rank);
		public static Coord operator -(Coord a, Coord b) => new Coord(a.file - b.file, a.rank - b.rank);
		public static Coord operator *(Coord a, int m) => new Coord(a.file * m, a.rank * m);
		public static Coord operator *(int m, Coord a) => a * m;

		public bool IsInBounds() => 0 <= file && file < 8 && 0 <= rank && rank < 8;
		public override string ToString() {
			return $"<{file}, {rank}>";
		}
		public int SquareIndex => 8*rank+file;
	}