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

	void Start() {
		fracXSlider.GetComponent<Slider>().value = GameStateSingleton.instance.fracX;
		fracYSlider.GetComponent<Slider>().value = GameStateSingleton.instance.fracY;
	}

	public void OnSoloplayButtonClick() {
		GameStateSingleton.instance.playerColor = EnumerableExtensions.ChooseRandom(DiskColor.Black, DiskColor.White);
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
	
	public void OnMultiplayButtonClick() {
		PhotonNetwork.OfflineMode = false;
		PhotonNetwork.ConnectUsingSettings();
		SceneManager.LoadSceneAsync("ConnectScene", LoadSceneMode.Single);
	}

	public void OnChangeFracXSlider() {
		GameStateSingleton.instance.fracX = Mathf.FloorToInt(fracXSlider.GetComponent<Slider>().value);
		fracXSlider.transform.Find("Value Indicator").GetComponent<TextMeshProUGUI>().text = Mathf.FloorToInt(fracXSlider.GetComponent<Slider>().value).ToString();
	}

	public void OnChangeFracYSlider() {
		GameStateSingleton.instance.fracY = Mathf.FloorToInt(fracYSlider.GetComponent<Slider>().value);
		fracYSlider.transform.Find("Value Indicator").GetComponent<TextMeshProUGUI>().text = Mathf.FloorToInt(fracYSlider.GetComponent<Slider>().value).ToString();
	}
}
