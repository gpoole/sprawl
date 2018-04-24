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

	private List<Car> cars = new List<Car>();

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

			var car = Car.Create(playerState.player.car, starts[i].transform, playerState);
			car.gameObject.transform.parent = carsGroup.transform;
			cars.Add(car);

			ScreenManager.Instance.AddScreen(playerState, car.GetComponent<DriftCameraRig>());
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