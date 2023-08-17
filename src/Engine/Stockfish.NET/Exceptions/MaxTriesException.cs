using System;

namespace Stockfish.NET;
public class MaxTriesException: Exception {
	public  MaxTriesException(string msg="") : base(msg) { }
}
