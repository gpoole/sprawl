using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour {

	public static ScreenManager Instance {
		get;
		private set;
	}

	public GameObject cameraPrefab;

	public GameObject uiPrefab;

	public List<PlayerScreen> screens = new List<PlayerScreen>();

	// Use this for initialization
	void Awake() {
		Instance = this;
	}

	void Start() { }

	// Update is called once per frame
	void Update() {

	}

	public PlayerScreen AddScreen(PlayerState playerState, DriftCameraRig rig) {
		var playerScreen = (new GameObject("Screen" + playerState.player.id, typeof(PlayerScreen))).GetComponent<PlayerScreen>();
		playerScreen.transform.parent = transform;

		var cameraObject = Instantiate(cameraPrefab, playerScreen.transform);
		var uiObject = Instantiate(uiPrefab, playerScreen.transform);

		playerScreen.playerCamera = cameraObject.GetComponent<Camera>();
		cameraObject.GetComponent<DriftCamera>().rig = rig;
		playerScreen.ui = uiObject.GetComponent<PlayerRaceUI>();
		playerScreen.playerState = playerState;

		screens.Add(playerScreen);

		return playerScreen;
	}
}