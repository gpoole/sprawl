using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LLRLightingMode : MonoBehaviour {

	public Light dayLight;

	public Light nightLight;

	public Material daySkybox;

	public Material nightSkyBox;

	public MeshRenderer water;

	public Material dayWater;

	public Material nightWater;

	public enum Mode {
		Daytime,
		Evening,
	}

	public Mode mode;

	void Start() {
		if (TrackManager.Instance != null) {
			try {
				mode = (Mode) Enum.Parse(typeof(Mode), TrackManager.Instance.GetSetting("lightingMode"));
			} catch (ArgumentException) {
				Debug.LogError("Failed to parse lightingMode setting.");
			}
		}
		ApplyMode();
	}

	void OnValidate() {
		ApplyMode();
	}

	void ApplyMode() {
		if (mode == Mode.Daytime) {
			nightLight.gameObject.SetActive(false);
			dayLight.gameObject.SetActive(true);
			RenderSettings.skybox = daySkybox;
			water.material = dayWater;
		} else {
			RenderSettings.skybox = nightSkyBox;
			nightLight.gameObject.SetActive(true);
			dayLight.gameObject.SetActive(false);
			water.material = nightWater;
		}
	}
}