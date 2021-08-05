using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DiskColor {
	None,
	Black,
	White
}
public class GameController : MonoBehaviour {
	private DiskColor turn = DiskColor.Black;
	private GameObject cursor;
	private (int x, int y) cursorPos = (0, 0);
	private GameObject diskPrefab;
	private GameObject placeableHintPrefab;
	private DiskColor[,] arrayColor;
	private GameObject[,] arrayDisk;
	private List<GameObject> arrayPlaceableHint;
	private const int rowSize = 8;
	private const int columnSize = 8;
	[SerializeField] private Color colorBlack = Color.black;
	[SerializeField] private Color colorWhite = Color.white;
	[SerializeField] private float offsetX = -3.5f;
	[SerializeField] private float offsetY = -3.5f;
	[SerializeField] private float unitX = 1.0f;
	[SerializeField] private float unitY = 1.0f;

	private void Awake() {
		diskPrefab = (GameObject) Resources.Load("Prefabs/DiskPrefab");
		placeableHintPrefab = (GameObject) Resources.Load("Prefabs/PlaceableHintPrefab");
		cursor = this.transform.Find("Cursor").gameObject;
		arrayColor = new DiskColor[rowSize, columnSize];
		arrayDisk = new GameObject[rowSize, columnSize];
		arrayPlaceableHint = new List<GameObject>();
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
		turn = DiskColor.Black;
		cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		RefreshHint(turn);
	}

	// Update is called once per frame
	void Update() {
		// キー入力による移動
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			cursorPos.y += 1;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			cursorPos.y -= 1;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			cursorPos.x -= 1;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			cursorPos.x += 1;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}

		// 石を置いて色を変更する
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
			if (IsPlaceableAt(cursorPos.x, cursorPos.y, turn)) {
				PlaceDisk(cursorPos.x, cursorPos.y, turn);
				turn = (turn == DiskColor.Black ? DiskColor.White : DiskColor.Black);
				cursor.GetComponent<SpriteRenderer>().color = (turn == DiskColor.Black ? colorBlack : colorWhite);
				RefreshHint(turn);
			}
		}
	}

	void RefreshHint(DiskColor _c) {
		foreach (var obj in arrayPlaceableHint) {
			Destroy(obj);
		}
		arrayPlaceableHint.Clear();
		for (int i = 0; i < rowSize; i++) {
			for (int j = 0; j < columnSize; j++) {
				if (IsPlaceableAt(i, j, _c)) {
					arrayPlaceableHint.Add(Instantiate(placeableHintPrefab, this.transform.position + Vector3FromInt3(i, j, 0), Quaternion.identity, this.transform));
				}
			}
		}
	}

	Vector3 Vector3FromInt3(int _x, int _y, int _z) {
		return new Vector3(offsetX + unitX * _x, offsetY + unitY * _y, _z);
	}

	bool IsPlaceableAt(int _x, int _y, DiskColor _c) {
		if (!InBoardArea(_x, _y)) {
			return false;
		}
		if (arrayColor[_x, _y] != DiskColor.None) {
			return false;
		}
		if (_c == DiskColor.None) {
			return false;
		}
		(int, int)[] dpos = new (int, int)[8] { (-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1) };
		foreach (var (dx, dy) in dpos) {
			int x = _x + dx;
			int y = _y + dy;
			int distance = 1;
			while (InBoardArea(x, y)) {
				if (arrayColor[x, y] == DiskColor.None) {
					break;
				}
				if (arrayColor[x, y] == _c) {
					if (distance == 1) {
						break;
					} else {
						return true;
					}
				}
				x += dx;
				y += dy;
				distance++;
			}
		}
		return false;
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
			color = colorBlack;
			break;
		case DiskColor.White:
			color = colorWhite;
			break;
		default:
			break;
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
