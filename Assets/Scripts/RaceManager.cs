using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceManager : MonoBehaviour {

	public static RaceManager Instance {
		get;
		private set;
	}

	private GameObject[] starts;

	private List<GameObject> cars = new List<GameObject>();

	void Awake() {
		Instance = this;
	}

	void Start() {
		starts = GameObject.FindGameObjectsWithTag("Start");
		CreateRacers();
	}

	void Update() {

	}

	void CreateRacers() {
		var cameraManager = FindObjectOfType<CameraManager>();
		for (var i = 0; i < GameManager.Instance.players.Count; i++) {
			var player = GameManager.Instance.players[i];
			var carInstance = Instantiate(player.car, starts[i].transform.position, starts[i].transform.rotation);
			cars.Add(carInstance);
			carInstance.GetComponent<CarController>().playerId = player.id;
			cameraManager.AddRaceCamera(carInstance, player.id);
		}
	}

	public void ResetPlayer(int id) {
		cars[id].GetComponent<CarController>().Reset();
		cars[id].transform.position = starts[id].transform.position;
		cars[id].transform.rotation = starts[id].transform.rotation;
	}
}