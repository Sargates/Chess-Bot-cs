

namespace ChessBot.Engine;
using Eval = Evaluation;
public static class Evaluation {
	public const int Pawn 	= 100;
	public const int Knight = 300;
	public const int Bishop = 300;
	public const int Rook 	= 500;
	public const int Queen 	= 900;
	public const int King 	= 100000;
	public static readonly int[] ValuesByEnum = { 0, Pawn, Knight, Bishop, Rook, Queen, King };
	public static int EnumToValue(int e) => Evaluation.ValuesByEnum[e];

	public static int SumFromBoard(Board board, int color) {
		int sum=0;
		for (int i=0; i<64; i++) {
			Piece piece = board.GetSquare(i);
			sum += Eval.ValuesByEnum[piece.Type] * (piece.Color == color ? 1 : -1);
		}
		return sum;
	}
}