using System;
using System.Text;
using System.Web;
using TMPro;
using UnityEngine;

public class ResultController : MonoBehaviour {
	[SerializeField] private TextMeshProUGUI scoreBlackUI;
	[SerializeField] private TextMeshProUGUI scoreWhiteUI;

	// Start is called before the first frame update
	void Start() {
		scoreBlackUI.text = GameStateSingleton.instance.blackScore.ToString();
		scoreWhiteUI.text = GameStateSingleton.instance.whiteScore.ToString();
	}

	public void ShareTwitter() {
		Application.OpenURL(
			new Func<string>(() => {
				var query = HttpUtility.ParseQueryString("");
				query.Add("url", "https://unityroom.com/games/scalable-reversi");
				query.Add("hashtags", "クソデカリバーシ");
				StringBuilder sb = new StringBuilder();
				sb.Append(GameStateSingleton.instance.blackScore);
				sb.Append("-");
				sb.Append(GameStateSingleton.instance.whiteScore);
				sb.Append("で");
				if (GameStateSingleton.instance.blackScore == GameStateSingleton.instance.whiteScore) {
					sb.Append("引き分けでした。");
				} else {
					if ((GameStateSingleton.instance.playerColor == DiskColor.Black) == (GameStateSingleton.instance.blackScore > GameStateSingleton.instance.whiteScore)) {
						sb.Append("勝ちました！");
					} else {
						sb.Append("負けました……");
					}
				}
				query.Add("text", sb.ToString());
				return new System.UriBuilder("https://twitter.com/intent/tweet") {
					Query = query.ToString()
				}.Uri.ToString();
			})()
		);
	}
}
