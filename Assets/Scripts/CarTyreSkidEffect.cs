using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTyreSkidEffect : MonoBehaviour {

	private ParticleSystem smokeEffect;

	private TrailRenderer skidMark;

	private CarController car;

	private CarWheel wheel;

	// Use this for initialization
	void Start() {
		smokeEffect = GetComponentInChildren<ParticleSystem>();
		skidMark = GetComponentInChildren<TrailRenderer>();
		car = GetComponentInParent<CarController>();
		wheel = GetComponentInParent<CarWheel>();

		transform.SetParent(null, true);
	}

	// Update is called once per frame
	void Update() {
		if (car.IsDrifting && wheel.IsGrounded) {
			if (!smokeEffect.isPlaying) {
				smokeEffect.Play();
			}
			transform.position = wheel.HitSurface.point;
			transform.rotation = Quaternion.FromToRotation(Vector3.up, wheel.HitSurface.normal);
		} else {
			if (smokeEffect.isPlaying) {
				smokeEffect.Stop();
			}
		}
	}
}