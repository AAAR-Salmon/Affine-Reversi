public class GameStateSingleton {
	private static GameStateSingleton _instance;

	public int fracX;
	public int fracY;
	public DiskColor playerColor;
	public string playerName;

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
		playerColor = DiskColor.Black;
		playerName = "player";
	}
}
