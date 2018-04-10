using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RaceManager : MonoBehaviour {

	public static RaceManager Instance {
		get;
		private set;
	}

	private GameObject[] starts;

	private List<PlayerState> playerStates = new List<PlayerState>();

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

		for (var i = 0; i < GameManager.Instance.players.Count; i++) {
			var player = GameManager.Instance.players[i];
			var playerState = (new GameObject("Player" + i, typeof(PlayerState))).GetComponent<PlayerState>();
			playerState.transform.parent = playerStatesGroup.transform;
			playerState.player = player;
			playerState.start = starts[i];
			playerState.lastCheckpoint = TrackNavigation.Instance.start;
			playerStates.Add(playerState);
		}
	}

	void UpdateRanks() {
		var sortedPlayers = playerStates
			.OrderBy(playerState => playerState.lap)
			.ThenBy(playerState => playerState.lastCheckpoint.order)
			.ThenBy(playerState => playerState.lastCheckpoint.PlaneDistance(playerState.car.transform.position))
			.Reverse();

		var rank = 1;
		foreach (var playerState in sortedPlayers) {
			playerState.rank = rank++;
		}
	}
}