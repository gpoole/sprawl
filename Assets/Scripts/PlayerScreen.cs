using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using UniRx;
using UnityEngine;

public class PlayerScreen : MonoBehaviour {

	public Camera playerCamera;

	public Camera hudCamera;

	public GameObject followTarget;

	public PlayerRaceUI ui;

	private PlayerState playerState;

	void Start() {
		float viewportWidth = 1;
		float viewportHeight = 1;
		float viewportXOffset = 0;
		float viewportYOffset = 0;

		if (GameManager.Instance != null) {
			viewportWidth = GameManager.Instance.players.Count > 2 ? 0.5f : 1f;
			viewportHeight = GameManager.Instance.players.Count > 1 ? 0.5f : 1f;
			viewportXOffset = viewportWidth * Mathf.Floor(playerState.player.id / 2);
			// This is inverted, x/y starts at bottom left
			viewportYOffset = viewportHeight - (viewportHeight * ((playerState.player.id + 1) % 2));

			var excludeLayers = GameManager.Instance.players
				.Where(player => player != playerState.player)
				.Select(player => String.Format("P{0} Camera", player.number))
				.ToArray();
			playerCamera.cullingMask = playerCamera.cullingMask & ~LayerMask.GetMask(excludeLayers);
		}

		var cameraRect = new Rect(viewportXOffset, viewportYOffset, viewportWidth, viewportHeight);
		playerCamera.rect = cameraRect;
		hudCamera.rect = cameraRect;

		if (followTarget != null) {
			var virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
			virtualCamera.m_Follow = followTarget.transform;
			virtualCamera.m_LookAt = followTarget.transform;

			if (playerState != null) {
				virtualCamera.gameObject.layer = LayerMask.NameToLayer(String.Format("P{0} Camera", playerState.player.number));
			}
		}

		if (playerState != null) {
			ui.playerState = playerState;
		}
	}

	public static PlayerScreen Create(GameObject screenPrefab, PlayerState playerState, Car car) {
		var screenGameObject = Instantiate(screenPrefab);

		var playerScreen = screenGameObject.GetComponent<PlayerScreen>();
		playerScreen.playerState = playerState;
		playerScreen.followTarget = car.gameObject;

		return playerScreen;
	}
}