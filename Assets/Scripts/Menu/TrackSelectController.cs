using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class TrackSelectController : MonoBehaviour {

	private const int Columns = 3;

	public ReactiveProperty<Track> selectedTrack = new ReactiveProperty<Track>();

	public TrackList trackList;

	private MenuActions input;

	private GridCollection<Track> trackGrid;

	void Start() {
		input = new MenuActions();
		trackGrid = new GridCollection<Track>(trackList.tracks, Columns);

		selectedTrack.Value = trackGrid.First();

		GridNavigator.FromMenuActions(input)
			.Subscribe(direction => {
				selectedTrack.Value = trackGrid.GetFrom(selectedTrack.Value, direction);
			});
	}

}