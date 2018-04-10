using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRaceUI : MonoBehaviour {

	public RaceManager.PlayerState playerState;

	public Text lap;

	public Text lapTimes;

	public Text rank;

	void Start() {

	}

	void Update() {
		// FIXME: move to coroutine
		lap.text = playerState.lap.ToString();

		lapTimes.text = "";
		for (var lap = 1; lap <= playerState.lapTimes.Length; lap++) {
			var lapTime = playerState.lapTimes[lap - 1];
			if (lapTime == 0) {
				break;
			}
			lapTimes.text += string.Format("Lap {0}: {1:00}:{2:00.00}\n", lap, Mathf.Floor(lapTime / 60), lapTime % 60);
		}

		rank.text = playerState.rank + OrdinalSuffix(playerState.rank);
	}

	string OrdinalSuffix(int rank) {
		if (rank <= 0) return "";

		switch (rank % 100) {
			case 11:
			case 12:
			case 13:
				return "th";
		}

		switch (rank % 10) {
			case 1:
				return "st";
			case 2:
				return "nd";
			case 3:
				return "rd";
			default:
				return "th";
		}

	}
}