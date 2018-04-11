using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class PlayerRaceUI : MonoBehaviour {

	public PlayerState playerState;

	public Text lap;

	public Text lapTimes;

	public Text rank;

	void Start() {
		playerState.lap.SubscribeToText(lap);
		playerState.rank.Select(rank => rank + OrdinalSuffix(rank)).SubscribeToText(rank);
		playerState.lapTimes
			.ObserveAdd()
			.Select(ev => String.Format("Lap {0}: {1:00}:{2:00.00}", ev.Index + 1, Mathf.Floor(ev.Value / 60), ev.Value % 60))
			.Scan(((acc, lapTime) => acc + "\n" + lapTime))
			.SubscribeToText(lapTimes);
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