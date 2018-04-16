using System;
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

	public CarController car;

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
		lapStartTime = Time.time;
		mode.Value = PlayerMode.Racing;
		// StartCoroutine(UpdateCheckpoint());
	}

	void RaceEnd() {
		mode.Value = PlayerMode.Finished;
	}

	// FIXME: move this to each car
	IEnumerator UpdateCheckpoint() {
		while (true) {
			if (car.IsOnTrack) {
				var prevCheckpoint = lastCheckpoint;
				lastCheckpoint = TrackNavigation.Instance.UpdateCurrentCheckpoint(lastCheckpoint, car.transform.position);

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