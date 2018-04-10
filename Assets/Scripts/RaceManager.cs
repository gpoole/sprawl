using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour {

	public class PlayerState {

		public Player player;

		public int lap = 1;

		public int rank = 0;

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
		UpdateRanks();
	}

	void CreateRacers() {
		for (var i = 0; i < GameManager.Instance.players.Count; i++) {
			var player = GameManager.Instance.players[i];
			var carInstance = Instantiate(player.car, starts[i].transform.position, starts[i].transform.rotation);
			var carController = carInstance.GetComponent<CarController>();
			carController.playerId = player.id;
			var playerState = new PlayerState { player = player, lastCheckpoint = TrackNavigation.Instance.start, car = carController, lapTimes = new float[3] };
			playerStates.Add(playerState);
			screenManager.AddScreen(playerState);
		}
	}

	void UpdateCheckpoints() {
		foreach (var playerState in playerStates) {
			var screen = screenManager.screens[playerState.player.id];
			screen.debug.Log(DebugUI.Category.GameLogic, "IsOnTrack", playerState.car.IsOnTrack);
			if (playerState.car.IsOnTrack) {
				var prevCheckpoint = playerState.lastCheckpoint;
				playerState.lastCheckpoint = TrackNavigation.Instance.UpdateCurrentCheckpoint(playerState.lastCheckpoint, playerState.car.transform.position);
				screen.debug.Log(DebugUI.Category.GameLogic, "lastCheckpoint", playerState.lastCheckpoint);

				if (prevCheckpoint != TrackNavigation.Instance.start && playerState.lastCheckpoint == TrackNavigation.Instance.start) {
					playerState.lapTimes[playerState.lap - 1] = Time.time - lapStartTime;
					lapStartTime = Time.time;
					playerState.lap++;
				}
			}
		}
	}

	void UpdateRanks() {
		var sortedPlayers = playerStates
			.OrderBy(playerState => playerState.lap)
			.ThenBy(playerState => playerState.lastCheckpoint.order)
			.ThenBy(playerStates => playerStates.lastCheckpoint.PlaneDistance(playerStates.car.transform.position))
			.Reverse();

		var rank = 1;
		foreach (var playerState in sortedPlayers) {
			playerState.rank = rank++;
		}
	}

	public void ResetPlayer(int id) {
		playerStates[id].car.transform.position = playerStates[id].lastCheckpoint.transform.position;
		playerStates[id].car.transform.rotation = playerStates[id].lastCheckpoint.transform.rotation;
		playerStates[id].car.Reset();
	}
}