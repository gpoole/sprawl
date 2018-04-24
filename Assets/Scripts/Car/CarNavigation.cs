using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarNavigation : MonoBehaviour {

	private float lapStartTime;

	private CarController car;

	private CarPlayerInput input;

	private PlayerState playerState;

	void Start() {
		playerState = GetComponent<Car>().playerState;
		if (playerState) {
			car = GetComponent<CarController>();
			input = GetComponent<CarPlayerInput>();
			playerState.lastCheckpoint = TrackNavigation.Instance.start;
			StartCoroutine(UpdateCheckpoint());
		} else {
			Debug.Log("No playerState detected, navigation will be disabled.");
			enabled = false;
		}
	}

	void Update() {
		if (input.IsResetting) {
			ResetCar();
		}
	}

	IEnumerator UpdateCheckpoint() {
		while (true) {
			if (car.IsOnTrack) {
				var prevCheckpoint = playerState.lastCheckpoint;
				playerState.lastCheckpoint = TrackNavigation.Instance.UpdateCurrentCheckpoint(playerState.lastCheckpoint, car.transform.position);

				if (prevCheckpoint != TrackNavigation.Instance.start && playerState.lastCheckpoint == TrackNavigation.Instance.start) {
					playerState.NextLap();
				}
			}

			yield return null;
		}
	}

	public void ResetCar() {
		car.transform.position = playerState.lastCheckpoint.transform.position;
		car.transform.rotation = playerState.lastCheckpoint.transform.rotation;
		car.Reset();
	}

}