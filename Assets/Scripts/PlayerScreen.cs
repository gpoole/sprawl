using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UniRx;
using UnityEngine;

public class PlayerScreen : MonoBehaviour {

	public Camera playerCamera;

	public GameObject followTarget;

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

		var excludeLayers = GameManager.Instance.players
			.Where(player => player != playerState.player)
			.Select(player => String.Format("P{0} Camera", player.number))
			.ToArray();
		Debug.Log(excludeLayers);
		playerCamera.cullingMask = playerCamera.cullingMask & ~LayerMask.GetMask(excludeLayers);

		var virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
		virtualCamera.gameObject.layer = LayerMask.NameToLayer(String.Format("P{0} Camera", playerState.player.number));
		virtualCamera.m_Follow = followTarget.transform;
		virtualCamera.m_LookAt = followTarget.transform;
	}

	public static PlayerScreen Create(GameObject screenPrefab, PlayerState playerState, Car car) {
		var screenGameObject = Instantiate(screenPrefab);

		var playerScreen = screenGameObject.GetComponent<PlayerScreen>();
		playerScreen.playerState = playerState;
		playerScreen.followTarget = car.gameObject;

		return playerScreen;
	}
}