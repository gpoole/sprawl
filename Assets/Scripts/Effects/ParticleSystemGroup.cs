using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ParticleSystemGroup : MonoBehaviour {

	public bool isPlaying {
		get;
		private set;
	}

	public ParticleSystem[] particles;

	public PlayableDirector[] playables;

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
		foreach (var playable in playables) {
			playable.gameObject.SetActive(true);
			playable.Play();
		}
	}

	public void Stop() {
		isPlaying = false;
		foreach (var particle in particles) {
			particle.Stop();
		}
		foreach (var playable in playables) {
			playable.Stop();
			playable.gameObject.SetActive(false);
		}
	}
}