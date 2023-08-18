namespace ChessBot.Application;
class Program {
	public static void Main(string[] args) {
		MainController gaming = MainController.Instance;
		gaming.MainLoop();
	}
}
