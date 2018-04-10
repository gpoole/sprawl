using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerState : MonoBehaviour {

	public Player player;

	public PlayerMode mode;

	public int lap = 1;

	public int rank = 0;

	public float[] lapTimes = new float[3];

	private float lapStartTime;

	public GameObject start;

	public TrackNavigationCheckpoint lastCheckpoint;

	public CarController car;

	private PlayerScreen screen;

	// Use this for initialization
	void Start() {
		var screenManager = ScreenManager.Instance;

		var carInstance = Instantiate(player.car, start.transform.position, start.transform.rotation);
		carInstance.transform.parent = transform;
		car = carInstance.GetComponent<CarController>();
		car.playerState = this;

		screen = screenManager.AddScreen(this);

		lapStartTime = Time.time; // FIXME: this needs to start when the race starts
	}

	// Update is called once per frame
	void Update() {
		screen.debug.Log(DebugUI.Category.GameLogic, "IsOnTrack", car.IsOnTrack);
		if (car.IsOnTrack) {
			var prevCheckpoint = lastCheckpoint;
			lastCheckpoint = TrackNavigation.Instance.UpdateCurrentCheckpoint(lastCheckpoint, car.transform.position);
			screen.debug.Log(DebugUI.Category.GameLogic, "lastCheckpoint", lastCheckpoint);

			if (prevCheckpoint != TrackNavigation.Instance.start && lastCheckpoint == TrackNavigation.Instance.start) {
				lapTimes[lap - 1] = Time.time - lapStartTime;
				lapStartTime = Time.time;
				lap++;
			}
		}
	}

	public void ResetCar() {
		car.transform.position = lastCheckpoint.transform.position;
		car.transform.rotation = lastCheckpoint.transform.rotation;
		car.Reset();
	}
}