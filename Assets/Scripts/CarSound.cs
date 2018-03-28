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

	private AudioSource carIdleSource;

	// Use this for initialization
	void Start() {
		car = GetComponent<CarController>();

		carEngineSource = (AudioSource) gameObject.AddComponent(typeof(AudioSource));
		carEngineSource.loop = true;
		carEngineSource.clip = engineMid;

		carIdleSource = (AudioSource) gameObject.AddComponent(typeof(AudioSource));
		carIdleSource.loop = true;
		carIdleSource.clip = engineIdle;
		carIdleSource.pitch = engineIdlePitch;
	}

	// Update is called once per frame
	void Update() {
		if (car.EngineSpeed > 0.02f) {
			var pitchRange = engineHighPitch - engineLowPitch;
			carEngineSource.pitch = engineLowPitch + (pitchRange * car.EngineSpeed);
			if (!carEngineSource.isPlaying) {
				carEngineSource.Play();
			}
			if (carIdleSource.isPlaying) {
				carIdleSource.Stop();
			}
			// CrossFade(0f, 1f);
		} else {
			if (!carIdleSource.isPlaying) {
				carIdleSource.Play();
			}
			if (carEngineSource.isPlaying) {
				carEngineSource.Stop();
			}
			// CrossFade(1f, 0f);
		}
	}

	void CrossFade(float carIdleVolume, float carEngineVolume) {
		carIdleSource.volume = Mathf.Lerp(carIdleSource.volume, carIdleVolume, Time.deltaTime * 10f);
		carEngineSource.volume = Mathf.Lerp(carEngineSource.volume, carEngineVolume, Time.deltaTime * 10f);
	}
}