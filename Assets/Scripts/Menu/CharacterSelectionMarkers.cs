using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class CharacterSelectionMarkers : MonoBehaviour {

	public GameObject markerPrefab;

	private GameCharacter character;

	// Use this for initialization
	void Start() {
		var state = GetComponentInParent<CharacterSelectScreen>();
		character = GetComponentInParent<CharacterSelectTile>().character;

		foreach (var playerSelection in state.playerSelections) {
			WatchPlayerSelection(playerSelection);
		}

		state.playerSelections.ObserveAdd().Subscribe(ev => {
			WatchPlayerSelection(ev.Value);
		}).AddTo(this);

		state.playerSelections.ObserveRemove().Subscribe(ev => {
			RemovePlayerMarker(ev.Value.player);
		}).AddTo(this);
	}

	void WatchPlayerSelection(CharacterSelectScreen.PlayerSelection playerSelection) {
		playerSelection.character
			.Where(playerCharacter => playerCharacter == character)
			.Subscribe(_ => {
				var newMarker = Instantiate(markerPrefab, transform);
				newMarker.GetComponent<CharacterSelectionMarker>().player = playerSelection.player;
			}).AddTo(this);

		playerSelection.character
			.Where(playerCharacter => playerCharacter != character)
			.Subscribe(_ => {
				RemovePlayerMarker(playerSelection.player);
			}).AddTo(this);
	}

	void RemovePlayerMarker(Player player) {
		var markers = GetComponentsInChildren<CharacterSelectionMarker>()
			.Where(marker => marker.player == player);
		foreach (var marker in markers) {
			Destroy(marker);
		}
	}

}