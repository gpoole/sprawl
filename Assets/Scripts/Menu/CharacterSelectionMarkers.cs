using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

public class CharacterSelectionMarkers : MonoBehaviour, ICharacterSelectionEventTarget {

	public GameObject markerPrefab;

	private GameCharacter character;

	void Start() {
		character = GetComponentInParent<CharacterSelectTile>().character;
	}

	void OnPlayerCharacterChanged(Player player, GameCharacter current) {
		if (current != character) {
			var newMarker = Instantiate(markerPrefab, transform);
			newMarker.GetComponent<CharacterSelectionMarker>().player = player;
		} else {
			var marker = GetPlayerMarker(player);
			if (marker != null) {
				Destroy(marker);
			}
		}
	}

	void OnPlayerCharacterConfirmed(Player player, GameCharacter current) {
		if (current == character) {
			var marker = GetPlayerMarker(player);
			if (marker) {
				marker.confirmed.Value = true;
			}
		}
	}

	void OnPlayerCharacterUnConfirmed(Player player, GameCharacter current) {
		if (current == character) {
			var marker = GetPlayerMarker(player);
			if (marker) {
				marker.confirmed.Value = false;
			}
		}
	}

	void OnPlayerRemoved(Player player) {
		var marker = GetPlayerMarker(player);
		if (marker != null) {
			Destroy(marker);
		}
	}

	void RemovePlayerMarker(Player player) {
		var marker = GetPlayerMarker(player);
		if (marker != null) {
			Destroy(marker);
		}
	}

	CharacterSelectionMarker GetPlayerMarker(Player player) {
		return GetComponentsInChildren<CharacterSelectionMarker>()
			.Where(marker => marker.player == player)
			.FirstOrDefault();
	}

}