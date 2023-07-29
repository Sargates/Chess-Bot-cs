namespace ChessBot.Helpers {
	public static class ConsoleHelper
	{
		public static void WriteLine(string msg, ConsoleColor col = ConsoleColor.White) {
			Console.ForegroundColor = col;
			Console.WriteLine(msg);
			Console.ResetColor();
		}
		public static void Write(string msg, ConsoleColor col = ConsoleColor.White) {
			Console.ForegroundColor = col;
			Console.Write(msg);
			Console.ResetColor();
		}
	}
}
