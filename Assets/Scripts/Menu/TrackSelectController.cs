using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class TrackSelectController : MonoBehaviour {

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

		var input = new MenuActions();
		var menuControls = new MenuControls(input);
		menuControls
			.DirectionalControls()
			.Where(_ => selectionConfirmed.Value == false)
			.Select(GridCollectionUtils.DirectionFromMenuAction)
			.Subscribe(gridDirection => {
				selectedTrack.Value = trackGrid.GetFrom(selectedTrack.Value, gridDirection);
			})
			.AddTo(this);

		menuControls
			.NavigationControls()
			.Where(action => action == MenuControls.Action.Ok)
			.Subscribe(_ => {
				if (selectionConfirmed.Value) {
					Debug.Log("Ready to race");
					// mainMenuManager.activeScreen.Value = ??
				} else {
					selectionConfirmed.Value = true;
				}
			})
			.AddTo(this);

		menuControls
			.NavigationControls()
			.Where(action => action == MenuControls.Action.Back)
			.Subscribe(_ => {
				if (selectionConfirmed.Value) {
					selectionConfirmed.Value = false;
				} else {
					mainMenuManager.activeScreen.Value = MainMenuManager.Screen.CharacterSelect;
				}
			})
			.AddTo(this);

		selectionConfirmed.Subscribe(confirmLabel.SetActive).AddTo(this);
	}

}