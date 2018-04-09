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

	// Use this for initialization
	void Awake() {
		Instance = this;
	}

	// Update is called once per frame
	void Update() {

	}

	public void AddScreen(GameObject car, int playerId) {
		var cameraPosition = car.transform.Find("CameraPosition");
		var cameraLookAtTarget = car.transform.Find("CameraLookAtTarget");
		var cameraObject = Instantiate(cameraPrefab, cameraPosition.position, cameraPosition.rotation, transform);

		float viewportWidth = GameManager.Instance.players.Count > 1 ? 0.5f : 1f;
		float viewportHeight = GameManager.Instance.players.Count > 2 ? 0.5f : 1f;
		float viewportXOffset = viewportWidth * (playerId % 2);
		float viewportYOffset = viewportHeight * Mathf.Floor(playerId / 2f);
		var camera = cameraObject.GetComponent<Camera>();
		camera.rect = new Rect(viewportXOffset, viewportYOffset, viewportWidth, viewportHeight);

		var driftCamera = cameraObject.GetComponent<DriftCamera>();
		driftCamera.lookAtTarget = cameraLookAtTarget;
		driftCamera.positionTarget = cameraPosition;

		var uiObject = Instantiate(uiPrefab);
		var uiRect = uiObject.transform.GetChild(0).GetComponent<RectTransform>();
		uiRect.anchorMax = new Vector2(viewportXOffset + viewportWidth, viewportYOffset + viewportHeight);
		uiRect.anchorMin = new Vector2(viewportXOffset, viewportYOffset);
		var ui = uiObject.GetComponent<PlayerRaceUI>();
		ui.playerId = playerId;
		screens.Add(new PlayerScreen { ui = ui, camera = camera, debug = uiObject.GetComponent<DebugUI>() });
	}
}