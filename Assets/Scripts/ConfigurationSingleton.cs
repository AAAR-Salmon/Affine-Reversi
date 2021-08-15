public class ConfigurationSingleton {
	private static ConfigurationSingleton _instance;

	public int fracX;
	public int fracY;
	public int blackScore;
	public int whiteScore;

	public static ConfigurationSingleton instance {
		get {
			if (_instance == null) {
				_instance = new ConfigurationSingleton();
			}
			return _instance;
		}
	}

	private ConfigurationSingleton() {
		fracX = 8;
		fracY = 8;
		blackScore = 0;
		whiteScore = 0;
	}
}
