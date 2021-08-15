using System;
using System.Web;
using TMPro;
using UnityEngine;

public class ResultController : MonoBehaviour {
	[SerializeField] private TextMeshProUGUI scoreBlackUI;
	[SerializeField] private TextMeshProUGUI scoreWhiteUI;

	// Start is called before the first frame update
	void Start() {
		scoreBlackUI.text = ConfigurationSingleton.instance.blackScore.ToString();
		scoreWhiteUI.text = ConfigurationSingleton.instance.whiteScore.ToString();
	}
}
