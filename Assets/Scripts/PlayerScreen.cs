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
		float viewportWidth = 1;
		float viewportHeight = 1;
		float viewportXOffset = 0;
		float viewportYOffset = 0;

		if (GameManager.Instance != null) {
			viewportWidth = GameManager.Instance.players.Count > 1 ? 0.5f : 1f;
			viewportHeight = GameManager.Instance.players.Count > 2 ? 0.5f : 1f;
			viewportXOffset = viewportWidth * (playerState.player.id % 2);
			viewportYOffset = viewportHeight * Mathf.Floor(playerState.player.id / 2f);

			var excludeLayers = GameManager.Instance.players
				.Where(player => player != playerState.player)
				.Select(player => String.Format("P{0} Camera", player.number))
				.ToArray();
			playerCamera.cullingMask = playerCamera.cullingMask & ~LayerMask.GetMask(excludeLayers);
		}

		playerCamera.rect = new Rect(viewportXOffset, viewportYOffset, viewportWidth, viewportHeight);
		ui.SetDimensions(viewportXOffset, viewportYOffset, viewportWidth, viewportHeight);

		var virtualCamera = GetComponentInChildren<CinemachineVirtualCamera>();
		virtualCamera.m_Follow = followTarget.transform;
		virtualCamera.m_LookAt = followTarget.transform;

		if (playerState != null) {
			virtualCamera.gameObject.layer = LayerMask.NameToLayer(String.Format("P{0} Camera", playerState.player.number));
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