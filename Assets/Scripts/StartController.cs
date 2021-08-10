using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartController : MonoBehaviour {
	[SerializeField] private GameObject fracXSlider;
	[SerializeField] private GameObject fracYSlider;
	public void OnStartButtonClick() {
		ConfigurationSingleton.instance.fracX = Mathf.FloorToInt(fracXSlider.GetComponent<Slider>().value);
		ConfigurationSingleton.instance.fracY = Mathf.FloorToInt(fracYSlider.GetComponent<Slider>().value);
		SceneManager.LoadSceneAsync("MainScene", LoadSceneMode.Single);
	}
}
