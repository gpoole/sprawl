﻿using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlayerState : MonoBehaviour {

	public enum PlayerMode {
		Starting,
		Racing,
		Finished,
	}

	[Serializable]
	public class ReactivePlayerModeProperty : ReactiveProperty<PlayerMode> {
		public ReactivePlayerModeProperty() : base() { }
		public ReactivePlayerModeProperty(PlayerMode initialValue) : base(initialValue) { }
	}

	public Player player;

	public ReactivePlayerModeProperty mode;

	public IntReactiveProperty lap;

	public IntReactiveProperty rank;

	public ReactiveCollection<float> lapTimes;

	public TrackNavigationCheckpoint lastCheckpoint;

	private float lapStartTime;

	void Awake() {
		lap = new IntReactiveProperty(1);
		rank = new IntReactiveProperty(1);
		lapTimes = new ReactiveCollection<float>();
		mode = new ReactivePlayerModeProperty();
	}

	void Start() {
		StartCoroutine(RaceStart());
	}

	IEnumerator RaceStart() {
		mode.Value = PlayerMode.Starting;
		yield return new WaitForSeconds(3f);
		mode.Value = PlayerMode.Racing;
		lapStartTime = Time.time;
		StartCoroutine(UpdateTimer());
	}

	IEnumerator UpdateTimer() {
		while (mode.Value == PlayerMode.Racing) {
			var lapTime = Time.time - lapStartTime;
			if (lapTimes.Count < lap.Value) {
				lapTimes.Add(lapTime);
			} else {
				lapTimes[lap.Value - 1] = lapTime;
			}
			yield return null;
		}
	}

	public void NextLap() {
		lap.Value += 1;
		lapStartTime = Time.time;
		if (lap.Value >= RaceManager.Instance.lapCount) {
			mode.Value = PlayerState.PlayerMode.Finished;
		}
	}
}