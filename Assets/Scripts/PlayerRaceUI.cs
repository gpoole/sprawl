using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRaceUI : MonoBehaviour {

	public int playerId;

	public Text laps;

	void Start() {

	}

	void Update() {

	}

	public void SetLap(int lap) {
		if (laps) {
			laps.text = lap.ToString();
		}
	}
}