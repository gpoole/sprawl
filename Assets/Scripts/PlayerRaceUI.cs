using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRaceUI : MonoBehaviour {

	public int playerId;

	public Text lapDisplay;

	public Text lapTimes;

	void Start() {

	}

	void Update() {

	}

	public void SetLap(int lap) {
		if (lapDisplay) {
			lapDisplay.text = lap.ToString();
		}
	}

	public void AddLapTime(int lap, float time) {
		lapTimes.text += string.Format("Lap {0}: {1:00}:{2:00.00}\n", lap, Mathf.Floor(time / 60), time % 60);
	}
}