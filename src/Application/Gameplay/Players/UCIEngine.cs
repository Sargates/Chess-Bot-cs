/////////////////////////////////////////////////////////////////////
// Most of this is taken from Stockfish.NET (see license)
// The only real changes are to make it compatible with my engine
/////////////////////////////////////////////////////////////////////

using System.Diagnostics;
using ChessBot.Helpers;
using ChessBot.Engine;

namespace ChessBot.Application;

public class UCIEngine {
	private const int MAX_TRIES = 300;
	public int Depth;
	public UCISettings Settings;
	private int _elo;
	public int Elo {
		get => _elo;
		set {
			_elo = value;
			Settings.Elo = Elo;
			setOption("UCI_Elo", Elo.ToString());
		}
	}
	public void SetElo(int rating) { Elo = rating; Console.WriteLine($"Fish Elo set to {Elo}"); }
	public char color;
	private ProcessStartInfo _processStartInfo;
	private Process _process;

	public bool ShouldSearch;

	public UCIEngine(
			string pathToExe,
			int depth = 18,
			UCISettings? settings = null) {
		_processStartInfo = new ProcessStartInfo {
			FileName = FileHelper.GetResourcePath(pathToExe),
			CreateNoWindow = true,
			RedirectStandardError = true,
			RedirectStandardInput = true,
			RedirectStandardOutput = true,
			UseShellExecute = false,
		};
		_process = new Process {StartInfo = _processStartInfo};

		Depth = depth;
		Settings = (settings ?? (new UCISettings()));
		// Settings = (settings ?? (new UCISettings { UseNNUE = false }));
	}

	public void Start() {
		
		_process.Start();
		ReadLine(); // Reads the buffer output when you launch stockfish
		Elo = Settings.Elo;

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
		// Console.WriteLine($"Sending: setoption name {name} value {value}");
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

	public Dictionary<string, int> GoPerft(int depth) {
		send($"go perft {depth}");
		Dictionary<string, int> moves = new Dictionary<string, int>();

		var tries = 0;
		while (true) {
			if (tries > MAX_TRIES) {
				throw new Exception("Max tries Exceeded");
			}

			var data = readLineAsList();

			if (data.Count <= 1) {
				return moves;
			}
			moves.Add(data[0].Replace(":", ""), int.Parse(data[1]));
			tries++;
		}
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

			string? response = ReadLine();
			if (response==null) {
				tries++;
				continue;
			}
			// Console.WriteLine(response);
			List<string> parsedData = response.Split(" ").ToList();
			if (parsedData[0] == "bestmove") {
				if (parsedData[1] == "(none)") {
					return "a1a1";
				}

				return parsedData[1];
			}
			tries++;
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

	public string GetBoardVisual() {
		send("d");
		var board = "";
		var lines = 0;
		var tries = 0;
		while (lines < 17) {
			if (tries > MAX_TRIES) {
				throw new Exception("Max tries Exceeded");
			}

			var data = ReadLine();
			if (data == null) { data = ""; }
			if (data.Contains("+") || data.Contains("|")) {
				lines++;
				board += $"{data}\n";
			}

			tries++;
		}

		return board;
	}

	// public Evaluation GetEvaluation() {
	// 	Evaluation evaluation = new Evaluation();
	// 	var fen = GetFenPosition();
	// 	char compare;
	// 	// fen sequence for white always contains w
	// 	if (fen.Contains("w")) {
	// 		compare = 'w';
	// 	}
	// 	else {
	// 		compare = 'b';
	// 	}

	// 	// I'm not sure this is the good way to handle evaluation of position, but why not?
	// 	// Another way we need to somehow limit engine depth? 
	// 	goTime(10000);
	// 	var tries = 0;
	// 	while (true) {
	// 		if (tries > MAX_TRIES) {
	// 			throw new Exception("Max tries Exceeded");
	// 		}

	// 		var data = readLineAsList();
	// 		if (data[0] == "info") {
	// 			for (int i = 0; i < data.Count; i++) {
	// 				if (data[i] == "score") {
	// 					//don't use ternary operator here for readability
	// 					int k;
	// 					if (compare == 'w') {
	// 						k = 1;
	// 					}
	// 					else {
	// 						k = -1;
	// 					}

	// 					evaluation = new Evaluation(data[i + 1], Convert.ToInt32(data[i + 2]) * k);
	// 				}
	// 			}
	// 		}

	// 		if (data[0] == "bestmove") {
	// 			return evaluation;
	// 		}

	// 		tries++;
	// 	}
	// }
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
