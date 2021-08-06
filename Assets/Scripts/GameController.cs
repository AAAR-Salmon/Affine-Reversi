using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DiskColor {
	None,
	Black,
	White
}
public class GameController : MonoBehaviour {
	private DiskColor turn = DiskColor.Black;
	[SerializeField] private GameObject cursor;
	[SerializeField] private GameObject scoreBlackUI;
	[SerializeField] private GameObject scoreWhiteUI;
	[SerializeField] private GameObject diskPrefab;
	[SerializeField] private GameObject placeableHintPrefab;
	private (int x, int y) cursorPos = (0, 0);
	private DiskColor[,] arrayColor;
	private GameObject[,] arrayDisk;
	private List<GameObject> listPlaceableHint;
	[SerializeField] private const int rowSize = 8;
	[SerializeField] private const int columnSize = 8;
	private int countBlackDisk = 0;
	private int countWhiteDisk = 0;
	[SerializeField] private Color colorDiskBlack = Color.black;
	[SerializeField] private Color colorCursorBlack = Color.black;
	[SerializeField] private Color colorDiskWhite = Color.white;
	[SerializeField] private Color colorCursorWhite = Color.white;
	[SerializeField] private float offsetX = -3.5f;
	[SerializeField] private float offsetY = -3.5f;
	[SerializeField] private float unitX = 1.0f;
	[SerializeField] private float unitY = 1.0f;

	private void Awake() {
		turn = DiskColor.Black;
		arrayColor = new DiskColor[rowSize, columnSize];
		arrayDisk = new GameObject[rowSize, columnSize];
		listPlaceableHint = new List<GameObject>();
		for (int i = 0; i < rowSize; i++) {
			for (int j = 0; j < columnSize; j++) {
				arrayColor[i, j] = DiskColor.None;
				arrayDisk[i, j] = null;
			}
		}
		PlaceDisk(3, 3, DiskColor.Black);
		PlaceDisk(4, 4, DiskColor.Black);
		PlaceDisk(3, 4, DiskColor.White);
		PlaceDisk(4, 3, DiskColor.White);
	}

	// Start is called before the first frame update
	void Start() {
		cursor.GetComponent<SpriteRenderer>().color = colorCursorBlack;
		cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		RefreshHint(turn);
	}

	// Update is called once per frame
	void Update() {
		// キー入力による移動
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			cursorPos.y = (cursorPos.y + 1) % rowSize;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			cursorPos.y = (cursorPos.y + rowSize - 1) % rowSize;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			cursorPos.x = (cursorPos.x + columnSize - 1) % columnSize;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			cursorPos.x = (cursorPos.x + 1) % columnSize;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}

		// 石を置いて色を変更する
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
			if (CountTurnoverOnPlace(cursorPos.x, cursorPos.y, turn) > 0) {
				PlaceDisk(cursorPos.x, cursorPos.y, turn);

				RefreshHint(turn == DiskColor.Black ? DiskColor.White : DiskColor.Black);
				if (listPlaceableHint.Count == 0) {
					RefreshHint(turn);
					if (listPlaceableHint.Count == 0) {
						// 両者置けない場合
						// TODO: ゲーム終了処理
					} else {
						// パスが起こり自分の手番が続く場合
					}
				} else {
					// パスが起こらず相手に手番が移る場合
					turn = (turn == DiskColor.Black ? DiskColor.White : DiskColor.Black);
				}
				scoreBlackUI.GetComponent<Text>().text = countBlackDisk.ToString();
				scoreWhiteUI.GetComponent<Text>().text = countWhiteDisk.ToString();
				cursor.GetComponent<SpriteRenderer>().color = (turn == DiskColor.Black ? colorCursorBlack : colorCursorWhite);
			}
		}
	}

	void RefreshHint(DiskColor _c) {
		foreach (var obj in listPlaceableHint) {
			Destroy(obj);
		}
		listPlaceableHint.Clear();
		for (int i = 0; i < rowSize; i++) {
			for (int j = 0; j < columnSize; j++) {
				if (CountTurnoverOnPlace(i, j, _c) > 0) {
					listPlaceableHint.Add(Instantiate(placeableHintPrefab, this.transform.position + Vector3FromInt3(i, j, 0), Quaternion.identity, this.transform));
				}
			}
		}
	}

	Vector3 Vector3FromInt3(int _x, int _y, int _z) {
		return new Vector3(offsetX + unitX * _x, offsetY + unitY * _y, _z);
	}

	int CountTurnoverOnPlace(int _x, int _y, DiskColor _c) {
		if (!InBoardArea(_x, _y)) {
			return 0;
		}
		if (arrayColor[_x, _y] != DiskColor.None) {
			return 0;
		}
		if (_c == DiskColor.None) {
			return 0;
		}
		int _result = 0;
		(int, int)[] dpos = new (int, int)[8] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };
		foreach (var (dx, dy) in dpos) {
			int x = _x + dx;
			int y = _y + dy;
			int distance = 1;
			while (InBoardArea(x, y) && arrayColor[x, y] != DiskColor.None && arrayColor[x, y] != _c) {
				x += dx;
				y += dy;
				distance++;
			}
			if (InBoardArea(x, y) && arrayColor[x, y] == _c) {
				_result += distance - 1;
			}
		}
		return _result;
	}

	void PlaceDisk(int _x, int _y, DiskColor _c) {
		if (_c == DiskColor.None) {
			return;
		}
		Color color = Color.magenta;
		switch (_c) {
		case DiskColor.None:
			break;
		case DiskColor.Black:
			color = colorDiskBlack;
			break;
		case DiskColor.White:
			color = colorDiskWhite;
			break;
		default:
			break;
		}

		int countTurnover = CountTurnoverOnPlace(_x, _y, _c);
		if (_c == DiskColor.Black) {
			countBlackDisk += countTurnover + 1;
			countWhiteDisk -= countTurnover;
		} else {
			countWhiteDisk += countTurnover + 1;
			countBlackDisk -= countTurnover;
		}

		arrayColor[_x, _y] = _c;
		arrayDisk[_x, _y] = Instantiate(diskPrefab, this.transform.position + Vector3FromInt3(_x, _y, 0), Quaternion.identity, this.transform);
		arrayDisk[_x, _y].GetComponent<SpriteRenderer>().color = color;

		(int, int)[] dpos = new (int, int)[8] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };
		foreach (var (dx, dy) in dpos) {
			int x = _x + dx;
			int y = _y + dy;
			while (InBoardArea(x, y) && arrayColor[x, y] != DiskColor.None && arrayColor[x, y] != _c) {
				x += dx;
				y += dy;
			}
			if (InBoardArea(x, y) && arrayColor[x, y] == _c) {
				while ((x -= dx, y -= dy) != (_x, _y)) {
					arrayColor[x, y] = _c;
					arrayDisk[x, y].GetComponent<SpriteRenderer>().color = color;
				}
			}
		}
	}

	bool InBoardArea(int _x, int _y) {
		return 0 <= _x && _x < rowSize && 0 <= _y && _y < columnSize;
	}
}
