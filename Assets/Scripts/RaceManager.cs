using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

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

	public GameObject[] starts;

	public FloatReactiveProperty startCountdown = new FloatReactiveProperty();

	public ReactiveRaceModeProperty mode = new ReactiveRaceModeProperty();

	public int lapCount = 3;

	public PlayableDirector titlePrefab;

	private List<PlayerState> playerStates = new List<PlayerState>();

	private List<Car> cars = new List<Car>();

	void Awake() {
		Instance = this;
	}

	void Start() {
		StartCoroutine(RunGame());
	}

	void Update() {
		UpdateRanks();
	}

	IEnumerator RunGame() {
		yield return StartCoroutine(PlayIntro());
		CreatePlayers();
		StartCoroutine(RaceStart());

		foreach (var playerState in playerStates) {
			playerState.mode
				.Where(mode => mode == PlayerState.PlayerMode.Finished)
				.Subscribe(_ => {
					var allFinished = playerStates.All(otherPlayerState => otherPlayerState.mode.Value == PlayerState.PlayerMode.Finished);
					if (allFinished) {
						StartCoroutine(WaitForRestart());
					}
				})
				.AddTo(this);
		}
	}

	IEnumerator WaitForRestart() {
		yield return new WaitForSeconds(3f);
		var menuController = new MenuController();
		yield return new WaitUntil(() => menuController.ok);
		SceneManager.LoadScene("Menu");
	}

	IEnumerator PlayIntro() {
		mode.Value = RaceMode.Intro;
		var intro = GameObject.Find("CinematicIntro");
		var director = intro.GetComponent<PlayableDirector>();
		var menuInput = new MenuController();
		yield return new WaitWhile(() => director.state == PlayState.Playing && !menuInput.ok);
		menuInput.Destroy();
		intro.SetActive(false);
	}

	IEnumerator RaceStart() {
		mode.Value = RaceMode.Starting;
		StartCoroutine(ShowTitle());
		for (var second = 4f; second >= 0; second -= Time.deltaTime) {
			startCountdown.Value = second;
			yield return null;
		}
		startCountdown.Value = 0;
		mode.Value = RaceMode.Racing;
	}

	IEnumerator ShowTitle() {
		var title = Instantiate(titlePrefab);
		title.Play();
		yield return new WaitWhile(() => title.state == PlayState.Playing);
		Destroy(title);
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

			var car = Car.Create(playerState.player.character.carPrefab, starts[i].transform, playerState);
			car.gameObject.transform.parent = carsGroup.transform;
			cars.Add(car);

			ScreenManager.Instance.AddScreen(playerState, car);
		}
	}

	void UpdateRanks() {
		var unfinishedPlayers = playerStates
			.Where(playerState => playerState.mode.Value == PlayerState.PlayerMode.Racing)
			.OrderByDescending(playerState => playerState.lap.Value)
			.ThenByDescending(playerState => playerState.lastCheckpoint.order)
			.ThenBy(playerState => {
				var closestCheckpointDistance = playerState.lastCheckpoint.next.Min(nextCheckpoint => nextCheckpoint.PlaneDistance(cars[playerState.player.id].transform.position));
				return closestCheckpointDistance;
			});

		var finishedPlayers = playerStates
			.Where(playerState => playerState.mode.Value == PlayerState.PlayerMode.Finished)
			.OrderBy(playerState => playerState.rank.Value);

		var orderedPlayers = finishedPlayers.Concat(unfinishedPlayers);

		var rank = 1;
		foreach (var playerState in orderedPlayers) {
			playerState.rank.Value = rank++;
		}
	}
}