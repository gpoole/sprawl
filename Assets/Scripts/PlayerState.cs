using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour {

	public enum PlayerMode {
		Starting,
		Racing,
		Finished,
	}

	public Player player;

	public PlayerMode mode;

	public int lap = 1;

	public int rank = 0;

	public float[] lapTimes;

	private float lapStartTime;

	public TrackNavigationCheckpoint lastCheckpoint;

	public CarController car;

	public PlayerScreen screen;

	// Use this for initialization
	void Start() {
		lapTimes = new float[RaceManager.Instance.lapCount];

		mode = PlayerMode.Starting;

		StartCoroutine(RaceStart());
	}

	IEnumerator RaceStart() {
		yield return new WaitForSeconds(3f);
		car.enabled = true;
		lapStartTime = Time.time;
		mode = PlayerMode.Racing;

		StartCoroutine(UpdateCheckpoint());
	}

	void RaceEnd() {
		mode = PlayerMode.Finished;
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
					lapTimes[lap - 1] = Time.time - lapStartTime;
					lapStartTime = Time.time;
					lap++;

					if (lap >= RaceManager.Instance.lapCount) {
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