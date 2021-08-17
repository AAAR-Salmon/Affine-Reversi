using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TitleController : MonoBehaviour {
	[SerializeField] private GameObject fracXSlider;
	[SerializeField] private GameObject fracYSlider;

	void Start() {
		fracXSlider.GetComponent<Slider>().value = GameStateSingleton.instance.fracX;
		fracYSlider.GetComponent<Slider>().value = GameStateSingleton.instance.fracY;
	}

	public void OnStartButtonClick() {
		SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);
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
