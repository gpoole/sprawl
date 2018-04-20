using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTyreSkidEffect : MonoBehaviour {

	public ParticleSystem smokeEffectPrefab;

	private ParticleSystem smokeEffect;

	private CarController car;

	private CarWheel wheel;

	// Use this for initialization
	void Start() {
		smokeEffect = Instantiate(smokeEffectPrefab);
		car = GetComponentInParent<CarController>();
		wheel = GetComponentInParent<CarWheel>();
	}

	// Update is called once per frame
	void FixedUpdate() {
		if (car.IsDrifting && wheel.IsGrounded) {
			if (smokeEffect && !smokeEffect.isPlaying) {
				smokeEffect.Play();
			}

			var position = transform.position;
			position.y = wheel.WheelBottom.y;
			transform.position = position;
		} else {
			if (smokeEffect && smokeEffect.isPlaying) {
				smokeEffect.Stop();
			}
		}
	}
}