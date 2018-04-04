using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour {

	void Start() {
		CreateRacers();
	}

	void Update() {

	}

	void CreateRacers() {
		var starts = GameObject.FindGameObjectsWithTag("Start");
		var cameraManager = FindObjectOfType<CameraManager>();
		for (var i = 0; i < GameManager.Instance.players.Count; i++) {
			var player = GameManager.Instance.players[i];
			var carInstance = Instantiate(player.car, starts[i].transform.position, starts[i].transform.rotation);
			carInstance.GetComponent<CarController>().playerId = player.id;
			cameraManager.AddRaceCamera(carInstance, player.id);
		}
	}
}