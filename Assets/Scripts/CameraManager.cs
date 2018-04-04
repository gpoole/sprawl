using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour {

	public GameObject cameraPrefab;

	// Use this for initialization
	void Start() {

	}

	// Update is called once per frame
	void Update() {

	}

	public void AddRaceCamera(GameObject car, int playerId) {
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
	}
}