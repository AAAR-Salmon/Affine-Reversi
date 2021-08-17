public enum DiskColor {
	None,
	Black,
	White
}

public class GameStateSingleton {
	private static GameStateSingleton _instance;

	public int fracX;
	public int fracY;
	public int blackScore;
	public int whiteScore;
	public DiskColor playerColor;

	public static GameStateSingleton instance {
		get {
			if (_instance == null) {
				_instance = new GameStateSingleton();
			}
			return _instance;
		}
	}

	private GameStateSingleton() {
		fracX = 8;
		fracY = 8;
		blackScore = 0;
		whiteScore = 0;
		playerColor = DiskColor.Black;
	}
}
