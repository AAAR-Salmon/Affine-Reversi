﻿using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
	[SerializeField] private GameObject grid;
	[SerializeField] private Material gridMaterial;
	private (int x, int y) cursorPos = (0, 0);
	private DiskColor[,] arrayColor;
	private Dictionary<(int, int), GameObject> arrayDisk;
	private List<GameObject> listPlaceableHint;
	private int countBlackDisk;
	private int countWhiteDisk;
	[SerializeField] private Color colorDiskBlack = Color.black;
	[SerializeField] private Color colorDiskWhite = Color.white;
	[SerializeField] private Color colorCursorBlack = Color.black;
	[SerializeField] private Color colorCursorWhite = Color.white;
	[SerializeField] private float lineWeight = 0.07f;
	[SerializeField] private float gridBaseScale = 8.0f;
	[SerializeField] private AudioClip[] putDiskSE;
	private int fracX;
	private int fracY;
	private float offsetX;
	private float offsetY;
	private float unitLength;


	void Awake() {
		listPlaceableHint = new List<GameObject>();
	}

	// Start is called before the first frame update
	void Start() {
		ResizeBoard(ConfigurationSingleton.instance.fracX, ConfigurationSingleton.instance.fracY);
		Init();
	}

	// Update is called once per frame
	void Update() {
		// キー入力による移動
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			cursorPos.y = (cursorPos.y + 1) % fracY;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			cursorPos.y = (cursorPos.y + fracY - 1) % fracY;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			cursorPos.x = (cursorPos.x + fracX - 1) % fracX;
			cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			cursorPos.x = (cursorPos.x + 1) % fracX;
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

	void ResizeBoard(int fracX, int fracY) {
		this.fracX = fracX;
		this.fracY = fracY;

		unitLength = gridBaseScale / (Mathf.Max(fracX, fracY) + lineWeight);
		offsetX = -(fracX - 1.0f) / 2.0f * unitLength;
		offsetY = -(fracY - 1.0f) / 2.0f * unitLength;
		grid.transform.localScale = new Vector3(unitLength * (fracX + lineWeight), unitLength * (fracY + lineWeight), 1);
		diskPrefab.transform.localScale = new Vector3(unitLength * 0.8f, unitLength * 0.8f, 1);
		placeableHintPrefab.transform.localScale = new Vector3(unitLength * 0.3f, unitLength * 0.3f, 1);
		cursor.transform.localScale = new Vector3(unitLength * 0.4f, unitLength * 0.4f, 1);
		gridMaterial.SetInt("_FracX", fracX);
		gridMaterial.SetInt("_FracY", fracY);
		gridMaterial.SetFloat("_LineWeight", lineWeight);
	}

	void Init() {
		countBlackDisk = 0;
		countWhiteDisk = 0;
		turn = DiskColor.Black;
		arrayColor = new DiskColor[fracX, fracY];
		for (int i = 0; i < fracX; i++) {
			for (int j = 0; j < fracY; j++) {
				arrayColor[i, j] = DiskColor.None;
			}
		}
		arrayDisk = new Dictionary<(int, int), GameObject>();
		PlaceDisk((fracX - 1) / 2, (fracY - 1) / 2, DiskColor.Black, true);
		PlaceDisk((fracX - 1) / 2 + 1, (fracY - 1) / 2 + 1, DiskColor.Black, true);
		PlaceDisk((fracX - 1) / 2, (fracY - 1) / 2 + 1, DiskColor.White, true);
		PlaceDisk((fracX - 1) / 2 + 1, (fracY - 1) / 2, DiskColor.White, true);
		scoreBlackUI.GetComponent<Text>().text = countBlackDisk.ToString();
		scoreWhiteUI.GetComponent<Text>().text = countWhiteDisk.ToString();
		cursor.GetComponent<SpriteRenderer>().color = colorCursorBlack;
		cursor.transform.position = this.transform.position + Vector3FromInt3(cursorPos.x, cursorPos.y, -1);
		RefreshHint(turn);
	}

	void Clear() {
		arrayColor = null;
		foreach (var pair in arrayDisk) {
			Destroy(pair.Value);
		}
		arrayDisk = null;
	}

	void RefreshHint(DiskColor _c) {
		foreach (var obj in listPlaceableHint) {
			Destroy(obj);
		}
		listPlaceableHint.Clear();
		for (int i = 0; i < fracX; i++) {
			for (int j = 0; j < fracY; j++) {
				if (CountTurnoverOnPlace(i, j, _c) > 0) {
					listPlaceableHint.Add(Instantiate(placeableHintPrefab, this.transform.position + Vector3FromInt3(i, j, 0), Quaternion.identity, this.transform));
				}
			}
		}
	}

	Vector3 Vector3FromInt3(int _x, int _y, int _z) {
		return new Vector3(offsetX + unitLength * _x, offsetY + unitLength * _y, _z);
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

	void PlaceDisk(int _x, int _y, DiskColor _c, bool mute = false) {
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
		GameObject newDisk = Instantiate(diskPrefab, this.transform.position + Vector3FromInt3(_x, _y, 0), Quaternion.identity, this.transform);
		newDisk.GetComponent<SpriteRenderer>().color = color;
		arrayDisk.Add((_x, _y), newDisk);

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
					arrayDisk[(x, y)].GetComponent<SpriteRenderer>().color = color;
				}
			}
		}
		if (!mute) {
			this.GetComponent<AudioSource>().PlayOneShot(putDiskSE[Random.Range(0, putDiskSE.GetLength(0))]);
		}
	}

	bool InBoardArea(int _x, int _y) {
		return 0 <= _x && _x < fracX && 0 <= _y && _y < fracY;
	}

	public void OnClickClearButton() {
		Clear();
		Init();
	}

	public void OnClickExitButton() {
		Clear();
		SceneManager.LoadSceneAsync("TitleScene", LoadSceneMode.Single);
	}
}
