using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class TitleController : MonoBehaviour {
	[SerializeField] private GameObject fracXSlider;
	[SerializeField] private GameObject fracYSlider;
	public void OnStartButtonClick() {
		ConfigurationSingleton.instance.fracX = Mathf.FloorToInt(fracXSlider.GetComponent<Slider>().value);
		ConfigurationSingleton.instance.fracY = Mathf.FloorToInt(fracYSlider.GetComponent<Slider>().value);
		SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);
	}

	public void OnChangeFracXSlider() {
		fracXSlider.transform.Find("Value Indicator").GetComponent<TextMeshProUGUI>().text = Mathf.FloorToInt(fracXSlider.GetComponent<Slider>().value).ToString();
	}

	public void OnChangeFracYSlider() {
		fracYSlider.transform.Find("Value Indicator").GetComponent<TextMeshProUGUI>().text = Mathf.FloorToInt(fracYSlider.GetComponent<Slider>().value).ToString();
	}
}
