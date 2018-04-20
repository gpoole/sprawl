using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSystemGroup : MonoBehaviour {

	public bool isPlaying {
		get;
		private set;
	}

	public ParticleSystem[] particles;

	public bool playOnAwake;

	void Awake() {
		if (playOnAwake) {
			Play();
		}
	}

	public void Play() {
		isPlaying = true;
		foreach (var particle in particles) {
			particle.Play();
		}
	}

	public void Stop() {
		isPlaying = false;
		foreach (var particle in particles) {
			particle.Stop();
		}
	}
}