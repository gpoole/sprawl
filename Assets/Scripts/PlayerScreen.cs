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

		playerState.mode.Where(mode => mode == PlayerState.PlayerMode.Starting).Subscribe(_ => StartCoroutine(PlayIntro()));
	}

	public void PlayOutro() {
		// ???
	}

	IEnumerator PlayIntro() {
		driftCamera.enabled = false;
		var positionTarget = driftCamera.rig.positionTarget;
		playerCamera.transform.position = positionTarget.transform.position - (positionTarget.transform.forward.normalized * 20f) + (Vector3.up * 20f);
		playerCamera.transform.rotation = positionTarget.transform.rotation;

		while (Vector3.Distance(playerCamera.transform.position, positionTarget.transform.position) > 1f) {
			playerCamera.transform.position = Vector3.Lerp(playerCamera.transform.position, positionTarget.transform.position, Time.deltaTime);
			playerCamera.transform.LookAt(driftCamera.rig.lookAtTarget);
			yield return null;
		}

		driftCamera.enabled = true;
	}
}