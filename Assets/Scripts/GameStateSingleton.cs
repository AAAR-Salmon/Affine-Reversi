public class GameStateSingleton {
	private static GameStateSingleton _instance;

	public int fracX;
	public int fracY;

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
	}
}
