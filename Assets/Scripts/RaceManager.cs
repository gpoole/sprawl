using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;

public class RaceManager : MonoBehaviour {

	public static RaceManager Instance {
		get;
		private set;
	}

	public enum RaceMode {
		Intro,
		Starting,
		Racing,
		Finished
	}

	[Serializable]
	public class ReactiveRaceModeProperty : ReactiveProperty<RaceMode> {
		public ReactiveRaceModeProperty() : base() { }
		public ReactiveRaceModeProperty(RaceMode initialValue) : base(initialValue) { }
	}

	public FloatReactiveProperty startCountdown = new FloatReactiveProperty();

	public ReactiveRaceModeProperty mode = new ReactiveRaceModeProperty();

	public int lapCount = 3;

	private GameObject[] starts;

	private List<PlayerState> playerStates = new List<PlayerState>();

	private List<Car> cars = new List<Car>();

	void Awake() {
		Instance = this;
	}

	void Start() {
		starts = GameObject.FindGameObjectsWithTag("Start");

		StartCoroutine(RunGame());
	}

	void Update() {
		UpdateRanks();
	}

	IEnumerator RunGame() {
		yield return StartCoroutine(PlayIntro());
		CreatePlayers();
		StartCoroutine(RaceStart());
	}

	IEnumerator PlayIntro() {
		mode.Value = RaceMode.Intro;
		var intro = GameObject.Find("CinematicIntro");
		var director = intro.GetComponent<PlayableDirector>();
		yield return new WaitWhile(() => director.state == PlayState.Playing);
		intro.SetActive(false);
	}

	IEnumerator RaceStart() {
		mode.Value = RaceMode.Starting;
		for (var second = 4f; second >= 0; second -= Time.deltaTime) {
			startCountdown.Value = second;
			yield return null;
		}
		startCountdown.Value = 0;
		mode.Value = RaceMode.Racing;
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

			ScreenManager.Instance.AddScreen(playerState, car);
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