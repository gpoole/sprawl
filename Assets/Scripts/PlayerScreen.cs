using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UniRx;
using UnityEngine;

public class PlayerScreen : MonoBehaviour {

	public Camera playerCamera;

	public PlayerRaceUI ui;

	private PlayerState playerState;

	void Start() {
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

	public static PlayerScreen Create(GameObject screenPrefab, PlayerState playerState, Car car) {
		var screenGameObject = Instantiate(screenPrefab);
		screenGameObject.GetComponent<PlayerScreen>().playerState = playerState;

		var virtualCamera = screenGameObject.GetComponentInChildren<CinemachineVirtualCamera>();
		virtualCamera.m_Follow = car.gameObject.transform;
		virtualCamera.m_LookAt = car.gameObject.transform;

		return screenGameObject.GetComponent<PlayerScreen>();
	}
}