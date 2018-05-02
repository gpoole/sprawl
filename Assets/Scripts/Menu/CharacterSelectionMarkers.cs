using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class CharacterSelectionMarkers : MonoBehaviour {

	public GameObject markerPrefab;

	public GameCharacter character;

	void Start() {
		var state = GetComponentInParent<CharacterSelectScreen>();

		var addedPlayers = state.playerSelections
			.ObserveAdd()
			.Select(ev => ev.Value)
			.StartWith(state.playerSelections);

		var characterChanges = addedPlayers
			.SelectMany(playerSelection => playerSelection.character.Select(_ => playerSelection));

		characterChanges
			.Where(playerSelection => playerSelection.character.Value == character)
			.Subscribe(playerSelection => {
				var marker = Instantiate(markerPrefab, transform);
				var characterSelectionMarker = marker.GetComponent<CharacterSelectionMarker>();
				characterSelectionMarker.player = playerSelection.player;
				characterSelectionMarker.confirmed = playerSelection.confirmed;
			})
			.AddTo(this);

		characterChanges
			.Where(playerSelection => playerSelection.character.Value != character)
			.Subscribe(playerSelection => {
				RemovePlayerMarker(playerSelection.player);
			})
			.AddTo(this);

		state.playerSelections
			.ObserveRemove()
			.Select(ev => ev.Value)
			.Where(playerSelection => playerSelection.character.Value == character)
			.Subscribe(playerSelection => {
				RemovePlayerMarker(playerSelection.player);
			})
			.AddTo(this);
	}

	void RemovePlayerMarker(Player player) {
		var marker = GetPlayerMarker(player);
		if (marker != null) {
			Destroy(marker.gameObject);
		}
	}

	CharacterSelectionMarker GetPlayerMarker(Player player) {
		return GetComponentsInChildren<CharacterSelectionMarker>()
			.Where(marker => marker.player == player)
			.FirstOrDefault();
	}

}