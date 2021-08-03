using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour {
	private bool isBlack = true;
	private GameObject diskPrefab;
	private GameObject board;
	[SerializeField] private Color blackColor = Color.black;
	[SerializeField] private Color whiteColor = Color.white;

	// Start is called before the first frame update
	void Start() {
		// 何故か動かない
		// Start()時点ではSerializeFieldにおかしな値が格納されてるっぽい
		// this.GetComponent<SpriteRenderer>().color = blackColor;
		isBlack = true;
		diskPrefab = (GameObject) Resources.Load("Prefabs/DiskPrefab");
	}

	// Update is called once per frame
	void Update() {
		// キー入力による移動
		if (Input.GetKeyDown(KeyCode.UpArrow)) {
			this.transform.position += new Vector3(0, 1.0f);
		}
		if (Input.GetKeyDown(KeyCode.DownArrow)) {
			this.transform.position += new Vector3(0, -1.0f);
		}
		if (Input.GetKeyDown(KeyCode.LeftArrow)) {
			this.transform.position += new Vector3(-1.0f, 0);
		}
		if (Input.GetKeyDown(KeyCode.RightArrow)) {
			this.transform.position += new Vector3(1.0f, 0);
		}

		// 石を置いて色を変更する
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)) {
			Instantiate(diskPrefab, this.transform.position, Quaternion.identity);
			this.GetComponent<SpriteRenderer>().color = isBlack ? Color.white : Color.black;
			isBlack = !isBlack;
		}
	}
}
