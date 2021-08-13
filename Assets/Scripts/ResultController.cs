using TMPro;
using UnityEngine;

public class ResultController : MonoBehaviour {
	[SerializeField] private GameObject scoreBlackUI;
	[SerializeField] private GameObject scoreWhiteUI;

	// Start is called before the first frame update
	void Start() {
		scoreBlackUI.GetComponent<TextMeshProUGUI>().text = ConfigurationSingleton.instance.blackScore.ToString();
		scoreWhiteUI.GetComponent<TextMeshProUGUI>().text = ConfigurationSingleton.instance.whiteScore.ToString();
	}
}
