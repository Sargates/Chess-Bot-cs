namespace ChessBot.Helpers; //* Copied from SebLague (mostly)
public static class ConsoleHelper {
	public static void WriteLine(ConsoleColor col = ConsoleColor.White) { WriteLine("", col); }
	// "\x1b[38;2;" + r + ";" + g + ";" + b + "m" SET FOREGROUND
	// "\x1b[48;2;" + r + ";" + g + ";" + b + "m" SET BACKGROUND
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
