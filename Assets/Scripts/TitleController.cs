using Enum;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviourPunCallbacks {
	[SerializeField] private GameObject fracXSlider;
	[SerializeField] private GameObject fracYSlider;

	private void Start() {
		fracXSlider.GetComponent<Slider>().value = GameStateSingleton.Instance.FracX;
		fracYSlider.GetComponent<Slider>().value = GameStateSingleton.Instance.FracY;
	}

	public void OnSoloPlayButtonClick() {
		GameStateSingleton.Instance.PlayerColor = EnumerableExtensions.ChooseRandom(DiskColor.Black, DiskColor.White);
		PhotonNetwork.OfflineMode = true;
		SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);
	}

	public override void OnConnectedToMaster() {
		if (PhotonNetwork.OfflineMode) {
			PhotonNetwork.CreateRoom(null, new RoomOptions {MaxPlayers = 1}, TypedLobby.Default);
			PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable {
				{"color", EnumerableExtensions.ChooseRandom(DiskColor.Black, DiskColor.White)}
			});
		}
	}

	public void OnMultiPlayButtonClick() {
		PhotonNetwork.OfflineMode = false;
		PhotonNetwork.ConnectUsingSettings();
		SceneManager.LoadSceneAsync("ConnectScene", LoadSceneMode.Single);
	}

	public void OnChangeFracXSlider() {
		GameStateSingleton.Instance.FracX = Mathf.FloorToInt(fracXSlider.GetComponent<Slider>().value);
		fracXSlider.transform.Find("Value Indicator").GetComponent<TextMeshProUGUI>().text =
			Mathf.FloorToInt(fracXSlider.GetComponent<Slider>().value).ToString();
	}

	public void OnChangeFracYSlider() {
		GameStateSingleton.Instance.FracY = Mathf.FloorToInt(fracYSlider.GetComponent<Slider>().value);
		fracYSlider.transform.Find("Value Indicator").GetComponent<TextMeshProUGUI>().text =
			Mathf.FloorToInt(fracYSlider.GetComponent<Slider>().value).ToString();
	}
}