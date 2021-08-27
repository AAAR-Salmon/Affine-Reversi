using Enum;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ConnectionController : MonoBehaviourPunCallbacks {
	private static readonly RoomOptions FreeRoomOptions;
	private static readonly RoomOptions PrivateRoomOptions;
	[SerializeField] private TMP_InputField playerNameTmpInput;
	[SerializeField] private TMP_InputField roomNameTmpInput;
	[SerializeField] private Button matchmakingFreeButton;
	[SerializeField] private Button matchmakingPrivateButton;

	static ConnectionController() {
		FreeRoomOptions = new RoomOptions {MaxPlayers = 2, CustomRoomProperties = new Hashtable {{"IsFree", true}}};
		PrivateRoomOptions = new RoomOptions {MaxPlayers = 2, CustomRoomProperties = new Hashtable {{"IsFree", false}}};
	}

	private void Start() {
		matchmakingFreeButton.interactable = true;
		matchmakingPrivateButton.interactable = false;
	}

	public void DoMatchmakingFree() {
		GameStateSingleton.Instance.PlayerName = playerNameTmpInput.text;
		PhotonNetwork.JoinRandomOrCreateRoom(roomOptions: FreeRoomOptions);
	}

	public void DoMatchmakingPrivate() {
		GameStateSingleton.Instance.PlayerName = playerNameTmpInput.text;
		PhotonNetwork.JoinOrCreateRoom(roomNameTmpInput.text, PrivateRoomOptions, TypedLobby.Default);
	}

	public void OnEnterPlayerName() {
		PhotonNetwork.LocalPlayer.NickName = playerNameTmpInput.text;
	}

	public void OnChangeRoomName() {
		if (roomNameTmpInput.text.Length > 0 && PhotonNetwork.IsConnected) {
			matchmakingPrivateButton.interactable = true;
		} else {
			matchmakingPrivateButton.interactable = false;
		}
	}

	public override void OnJoinedRoom() {
		matchmakingFreeButton.interactable = false;
		matchmakingPrivateButton.interactable = false;
		if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers) {
			PhotonNetwork.CurrentRoom.IsOpen = false;
			DiskColor myColor = EnumerableExtensions.ChooseRandom(DiskColor.Black, DiskColor.White);
			GameStateSingleton.Instance.PlayerColor = myColor;
			PhotonNetwork.SetPlayerCustomProperties(new Hashtable {{"color", myColor}});
			PhotonNetwork.PlayerListOthers[0].SetCustomProperties(new Hashtable {
				{"color", myColor == DiskColor.Black ? DiskColor.White : DiskColor.Black}
			});
			SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);
		}
	}

	public override void OnPlayerEnteredRoom(Player newPlayer) {
		if (PhotonNetwork.CurrentRoom.PlayerCount == PhotonNetwork.CurrentRoom.MaxPlayers) {
			SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);
		}
	}
}
