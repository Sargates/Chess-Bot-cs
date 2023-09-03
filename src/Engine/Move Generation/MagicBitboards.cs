namespace ChessBot.Engine;

public static class MagicBitboards {
	public static ulong GetRookMoves(int index, ulong board) {
		ulong blockers = board & PrecomputedMoveData.GetRookMoves(index);
		ulong hash = (blockers * PrecomputedMagics.RookMagics[index]) >> PrecomputedMagics.RookShifts[index];

		return hash;
	}
}