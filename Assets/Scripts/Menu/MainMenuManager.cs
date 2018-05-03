using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class MainMenuManager : MonoBehaviour {

	public enum Screen {
		Main,
		CharacterSelect,
		TrackSelect
	}

	[Serializable]
	public class ReactiveScreenProperty : ReactiveProperty<Screen> {
		public ReactiveScreenProperty() : base() { }
		public ReactiveScreenProperty(Screen screen) : base(screen) { }
	}

	public ReactiveScreenProperty activeScreen = new ReactiveScreenProperty(Screen.Main);

	private Dictionary<Screen, GameObject> screenMap = new Dictionary<Screen, GameObject>();

	void Start() {
		CloneScreen(Screen.Main);
		CloneScreen(Screen.CharacterSelect);
		CloneScreen(Screen.TrackSelect);

		ChangeScreen(activeScreen.Value);

		activeScreen.Pairwise().Subscribe(screens => {
			ChangeScreen(screens.Current, screens.Previous);
		}).AddTo(this);
	}

	void ChangeScreen(Screen newScreen, Screen oldScreen) {
		var oldScreenObject = transform.Find(oldScreen.ToString());
		if (oldScreenObject) {
			Destroy(oldScreenObject.gameObject);
		}
		ChangeScreen(newScreen);
	}

	void ChangeScreen(Screen newScreen) {
		if (screenMap.ContainsKey(newScreen)) {
			var screenInstance = Instantiate(screenMap[newScreen], transform);
		}
	}

	void CloneScreen(Screen screen) {
		var screenRef = transform.Find(screen.ToString());
		if (screenRef) {
			screenMap.Add(screen, screenRef.gameObject);
			screenRef.gameObject.SetActive(true);
			Destroy(screenRef.gameObject);
		}
	}

}