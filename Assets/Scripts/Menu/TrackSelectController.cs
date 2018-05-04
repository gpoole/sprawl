using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TrackSelectController : MonoBehaviour, IMenuInputEventHandler {

	private const int Columns = 3;

	public ReactiveProperty<Track> selectedTrack = new ReactiveProperty<Track>();

	public BoolReactiveProperty selectionConfirmed = new BoolReactiveProperty(false);

	public TrackList trackList;

	public HideShowAnimation confirmPrompt;

	private GridCollection<Track> trackGrid;

	private MenuScreenManager menuScreenManager;

	void Start() {
		menuScreenManager = GetComponentInParent<MenuScreenManager>();
		trackGrid = new GridCollection<Track>(trackList.tracks, Columns);

		selectedTrack.Value = trackGrid.First();

		selectionConfirmed.Subscribe(confirmed => {
			if (confirmed) {
				confirmPrompt.Show();
			} else {
				confirmPrompt.Hide();
			}
		});
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
			menuScreenManager.GoTo("CharacterSelect");
		}
	}

	public void OnInputOk(InputDevice device) {
		if (!selectionConfirmed.Value) {
			selectionConfirmed.Value = true;
		} else {
			SceneManager.LoadScene(selectedTrack.Value.sceneName);
		}
	}

}