namespace ChessBot.Helpers; //* Copied from SebLague (mostly)
public static class ConsoleHelper {
	public static void WriteLine(string? msg, ConsoleColor col = ConsoleColor.White) {
		Console.ForegroundColor = col;
		Console.WriteLine(msg);
		Console.ResetColor();
	}
	public static void Write(string? msg, ConsoleColor col = ConsoleColor.White) {
		Console.ForegroundColor = col;
		Console.Write(msg);
		Console.ResetColor();
	}
}
