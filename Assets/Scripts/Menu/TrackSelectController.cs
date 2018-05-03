using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UniRx;
using UnityEngine;

public class TrackSelectController : MonoBehaviour, IMenuInputEventHandler {

	private const int Columns = 3;

	public ReactiveProperty<Track> selectedTrack = new ReactiveProperty<Track>();

	public BoolReactiveProperty selectionConfirmed = new BoolReactiveProperty(false);

	public TrackList trackList;

	public GameObject confirmLabel;

	private GridCollection<Track> trackGrid;

	private MainMenuManager mainMenuManager;

	void Start() {
		mainMenuManager = GetComponentInParent<MainMenuManager>();
		trackGrid = new GridCollection<Track>(trackList.tracks, Columns);

		selectedTrack.Value = trackGrid.First();

		selectionConfirmed.Subscribe(confirmLabel.SetActive);
	}

	public void OnInputAction(InputAction action, InputDevice player) {
		switch (action) {
			case InputAction.Up:
			case InputAction.Down:
			case InputAction.Left:
			case InputAction.Right:
				selectedTrack.Value = trackGrid.GetFrom(selectedTrack.Value, GridCollectionUtils.DirectionFromMenuAction(action));
				break;
		}
	}

	public void OnInputBack(InputDevice device) {
		if (selectionConfirmed.Value) {
			selectionConfirmed.Value = false;
		} else {
			mainMenuManager.activeScreen.Value = MainMenuManager.Screen.CharacterSelect;
		}
	}

	public void OnInputOk(InputDevice device) {
		if (!selectionConfirmed.Value) {
			selectionConfirmed.Value = true;
		} else {
			// Done!
			Debug.Log("Ready to race");
		}
	}

}