using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LLRLightingMode : MonoBehaviour {

	public enum Mode {
		Daytime,
		Evening,
	}

	public Mode mode {
		get {
			if (!_mode.HasValue) {
				try {
					_mode = (Mode) Enum.Parse(typeof(Mode), TrackManager.Instance.GetSetting("lighting"));
				} catch (ArgumentException) {
					return Mode.Daytime;
				}
			}
			return _mode.Value;
		}
	}

	private Mode? _mode;

	void Start() {

	}

	void Update() {

	}
}