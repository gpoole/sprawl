using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

public class CarBooster : MonoBehaviour {

	public PlayerState playerState;

	[Serializable]
	public struct StartBoost {
		public float boostForce;
		public float duration;
		public float timing;
	};

	public StartBoost startBoost;

	public float cooldownTime = 2f;

	public bool canBoost {
		get;
		private set;
	}

	public AudioClip boostClip;

	public ParticleSystemGroup boostFlameEffectPrefab;

	public Transform exhaustPosition;

	private ParticleSystemGroup boostFlameEffect;

	new private Rigidbody rigidbody;

	private AudioSource boostSound;

	// Use this for initialization
	void Start() {
		rigidbody = GetComponent<Rigidbody>();
		boostFlameEffect = Instantiate(boostFlameEffectPrefab, exhaustPosition);
		boostSound = (AudioSource) gameObject.AddComponent(typeof(AudioSource));
		boostSound.clip = boostClip;
		boostSound.loop = true;

		playerState.mode.Where(mode => mode == PlayerState.PlayerMode.Starting).Subscribe(_ => {
			StartCoroutine(WatchForStartBoost());
		});
	}

	public void ApplyBoost(float force, float duration) {
		StartCoroutine(RunBoost(force, duration));
	}

	IEnumerator WatchForStartBoost() {
		var input = GetComponent<CarPlayerInput>();
		bool doBoost = false;
		while (playerState.startCountdown.Value > 0) {
			var countdown = playerState.startCountdown.Value;
			if (countdown > startBoost.timing && input.Accelerator == 0) {
				yield return null;
			} else if (countdown < startBoost.timing) {
				if (input.Accelerator == 1) {
					doBoost = true;
				} else {
					doBoost = false;
				}
				yield return null;
			} else {
				break;
			}
		}

		if (doBoost) {
			ApplyBoost(startBoost.boostForce, startBoost.duration);
		}
	}

	IEnumerator RunBoost(float force, float duration) {
		canBoost = false;
		boostSound.Play();
		boostFlameEffect.Play();
		for (float timer = duration; timer > 0; timer -= Time.deltaTime) {
			rigidbody.AddRelativeForce(Vector3.forward * force, ForceMode.Acceleration);
			yield return null;
		}
		boostFlameEffect.Stop();
		boostSound.Stop();
		yield return new WaitForSeconds(cooldownTime);
		canBoost = true;
	}
}