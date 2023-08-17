// Ref: Stockfish.NET (see license)
using System.Collections.Generic;

namespace ChessBot.Application;
public class UCISettings {
	public int Contempt;
	public int Threads;
	public bool Ponder;
	public int MultiPV;
	public bool LimitStrength;
	public int Elo;
	public int SkillLevel;
	public int MoveOverhead;
	public int SlowMover;
	public bool UCIChess960;

	public UCISettings(
		int contempt = 0,
		int threads = 0,
		bool ponder = false,
		int multiPV = 1,
		bool limitStrength = true,
		int elo = 3000,
		int skillLevel = 20,
		int moveOverhead = 30,
		int slowMover = 10,
		bool uciChess960 = false
	) {
		Contempt = contempt;
		Ponder = ponder;
		Threads = threads;
		LimitStrength = limitStrength;
		Elo = elo;
		MultiPV = multiPV;
		SkillLevel = skillLevel;
		MoveOverhead = moveOverhead;
		SlowMover = slowMover;
		UCIChess960 = uciChess960;
	}

	public Dictionary<string, string> GetPropertiesAsDictionary() {
		return new Dictionary<string, string> {
			["Contempt"] = Contempt.ToString(),
			["Threads"] = Threads.ToString(),
			["Ponder"] = Ponder.ToString(),
			["MultiPV"] = MultiPV.ToString(),
			["UCI_LimitStrength"] = LimitStrength.ToString(),
			["UCI_Elo"] = Elo.ToString(),
			// ["Skill Level"] = SkillLevel.ToString(),
			["Move Overhead"] = MoveOverhead.ToString(),
			["Slow Mover"] = SlowMover.ToString(),
			["UCI_Chess960"] = UCIChess960.ToString(),
		};
	}
}
