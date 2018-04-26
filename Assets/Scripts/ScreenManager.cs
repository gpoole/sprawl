using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenManager : MonoBehaviour {

	public static ScreenManager Instance {
		get;
		private set;
	}

	public GameObject screenPrefab;

	public List<PlayerScreen> screens = new List<PlayerScreen>();

	void Awake() {
		Instance = this;
	}

	public PlayerScreen AddScreen(PlayerState playerState, Car car) {
		var screen = PlayerScreen.Create(screenPrefab, playerState, car);
		screen.transform.parent = transform;
		screens.Add(screen);
		return screen;
	}
}