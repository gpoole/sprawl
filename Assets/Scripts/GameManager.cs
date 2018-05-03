using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public List<Player> players;

	public const int maxPlayers = 4;

	public bool debugMode;

	public static GameManager Instance {
		get;
		private set;
	}

	void Awake() {
		Instance = this;
	}

	void Update() {

	}

	public Player AddPlayer(InputDevice device) {
		if (players.Count + 1 >= maxPlayers) {
			Debug.LogError(String.Format("Tried to create more than {0} players", maxPlayers));
			return null;
		}

		var id = players.Count;
		var player = new Player { id = id, device = device };
		players.Add(player);
		return player;
	}
}