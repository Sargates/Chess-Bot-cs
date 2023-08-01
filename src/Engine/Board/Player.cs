using Raylib_cs;


namespace ChessBot.Engine {
	public readonly struct Player {
		public readonly char color;
		public readonly char forward;
		// public readonly char ;

		public Player(char color) {
			this.color = color;
		}

		public void HandleInput() {}
	}
}