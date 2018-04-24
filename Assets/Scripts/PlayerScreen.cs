using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class PlayerScreen : MonoBehaviour {

	public PlayerState playerState;

	public Camera playerCamera;

	public PlayerRaceUI ui;

	private DriftCamera driftCamera;

	// Use this for initialization
	void Start() {
		driftCamera = playerCamera.GetComponent<DriftCamera>();

		float viewportWidth = GameManager.Instance.players.Count > 1 ? 0.5f : 1f;
		float viewportHeight = GameManager.Instance.players.Count > 2 ? 0.5f : 1f;
		float viewportXOffset = viewportWidth * (playerState.player.id % 2);
		float viewportYOffset = viewportHeight * Mathf.Floor(playerState.player.id / 2f);
		playerCamera.rect = new Rect(viewportXOffset, viewportYOffset, viewportWidth, viewportHeight);

		var uiRect = ui.transform.GetChild(0).GetComponent<RectTransform>();
		uiRect.anchorMax = new Vector2(viewportXOffset + viewportWidth, viewportYOffset + viewportHeight);
		uiRect.anchorMin = new Vector2(viewportXOffset, viewportYOffset);
		ui.playerState = playerState;
	}
}