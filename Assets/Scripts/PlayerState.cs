﻿using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlayerState : MonoBehaviour {

	public enum PlayerMode {
		Starting,
		Racing,
		Finished,
	}

	public Player player;

	public ReactiveProperty<PlayerMode> mode {
		get;
		private set;
	}

	public IntReactiveProperty lap {
		get;
		private set;
	}

	public IntReactiveProperty rank {
		get;
		private set;
	}

	public ReactiveCollection<float> lapTimes {
		get;
		private set;
	}

	public TrackNavigationCheckpoint lastCheckpoint;

	public CarController car;

	public PlayerScreen screen;

	private float lapStartTime;

	void Awake() {
		lap = new IntReactiveProperty();
		lap.Value = 1;

		rank = new IntReactiveProperty();
		rank.Value = 1;

		lapTimes = new ReactiveCollection<float>();

		mode = new ReactiveProperty<PlayerMode>();
		mode.Value = PlayerMode.Starting;
	}

	// Use this for initialization
	void Start() {
		StartCoroutine(RaceStart());
	}

	IEnumerator RaceStart() {
		yield return new WaitForSeconds(3f);
		car.enabled = true;
		lapStartTime = Time.time;
		mode.Value = PlayerMode.Racing;

		StartCoroutine(UpdateCheckpoint());
		StartCoroutine(FakeLaps());
	}

	IEnumerator FakeLaps() {
		for (var i = 0; i < 3; i++) {
			yield return new WaitForSeconds(3f);
			lapTimes.Add(Time.time);
		}
	}

	void RaceEnd() {
		mode.Value = PlayerMode.Finished;
		car.enabled = false;
		screen.PlayOutro();
	}

	IEnumerator UpdateCheckpoint() {
		while (true) {
			screen.debug.Log(DebugUI.Category.GameLogic, "IsOnTrack", car.IsOnTrack);
			if (car.IsOnTrack) {
				var prevCheckpoint = lastCheckpoint;
				lastCheckpoint = TrackNavigation.Instance.UpdateCurrentCheckpoint(lastCheckpoint, car.transform.position);
				screen.debug.Log(DebugUI.Category.GameLogic, "lastCheckpoint", lastCheckpoint);

				if (prevCheckpoint != TrackNavigation.Instance.start && lastCheckpoint == TrackNavigation.Instance.start) {
					lapTimes.Add(Time.time - lapStartTime);
					lapStartTime = Time.time;
					lap.Value = lap.Value + 1;

					if (lap.Value >= RaceManager.Instance.lapCount) {
						RaceEnd();
						break;
					}
				}
			}

			yield return null;
		}
	}

	public void ResetCar() {
		car.transform.position = lastCheckpoint.transform.position;
		car.transform.rotation = lastCheckpoint.transform.rotation;
		car.Reset();
	}
}