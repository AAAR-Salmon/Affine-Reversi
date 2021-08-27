using Enum;

public class GameStateSingleton {
	private static GameStateSingleton _instance;

	public int FracX;
	public int FracY;
	public DiskColor PlayerColor;
	public string PlayerName;
	public string OppositePlayerName;

	public static GameStateSingleton Instance => _instance ??= new GameStateSingleton();

	private GameStateSingleton() {
		FracX = 8;
		FracY = 8;
		PlayerColor = DiskColor.Black;
		PlayerName = "Player";
		OppositePlayerName = "CPU";
	}
}