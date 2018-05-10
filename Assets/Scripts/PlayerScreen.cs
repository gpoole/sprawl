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

	public PlayerState playerState;

	private static readonly Rect[][] viewportLayouts = {
		new Rect[] {
			new Rect(0, 0, 1f, 1f)
		},
		new Rect[] {
			new Rect(0, 0.5f, 1f, 0.5f),
				new Rect(0, 0, 1f, 0.5f)
		},
		new Rect[] {
			new Rect(0f, 0.5f, 0.5f, 0.5f),
				new Rect(0.5f, 0f, 0.5f, 0.5f),
				new Rect(0, 0.5f, 0.5f, 0.5f),
		},
		new Rect[] {
			new Rect(0f, 0.5f, 0.5f, 0.5f),
				new Rect(0.5f, 0, 0.5f, 0.5f),
				new Rect(0.5f, 0.5f, 0.5f, 0.5f),
				new Rect(0.5f, 0, 0.5f, 0.5f),
		}
	};

	void Start() {
		if (GameManager.Instance != null) {
			var excludeLayers = GameManager.Instance.players
				.Where(player => player != playerState.player)
				.Select(player => String.Format("P{0} Camera", player.number))
				.ToArray();
			playerCamera.cullingMask = playerCamera.cullingMask & ~LayerMask.GetMask(excludeLayers);
		}

		var cameraRect = viewportLayouts[GameManager.Instance.players.Count - 1][playerState.player.id];
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