using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour {

	public class PlayerState {

		public Player player;

		public int lap = 0;

		public TrackNavigationCheckpoint lastCheckpoint;

		public CarController car;
	}

	public static RaceManager Instance {
		get;
		private set;
	}

	private ScreenManager screenManager;

	private GameObject[] starts;

	private List<PlayerState> playerStates = new List<PlayerState>();

	void Awake() {
		Instance = this;
	}

	void Start() {
		starts = GameObject.FindGameObjectsWithTag("Start");
		screenManager = ScreenManager.Instance;
		CreateRacers();
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
			playerStates.Add(new PlayerState { player = player, lastCheckpoint = TrackNavigation.Instance.start, car = carController });
		}
	}

	void UpdateCheckpoints() {
		foreach (var playerState in playerStates) {
			var screen = screenManager.screens[playerState.player.id];
			var prevCheckpoint = playerState.lastCheckpoint;
			// TODO: check if on or very near the track, but assuming they are then update the checkpoint
			playerState.lastCheckpoint = TrackNavigation.Instance.UpdateCurrentCheckpoint(playerState.lastCheckpoint, playerState.car.transform.position);
			screen.debug.Log(DebugUI.Category.GameLogic, "lastCheckpoint", playerState.lastCheckpoint);

			if (prevCheckpoint != TrackNavigation.Instance.start && playerState.lastCheckpoint == TrackNavigation.Instance.start) {
				playerState.lap++;
				screen.ui.SetLap(playerState.lap);
			}
		}
	}

	public void ResetPlayer(int id) {
		playerStates[id].car.transform.position = playerStates[id].lastCheckpoint.transform.position;
		playerStates[id].car.transform.rotation = playerStates[id].lastCheckpoint.transform.rotation;
	}
}