namespace ChessBot.Application;
class Program {
	public static void Main() {
		MainController gaming = MainController.Instance;
		gaming.MainLoop();
	}
}
