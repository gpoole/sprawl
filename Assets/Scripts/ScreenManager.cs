using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour {

	public static ScreenManager Instance {
		get;
		private set;
	}

	public class PlayerScreen {
		public Camera camera;
		public PlayerRaceUI ui;
		public DebugUI debug;
	}

	public GameObject cameraPrefab;

	public GameObject uiPrefab;

	public List<PlayerScreen> screens = new List<PlayerScreen>();

	private GameObject cameraGroup;

	private GameObject uiGroup;

	// Use this for initialization
	void Awake() {
		Instance = this;
	}

	void Start() {
		cameraGroup = new GameObject("_Cameras");
		cameraGroup.transform.parent = transform;
		uiGroup = new GameObject("_UIs");
		uiGroup.transform.parent = transform;
	}

	// Update is called once per frame
	void Update() {

	}

	public PlayerScreen AddScreen(PlayerState playerState) {
		var cameraPosition = playerState.car.transform.Find("CameraPosition");
		var cameraLookAtTarget = playerState.car.transform.Find("CameraLookAtTarget");
		var cameraObject = Instantiate(cameraPrefab, cameraPosition.position, cameraPosition.rotation, cameraGroup.transform);

		float viewportWidth = GameManager.Instance.players.Count > 1 ? 0.5f : 1f;
		float viewportHeight = GameManager.Instance.players.Count > 2 ? 0.5f : 1f;
		float viewportXOffset = viewportWidth * (playerState.player.id % 2);
		float viewportYOffset = viewportHeight * Mathf.Floor(playerState.player.id / 2f);
		var camera = cameraObject.GetComponent<Camera>();
		camera.rect = new Rect(viewportXOffset, viewportYOffset, viewportWidth, viewportHeight);

		var driftCamera = cameraObject.GetComponent<DriftCamera>();
		driftCamera.lookAtTarget = cameraLookAtTarget;
		driftCamera.positionTarget = cameraPosition;

		var uiObject = Instantiate(uiPrefab, uiGroup.transform);
		var uiRect = uiObject.transform.GetChild(0).GetComponent<RectTransform>();
		uiRect.anchorMax = new Vector2(viewportXOffset + viewportWidth, viewportYOffset + viewportHeight);
		uiRect.anchorMin = new Vector2(viewportXOffset, viewportYOffset);
		var ui = uiObject.GetComponent<PlayerRaceUI>();
		ui.playerState = playerState;
		var playerScreen = new PlayerScreen { ui = ui, camera = camera, debug = uiObject.GetComponent<DebugUI>() };
		screens.Add(playerScreen);

		return playerScreen;
	}
}