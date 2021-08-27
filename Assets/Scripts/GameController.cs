using Enum;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Enum.DiskColor;

public class GameController : MonoBehaviourPunCallbacks {
	private DiskColor _currentTurn = Black;
	private bool _isGameFinished;
	private DiskColor _playerSurrenderColor = None;
	[SerializeField] private GameObject cursor;
	[SerializeField] private TextMeshProUGUI scoreBlackUI;
	[SerializeField] private TextMeshProUGUI scoreWhiteUI;
	[SerializeField] private TextMeshProUGUI playerNameBlackUi;
	[SerializeField] private TextMeshProUGUI playerNameWhiteUi;
	[SerializeField] private GameObject diskPrefab;
	[SerializeField] private GameObject placeableHintPrefab;
	[SerializeField] private GameObject grid;
	private MaterialPropertyBlock _gridMaterialPropertyBlock;
	private (int x, int y) _cursorPos = (0, 0);
	private DiskColor[,] _arrayColor;
	private Dictionary<(int, int), GameObject> _dictionaryDiskInstance;
	private List<GameObject> _listPlaceableHint;
	private List<(int, int)> _listPlaceablePosition;
	private int _countBlackDisk;
	private int _countWhiteDisk;
	[SerializeField] private Color colorDiskBlack = Color.black;
	[SerializeField] private Color colorDiskWhite = Color.white;
	[SerializeField] private Color colorCursorBlack = Color.black;
	[SerializeField] private Color colorCursorWhite = Color.white;
	[SerializeField] private float lineWeight = 0.07f;
	[SerializeField] private float gridBaseScale = 8.0f;
	[SerializeField] private AudioClip[] putDiskSoundEffect;
	private int _fracX;
	private int _fracY;
	private float _offsetX;
	private float _offsetY;
	private float _unitLength;
	[SerializeField] private GameObject clearButton;
	[SerializeField] private GameObject exitButton;
	[SerializeField] private GameObject surrenderButton;
	[SerializeField] private GameObject twitterShareButton;
	[SerializeField] private Camera mainCamera;
	private PhotonView _photonView;
	private static readonly int FracX = Shader.PropertyToID("_FracX");
	private static readonly int FracY = Shader.PropertyToID("_FracY");
	private static readonly int LineWeight = Shader.PropertyToID("_LineWeight");

#if UNITY_WEBGL
	[DllImport("__Internal")]
	private static extern void DownloadPNG(byte[] bytes, int size);
#endif

	private void Awake() {
		_listPlaceablePosition = new List<(int, int)>();
		_listPlaceableHint = new List<GameObject>();
	}

	// Start is called before the first frame update
	private void Start() {
		_photonView = PhotonView.Get(this);
		GameStateSingleton.Instance.PlayerColor = (DiskColor) PhotonNetwork.LocalPlayer.CustomProperties["color"];

		clearButton.SetActive(PhotonNetwork.OfflineMode);
		exitButton.SetActive(PhotonNetwork.OfflineMode);
		surrenderButton.SetActive(!PhotonNetwork.OfflineMode);
		
		if (!PhotonNetwork.OfflineMode) {
			GameStateSingleton.Instance.OppositePlayerName = PhotonNetwork.PlayerListOthers[0].NickName;
		}

		switch (GameStateSingleton.Instance.PlayerColor) {
		case Black:
			playerNameBlackUi.text = GameStateSingleton.Instance.PlayerName;
			playerNameWhiteUi.text = GameStateSingleton.Instance.OppositePlayerName;
			break;
		case White:
			playerNameBlackUi.text = GameStateSingleton.Instance.OppositePlayerName;
			playerNameWhiteUi.text = GameStateSingleton.Instance.PlayerName;
			break;
		}

		if (!PhotonNetwork.OfflineMode && !PhotonNetwork.IsMasterClient) {
			GameStateSingleton.Instance.FracX = (int) PhotonNetwork.CurrentRoom.CustomProperties["fracX"];
			GameStateSingleton.Instance.FracY = (int) PhotonNetwork.CurrentRoom.CustomProperties["fracY"];
		}

		ResizeBoard(GameStateSingleton.Instance.FracX, GameStateSingleton.Instance.FracY);
		Init();
	}

	// Update is called once per frame
	private void Update() {
		// キー入力による移動
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			_cursorPos.y = (_cursorPos.y + 1) % _fracY;
			cursor.transform.position = this.transform.position + Vector3FromInt3(_cursorPos.x, _cursorPos.y, -1);
		}

		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			_cursorPos.y = (_cursorPos.y + _fracY - 1) % _fracY;
			cursor.transform.position = this.transform.position + Vector3FromInt3(_cursorPos.x, _cursorPos.y, -1);
		}

		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			_cursorPos.x = (_cursorPos.x + _fracX - 1) % _fracX;
			cursor.transform.position = this.transform.position + Vector3FromInt3(_cursorPos.x, _cursorPos.y, -1);
		}

		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			_cursorPos.x = (_cursorPos.x + 1) % _fracX;
			cursor.transform.position = this.transform.position + Vector3FromInt3(_cursorPos.x, _cursorPos.y, -1);
		}

		if (_isGameFinished) {
			return;
		}

		if (_currentTurn == GameStateSingleton.Instance.PlayerColor) {
			// 石を置いて色を変更する
			if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
				if (CountTurnoverOnPlace(_cursorPos.x, _cursorPos.y, _currentTurn) > 0) {
					_photonView.RPC(nameof(PlaceDisk), RpcTarget.All, _cursorPos.x, _cursorPos.y, _currentTurn, false);
					_photonView.RPC(nameof(ChangeTurn), RpcTarget.All);
				}
			} else if (Input.GetMouseButtonDown(0)) {
				Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
				RaycastHit2D raycastHit = Physics2D.Raycast(ray.origin, ray.direction);
				if (raycastHit.collider is null) {
					return;
				}

				var lossyScale = grid.transform.lossyScale;
				var position = this.transform.position;
				int x = Mathf.FloorToInt((lossyScale.x / 2.0f + raycastHit.point.x - position.x) / _unitLength);
				int y = Mathf.FloorToInt((lossyScale.y / 2.0f + raycastHit.point.y - position.y) / _unitLength);
				if (CountTurnoverOnPlace(x, y, _currentTurn) > 0) {
					_photonView.RPC(nameof(PlaceDisk), RpcTarget.All, x, y, _currentTurn, false);
					_photonView.RPC(nameof(ChangeTurn), RpcTarget.All);
				}
			}
		} else if (PhotonNetwork.OfflineMode) {
			var (x, y) = _listPlaceablePosition.RandomElement();
			PlaceDisk(x, y, _currentTurn);
			ChangeTurn();
		}
	}

	private void ResizeBoard(int fracX, int fracY) {
		_fracX = fracX;
		_fracY = fracY;

		_unitLength = gridBaseScale / (Mathf.Max(fracX, fracY) + lineWeight);
		_offsetX = -(fracX - 1.0f) / 2.0f * _unitLength;
		_offsetY = -(fracY - 1.0f) / 2.0f * _unitLength;
		grid.transform.localScale =
			new Vector3(_unitLength * (fracX + lineWeight), _unitLength * (fracY + lineWeight), 1);
		diskPrefab.transform.localScale = new Vector3(_unitLength * 0.8f, _unitLength * 0.8f, 1);
		placeableHintPrefab.transform.localScale = new Vector3(_unitLength * 0.3f, _unitLength * 0.3f, 1);
		cursor.transform.localScale = new Vector3(_unitLength * 0.4f, _unitLength * 0.4f, 1);
		_gridMaterialPropertyBlock = new MaterialPropertyBlock();
		_gridMaterialPropertyBlock.SetInt(FracX, fracX);
		_gridMaterialPropertyBlock.SetInt(FracY, fracY);
		_gridMaterialPropertyBlock.SetFloat(LineWeight, lineWeight);
		grid.GetComponent<SpriteRenderer>().SetPropertyBlock(_gridMaterialPropertyBlock);
	}

	private void Init() {
		_isGameFinished = false;
		_countBlackDisk = 0;
		_countWhiteDisk = 0;
		_currentTurn = Black;
		_arrayColor = new DiskColor[_fracX, _fracY];
		for (int i = 0; i < _fracX; i++) {
			for (int j = 0; j < _fracY; j++) {
				_arrayColor[i, j] = None;
			}
		}

		_dictionaryDiskInstance = new Dictionary<(int, int), GameObject>();
		PlaceDisk((_fracX - 1) / 2, (_fracY - 1) / 2, Black, true);
		PlaceDisk((_fracX - 1) / 2 + 1, (_fracY - 1) / 2 + 1, Black, true);
		PlaceDisk((_fracX - 1) / 2, (_fracY - 1) / 2 + 1, White, true);
		PlaceDisk((_fracX - 1) / 2 + 1, (_fracY - 1) / 2, White, true);
		scoreBlackUI.text = _countBlackDisk.ToString();
		scoreWhiteUI.text = _countWhiteDisk.ToString();
		cursor.GetComponent<SpriteRenderer>().color = colorCursorBlack;
		cursor.transform.position = this.transform.position + Vector3FromInt3(_cursorPos.x, _cursorPos.y, -1);
		RefreshPlaceablePosition(_currentTurn);
		RefreshPlaceableHint(_currentTurn);
		twitterShareButton.SetActive(false);
	}

	private void Clear() {
		_arrayColor = null;
		foreach (var pair in _dictionaryDiskInstance) {
			Destroy(pair.Value);
		}

		_dictionaryDiskInstance = null;
	}


	private void RefreshPlaceablePosition(DiskColor diskColor) {
		_listPlaceablePosition.Clear();
		for (int x = 0; x < _fracX; x++) {
			for (int y = 0; y < _fracY; y++) {
				if (CountTurnoverOnPlace(x, y, diskColor) > 0) {
					_listPlaceablePosition.Add((x, y));
				}
			}
		}
	}

	private void RefreshPlaceableHint(DiskColor turn) {
		foreach (var obj in _listPlaceableHint) {
			Destroy(obj);
		}

		_listPlaceableHint.Clear();
		if (turn != GameStateSingleton.Instance.PlayerColor) {
			return;
		}

		foreach (var (x, y) in _listPlaceablePosition) {
			_listPlaceableHint.Add(Instantiate(placeableHintPrefab,
				this.transform.position + Vector3FromInt3(x, y, 0),
				Quaternion.identity,
				this.transform));
		}
	}

	private Vector3 Vector3FromInt3(int x, int y, int z) {
		return new Vector3(_offsetX + _unitLength * x, _offsetY + _unitLength * y, z);
	}

	private int CountTurnoverOnPlace(int x, int y, DiskColor diskColor) {
		if (!InBoardArea(x, y)) {
			return 0;
		}

		if (_arrayColor[x, y] != None) {
			return 0;
		}

		if (diskColor == None) {
			return 0;
		}

		int result = 0;
		(int, int)[] dPos = {(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1)};
		foreach (var (dx, dy) in dPos) {
			int i = x + dx;
			int j = y + dy;
			int distance = 1;
			while (InBoardArea(i, j) && _arrayColor[i, j] != None && _arrayColor[i, j] != diskColor) {
				i += dx;
				j += dy;
				distance++;
			}

			if (InBoardArea(i, j) && _arrayColor[i, j] == diskColor) {
				result += distance - 1;
			}
		}

		return result;
	}

	[PunRPC]
	private void PlaceDisk(int x, int y, DiskColor diskColor, bool mute = false) {
		if (diskColor == None) {
			return;
		}

		Color color = Color.magenta;
		switch (diskColor) {
		case Black:
			color = colorDiskBlack;
			break;
		case White:
			color = colorDiskWhite;
			break;
		}

		int countTurnover = CountTurnoverOnPlace(x, y, diskColor);
		if (diskColor == Black) {
			_countBlackDisk += countTurnover + 1;
			_countWhiteDisk -= countTurnover;
		} else {
			_countWhiteDisk += countTurnover + 1;
			_countBlackDisk -= countTurnover;
		}

		_arrayColor[x, y] = diskColor;
		GameObject newDisk = Instantiate(diskPrefab,
			this.transform.position + Vector3FromInt3(x, y, 0),
			Quaternion.identity,
			this.transform);
		newDisk.GetComponent<SpriteRenderer>().color = color;
		_dictionaryDiskInstance.Add((x, y), newDisk);

		(int, int)[] dPos = {(-1, -1), (-1, 0), (-1, 1), (0, -1), (0, 1), (1, -1), (1, 0), (1, 1)};
		foreach (var (dx, dy) in dPos) {
			int i = x + dx;
			int j = y + dy;
			while (InBoardArea(i, j) && _arrayColor[i, j] != None && _arrayColor[i, j] != diskColor) {
				i += dx;
				j += dy;
			}

			if (InBoardArea(i, j) && _arrayColor[i, j] == diskColor) {
				while ((i -= dx, j -= dy) != (x, y)) {
					_arrayColor[i, j] = diskColor;
					_dictionaryDiskInstance[(i, j)].GetComponent<SpriteRenderer>().color = color;
				}
			}
		}

		if (!mute) {
			this.GetComponent<AudioSource>().PlayOneShot(putDiskSoundEffect.RandomElement());
		}
	}

	[PunRPC]
	private void ChangeTurn() {
		_currentTurn = NextTurn(_currentTurn);
	}

	private DiskColor NextTurn(DiskColor turn) {
		RefreshPlaceablePosition(turn == Black ? White : Black);
		if (_listPlaceablePosition.Count == 0) {
			RefreshPlaceablePosition(turn);
			if (_listPlaceablePosition.Count == 0) {
				// 両者置けない場合
				FinishGame();
			} else {
				// パスが起こり自分の手番が続く場合
				;
			}
		} else {
			// パスが起こらず相手に手番が移る場合
			turn = (turn == Black ? White : Black);
		}

		scoreBlackUI.text = _countBlackDisk.ToString();
		scoreWhiteUI.text = _countWhiteDisk.ToString();
		cursor.GetComponent<SpriteRenderer>().color = (turn == Black ? colorCursorBlack : colorCursorWhite);
		RefreshPlaceableHint(turn);
		return turn;
	}

	private bool InBoardArea(int x, int y) {
		return 0 <= x && x < _fracX && 0 <= y && y < _fracY;
	}

	public void OnClickClearButton() {
		Clear();
		Init();
	}

	public void OnClickExitButton() {
		Clear();
		PhotonNetwork.Disconnect();
		SceneManager.LoadSceneAsync("TitleScene", LoadSceneMode.Single);
	}

	public void OnClickSurrenderButton() {
		_photonView.RPC(nameof(FinishGame), RpcTarget.All, GameStateSingleton.Instance.PlayerColor);
	}

	[PunRPC]
	private void FinishGame(DiskColor playerSurrenderColor = None) {
		_playerSurrenderColor = playerSurrenderColor;
		_isGameFinished = true;
		twitterShareButton.SetActive(true);
		exitButton.SetActive(true);
		surrenderButton.SetActive(false);
	}

	public override void OnPlayerLeftRoom(Player otherPlayer) {
		DiskColor colorOtherPlayer = (DiskColor) otherPlayer.CustomProperties["color"];
		FinishGame(colorOtherPlayer);
	}

	public void ShareTwitter() {
		Application.OpenURL(
			new Func<string>(() => {
				var query = HttpUtility.ParseQueryString("");
				query.Add("url", "https://unityroom.com/games/scalable-reversi");
				query.Add("hashtags", "クソデカリバーシ");
				StringBuilder sb = new StringBuilder();
				sb.Append(GameStateSingleton.Instance.OppositePlayerName);
				sb.Append("と対戦し、");
				sb.Append(_countBlackDisk);
				sb.Append("-");
				sb.Append(_countWhiteDisk);
				if (_playerSurrenderColor != None) {
					sb.Append("でしたが、");
					if (_playerSurrenderColor == GameStateSingleton.Instance.PlayerColor) {
						sb.Append("投了しました。");
					} else {
						sb.Append("相手が投了しました。");
					}
				} else {
					sb.Append("で");
					if (_countBlackDisk == _countWhiteDisk) {
						sb.Append("引き分けでした。");
					} else {
						if ((GameStateSingleton.Instance.PlayerColor == Black) == (_countBlackDisk > _countWhiteDisk)) {
							sb.Append("勝ちました！");
						} else {
							sb.Append("負けました……");
						}
					}
				}

				query.Add("text", sb.ToString());
				return new UriBuilder("https://twitter.com/intent/tweet") {Query = query.ToString()}.Uri.ToString();
			})()
		);
	}

	public void SavePNGImage() {
		StartCoroutine(CaptureScreenshotIntoTexture());
	}

	private IEnumerator CaptureScreenshotIntoTexture() {
		Texture2D tex = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		yield return new WaitForEndOfFrame();
		tex.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
		tex.Apply();
		byte[] bytes = tex.EncodeToPNG();
		Destroy(tex);
#if UNITY_EDITOR
		File.WriteAllBytes(Application.persistentDataPath + "/screenshot.png", bytes);
#elif UNITY_WEBGL
		DownloadPNG(bytes, bytes.Length);
#endif
	}
}