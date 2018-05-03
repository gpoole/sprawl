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

	void Start() {
		var defaultScreen = GetScreen(activeScreen.Value);
		if (defaultScreen) {
			defaultScreen.SetActive(true);
		}

		activeScreen.Pairwise().Subscribe(screens => {
			var newScreen = GetScreen(screens.Current);
			if (newScreen) {
				newScreen.SetActive(true);
			}

			var oldScreen = GetScreen(screens.Previous);
			if (oldScreen) {
				oldScreen.SetActive(false);
			}
		}).AddTo(this);
	}

	GameObject GetScreen(Screen screen) {
		var screenObject = transform.Find(screen.ToString());
		if (screenObject) {
			return screenObject.gameObject;
		}
		return null;
	}

}