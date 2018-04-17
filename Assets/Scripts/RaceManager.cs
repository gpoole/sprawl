using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour {

	public static RaceManager Instance {
		get;
		private set;
	}

	public int lapCount = 3;

	private GameObject[] starts;

	private List<PlayerState> playerStates = new List<PlayerState>();

	private List<CarController> cars = new List<CarController>();

	void Awake() {
		Instance = this;
	}

	void Start() {
		starts = GameObject.FindGameObjectsWithTag("Start");
		CreatePlayers();
	}

	void Update() {
		UpdateRanks();
	}

	void CreatePlayers() {
		var playerStatesGroup = new GameObject("_PlayerStates");
		var carsGroup = new GameObject("_Cars");

		for (var i = 0; i < GameManager.Instance.players.Count; i++) {
			var player = GameManager.Instance.players[i];

			var playerState = (new GameObject("Player" + i, typeof(PlayerState))).GetComponent<PlayerState>();
			playerState.transform.parent = playerStatesGroup.transform;
			playerState.player = player;
			playerStates.Add(playerState);

			var carInstance = Instantiate(player.car, starts[i].transform.position, starts[i].transform.rotation);
			carInstance.transform.parent = carsGroup.transform;
			var car = carInstance.GetComponent<CarController>();
			carInstance.GetComponent<CarPlayerInput>().device = player.device;
			carInstance.GetComponent<CarNavigation>().playerState = playerState;
			cars.Add(car);

			var playerScreen = ScreenManager.Instance.AddScreen(playerState, car.GetComponent<DriftCameraRig>());
			car.valueTracker = playerScreen.ui.transform.GetComponentInChildren<DebugValueTracker>();
		}
	}

	void UpdateRanks() {
		var sortedPlayers = playerStates
			.OrderBy(playerState => playerState.lap.Value)
			.ThenBy(playerState => playerState.lastCheckpoint.order)
			.ThenBy(playerState => playerState.lastCheckpoint.PlaneDistance(cars[playerState.player.id].transform.position))
			.Reverse();

		var rank = 1;
		foreach (var playerState in sortedPlayers) {
			playerState.rank.Value = rank++;
		}
	}
}