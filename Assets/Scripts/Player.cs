using System;
using System.Collections;
using System.Collections.Generic;
using InControl;
using UnityEngine;

[Serializable]
public class Player {

	public int id;

	public int number {
		get { return id + 1; }
	}

	public InputDevice device;

	public GameCharacter character;

}