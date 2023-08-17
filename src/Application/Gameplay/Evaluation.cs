namespace ChessBot.Application;
public class Evaluation {
	public string Type { get; set; }
	public int Value { get; set; }

	public Evaluation() {
		Type = "";
	}

	public Evaluation(string type, int value) {
		Type = type;
		Value = value;
	}
}
