using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConfigurationSingleton {
	private static ConfigurationSingleton _instance;

	public int fracX;
	public int fracY;

	public static ConfigurationSingleton instance {
		get {
			if (_instance == null) {
				_instance = new ConfigurationSingleton();
			}
			return _instance;
		}
	}

	private ConfigurationSingleton() {
		fracX = 8;
		fracY = 8;
	}
}
