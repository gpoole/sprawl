using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CarNavigation : MonoBehaviour {

	private float lapStartTime;

	private CarController carController;

	private CarPlayerInput input;

	private PlayerState playerState;

	private GameObject[] warpPoints;

	void Start() {
		input = GetComponent<CarPlayerInput>();
		carController = GetComponent<CarController>();
		playerState = GetComponent<Car>().playerState;

		if (playerState) {
			playerState.lastCheckpoint = TrackNavigation.Instance.start;
			StartCoroutine(UpdateCheckpoint());
		} else {
			Debug.Log("No playerState detected, checkpoint navigation will be disabled");
			warpPoints = GameObject.FindGameObjectsWithTag("Respawn");
		}
	}

	void Update() {
		if (input.IsResetting) {
			ResetCar();
		}
	}

	IEnumerator UpdateCheckpoint() {
		while (true) {
			if (carController.IsOnTrack) {
				var prevCheckpoint = playerState.lastCheckpoint;
				playerState.lastCheckpoint = TrackNavigation.Instance.UpdateCurrentCheckpoint(playerState.lastCheckpoint, carController.transform.position);

				if (prevCheckpoint != TrackNavigation.Instance.start && playerState.lastCheckpoint == TrackNavigation.Instance.start) {
					playerState.NextLap();
				}
			}

			yield return null;
		}
	}

	public void ResetCar() {
		if (playerState != null) {
			WarpTo(playerState.lastCheckpoint.transform);
		} else {
			// In tracks with no navigation just dump the player at a warp point, if any
			var closestPoint = warpPoints.Aggregate((acc, point) => {
				if (acc == null || Vector3.Distance(acc.transform.position, transform.position) > Vector3.Distance(point.transform.position, transform.position)) {
					return point;
				}
				return acc;
			});

			if (closestPoint) {
				WarpTo(closestPoint.transform);
			} else {
				Debug.LogError("No warp points or track navigation, can't reset player.");
			}
		}
		carController.Reset();
	}

	void WarpTo(Transform warpPoint) {
		transform.position = warpPoint.position;
		transform.rotation = warpPoint.rotation;
	}

}