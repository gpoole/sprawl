using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;

[Serializable]
public class Player {

	public int id;

	public InputDevice device {
		get {
			if (id >= InputManager.Devices.Count) {
				Debug.LogError(String.Format("Trying to get device for player {0} with only {1} devices", id, InputManager.Devices.Count));
			}
			return InputManager.Devices[id];
		}
	}

	public GameObject car;

}