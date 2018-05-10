using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCharacterPalette : MonoBehaviour {

	public MeshRenderer carMesh;

	void Start() {
		var car = GetComponent<Car>();
		if (car != null && car.playerState != null && car.playerState.player.character.carSkin != null) {
			carMesh.material = car.playerState.player.character.carSkin;
		}
	}
}