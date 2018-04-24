using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Car : MonoBehaviour {

	public PlayerState playerState;

	public static Car Create(GameObject carPrefab, Transform startPosition, PlayerState playerState) {
		GameObject carGameObject;
		if (startPosition != null) {
			carGameObject = Instantiate(carPrefab, startPosition.transform.position, startPosition.transform.rotation);
		} else {
			carGameObject = Instantiate(carPrefab);
		}
		var car = carGameObject.GetComponent<Car>();
		car.playerState = playerState;
		return car;
	}

}