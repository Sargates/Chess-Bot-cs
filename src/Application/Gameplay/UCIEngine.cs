/////////////////////////////////////////////////////////////////////
// Most of this is taken from Stockfish.NET
// The only real changes are to make it compatible with my engine
/////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using ChessBot.Helpers;
using ChessBot.Engine;

namespace ChessBot.Application;

public class UCIEngine {
	private const int MAX_TRIES = 200;
	private int _skillLevel;
	public int Depth;
	public UCISettings Settings;
	public int SkillLevel {
		get => _skillLevel;
		set {
			_skillLevel = value;
			Settings.SkillLevel = SkillLevel;
			setOption("Skill level", SkillLevel.ToString());
		}
	}
	public char color;
	private ProcessStartInfo _processStartInfo;
	private Process _process;

	public bool ShouldSearch;

	public UCIEngine(
			string pathToExe,
			int depth = 14,
			UCISettings? settings = null) {
		_processStartInfo = new ProcessStartInfo {
			FileName = FileHelper.GetResourcePath(pathToExe),
			UseShellExecute = false,
			RedirectStandardError = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true
		};
		_process = new Process {StartInfo = _processStartInfo};

		Depth = depth;
		Settings = (settings ?? (new UCISettings()));
	}

	public void Start() {
		
		_process.Start();
		ReadLine(); // Reads the buffer output when you launch stockfish
		SkillLevel = Settings.SkillLevel;

		foreach (var property in Settings.GetPropertiesAsDictionary()) {
			setOption(property.Key, property.Value);
		}
	}


	private void send(string command, int estimatedTime = 100) {
		WriteLine(command);
		Wait(estimatedTime);
	}

	private bool isReady() {
		send("isready");
		var tries = 0;
		while (tries < MAX_TRIES) {
			++tries;

			if (ReadLine() == "readyok") {
				return true;
			}
		}
		throw new Exception("Max tries Exceeded");
	}

	private void setOption(string name, string value) {
		send($"setoption name {name} value {value}");
		if (!isReady()) {
			throw new ApplicationException();
		}
	}

	private void startNewGame() {
		send("ucinewgame");
		if (!isReady()) {
			throw new ApplicationException();
		}
	}

	private void go() {
		send($"go depth {Depth}");
	}

	private void goTime(int time) {
		send($"go movetime {time}");
	}

	private List<string> readLineAsList() {
		var data = ReadLine();
		if (data == null) { data = ""; }
		return data.Split(' ').ToList();
	}

	public void SetPosition(string uciGameFormat) {
		startNewGame();
		send($"{uciGameFormat}");
	}

	public string GetFenPosition() {
		send("d");
		var tries = 0;
		while (true) {
			if (tries > MAX_TRIES) {
				throw new Exception("Max tries Exceeded");
			}

			var data = readLineAsList();
			if (data[0] == "Fen:") {
				return string.Join(" ", data.GetRange(1, data.Count - 1));
			}

			tries++;
		}
	}

	public string GetBestMove() {
		go();
		var tries = 0;
		while (true) {
			if (tries > MAX_TRIES) {
				throw new Exception("Max tries Exceeded");
			}

			var data = readLineAsList();

			if (data[0] == "bestmove") {
				if (data[1] == "(none)") {
					return "a1a1"; // Represents null move in my engine
				}

				return data[1];
			}

			tries++;
		}
	}

	public string GetBestMoveTime(int time = 1000) {
		goTime(time);
		var tries = 0;
		while (true) {
			if (tries > MAX_TRIES) {
				throw new Exception("Max tries Exceeded");
			}

			var data = readLineAsList();
			if (data[0] == "bestmove") {
				if (data[1] == "(none)") {
					return "a1a1";
				}

				return data[1];
			}
		}
	}

	public bool IsMoveCorrect(string moveValue) {
		send($"go depth 1 searchmoves {moveValue}");
		var tries = 0;
		while (true) {
			if (tries > MAX_TRIES) {
				throw new Exception("Max tries Exceeded");
			}

			var data = readLineAsList();
			if (data[0] == "bestmove") {
				if (data[1] == "(none)") {
					return false;
				}

				return true;
			}

			tries++;
		}
	}

	public Evaluation GetEvaluation() {
		Evaluation evaluation = new Evaluation();
		var fen = GetFenPosition();
		char compare;
		// fen sequence for white always contains w
		if (fen.Contains("w")) {
			compare = 'w';
		}
		else {
			compare = 'b';
		}

		// I'm not sure this is the good way to handle evaluation of position, but why not?
		// Another way we need to somehow limit engine depth? 
		goTime(10000);
		var tries = 0;
		while (true) {
			if (tries > MAX_TRIES) {
				throw new Exception("Max tries Exceeded");
			}

			var data = readLineAsList();
			if (data[0] == "info") {
				for (int i = 0; i < data.Count; i++) {
					if (data[i] == "score") {
						//don't use ternary operator here for readability
						int k;
						if (compare == 'w') {
							k = 1;
						}
						else {
							k = -1;
						}

						evaluation = new Evaluation(data[i + 1], Convert.ToInt32(data[i + 2]) * k);
					}
				}
			}

			if (data[0] == "bestmove") {
				return evaluation;
			}

			tries++;
		}
	}
	public void Wait(int millisecond) {
		this._process.WaitForExit(millisecond);
	}

	public void WriteLine(string command) {
		if (_process.StandardInput == null) {
			throw new NullReferenceException();
		}
		_process.StandardInput.WriteLine(command);
		_process.StandardInput.Flush();
	}

	public string? ReadLine() {
		if (_process.StandardOutput == null) {
			throw new NullReferenceException();
		}
		return _process.StandardOutput.ReadLine();
	}

	~UCIEngine() {
		//When process is going to be destructed => we are going to close stockfish process
		_process.Close();
	}
}
