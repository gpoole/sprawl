using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerScreen : MonoBehaviour {

	public PlayerState playerState;

	public Camera playerCamera;

	public PlayerRaceUI ui;

	public DebugUI debug;

	// Use this for initialization
	void Start() {
		float viewportWidth = GameManager.Instance.players.Count > 1 ? 0.5f : 1f;
		float viewportHeight = GameManager.Instance.players.Count > 2 ? 0.5f : 1f;
		float viewportXOffset = viewportWidth * (playerState.player.id % 2);
		float viewportYOffset = viewportHeight * Mathf.Floor(playerState.player.id / 2f);
		playerCamera.rect = new Rect(viewportXOffset, viewportYOffset, viewportWidth, viewportHeight);

		var driftCamera = playerCamera.GetComponent<DriftCamera>();
		driftCamera.lookAtTarget = playerState.car.transform.Find("CameraPosition");
		driftCamera.positionTarget = playerState.car.transform.Find("CameraLookAtTarget");

		var uiRect = ui.transform.GetChild(0).GetComponent<RectTransform>();
		uiRect.anchorMax = new Vector2(viewportXOffset + viewportWidth, viewportYOffset + viewportHeight);
		uiRect.anchorMin = new Vector2(viewportXOffset, viewportYOffset);
		ui.playerState = playerState;
		debug = ui.GetComponent<DebugUI>();
	}

	// Update is called once per frame
	void Update() {

	}
}