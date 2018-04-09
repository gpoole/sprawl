using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour {

	public class PlayerState {

		public Player player;

		public int lap = 1;

		public float[] lapTimes;

		public TrackNavigationCheckpoint lastCheckpoint;

		public CarController car;
	}

	public static RaceManager Instance {
		get;
		private set;
	}

	private ScreenManager screenManager;

	private GameObject[] starts;

	private float startTime;

	private float lapStartTime;

	private List<PlayerState> playerStates = new List<PlayerState>();

	void Awake() {
		Instance = this;
	}

	void Start() {
		starts = GameObject.FindGameObjectsWithTag("Start");
		screenManager = ScreenManager.Instance;
		CreateRacers();
		startTime = Time.time;
		lapStartTime = startTime;
	}

	void Update() {
		UpdateCheckpoints();
	}

	void CreateRacers() {
		for (var i = 0; i < GameManager.Instance.players.Count; i++) {
			var player = GameManager.Instance.players[i];
			var carInstance = Instantiate(player.car, starts[i].transform.position, starts[i].transform.rotation);
			var carController = carInstance.GetComponent<CarController>();
			carController.playerId = player.id;
			screenManager.AddScreen(carInstance, player.id);
			playerStates.Add(new PlayerState { player = player, lastCheckpoint = TrackNavigation.Instance.start, car = carController, lapTimes = new float[3] });
		}
	}

	void UpdateCheckpoints() {
		foreach (var playerState in playerStates) {
			var screen = screenManager.screens[playerState.player.id];
			screen.debug.Log(DebugUI.Category.GameLogic, "IsOnTrack", playerState.car.IsOnTrack);
			if (playerState.car.IsOnTrack) {
				var prevCheckpoint = playerState.lastCheckpoint;
				// TODO: check if on or very near the track, but assuming they are then update the checkpoint
				playerState.lastCheckpoint = TrackNavigation.Instance.UpdateCurrentCheckpoint(playerState.lastCheckpoint, playerState.car.transform.position);
				screen.debug.Log(DebugUI.Category.GameLogic, "lastCheckpoint", playerState.lastCheckpoint);

				if (prevCheckpoint != TrackNavigation.Instance.start && playerState.lastCheckpoint == TrackNavigation.Instance.start) {
					var lapTime = playerState.lapTimes[playerState.lap] = Time.time - lapStartTime;
					lapStartTime = Time.time;
					screen.ui.AddLapTime(playerState.lap, lapTime);

					playerState.lap++;
					screen.ui.SetLap(playerState.lap);
				}
			}
		}
	}

	public void ResetPlayer(int id) {
		playerStates[id].car.transform.position = playerStates[id].lastCheckpoint.transform.position;
		playerStates[id].car.transform.rotation = playerStates[id].lastCheckpoint.transform.rotation;
		playerStates[id].car.Reset();
	}
}