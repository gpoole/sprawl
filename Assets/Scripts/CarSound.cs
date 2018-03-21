using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSound : MonoBehaviour {

	public AudioClip engineIdle;

	public AudioClip engineMid;

	[Range(-3, 3)]
	public float engineLowPitch;

	[Range(-3, 3)]
	public float engineHighPitch;

	[Range(-3, 3)]
	public float engineIdlePitch;

	private CarController car;

	private AudioSource carEngineSource;

	// Use this for initialization
	void Start () {
		car = GetComponent<CarController>();

		carEngineSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
	}
	
	// Update is called once per frame
	void Update () {
		if (car.EngineSpeed > 0.05) {
			var pitchRange = engineHighPitch - engineLowPitch;
			carEngineSource.pitch = engineLowPitch + (pitchRange * (car.EngineSpeed / car.maxEngineSpeed));
			ChangeClip(engineMid);
		} else {
			carEngineSource.pitch = engineIdlePitch;
			ChangeClip(engineIdle);
		}
	}

	void ChangeClip(AudioClip clip) {
		if (carEngineSource.clip != clip) {
			carEngineSource.clip = clip;
			carEngineSource.Play();
		}
	}
}
