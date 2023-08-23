namespace ChessBot.Engine;


public class Gamestate {
	public int ID = -1; // -1 if not getting set
	public Move moveMade;		// Move made on the active gamestate
	public Piece pieceMoved;	// Piece moved by `moveMade`
	public Piece pieceTaken;	// Piece taken by `moveMade`; Can be `Piece.None`
	public int soundPlayed;		// Sound played by `moveMade`; Defaults to MoveSounds
	public int castleRights;	// Castle rights at the current board state; Defaults to all allowed
	public int enPassantIndex=-1;	// En-passant index of current state
	public bool IsWhole => (!moveMade.IsNull) && pieceMoved != Piece.None && soundPlayed != 0;
	public Gamestate() {}
	public Gamestate(Move moveMade, Piece pieceMoved, Piece pieceTaken, int soundPlayed, int castlingRights) {
		this.moveMade = moveMade;
		this.pieceMoved = pieceMoved;
		this.pieceTaken = pieceTaken;
		this.soundPlayed = soundPlayed;
		this.castleRights = castlingRights;
	}

	public override string ToString() {
		return $"ID={ID}, move={moveMade}, pM={pieceMoved}, pT={pieceTaken}, enPassantIndex={enPassantIndex}, sound={soundPlayed}, castles={System.Convert.ToString(castleRights, 2)}, IsWhole={IsWhole}";
	}
}