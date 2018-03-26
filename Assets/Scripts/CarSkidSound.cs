using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSkidSound : MonoBehaviour {

	public AudioClip slideStart;

	public AudioClip slideLoop;

	public AudioClip slideEnd;

	public AudioClip[] skids;

	public float slideBeginSpeed;

	public float slideBeginAngle;

	public float skidVelocity;

	private AudioSource slideAudioSource;

	private AudioSource skidAudioSource;

	private CarController car;

	new private Rigidbody rigidbody;

	private float prevAngularVelocity = 0f;

	// Use this for initialization
	void Start() {
		slideAudioSource = (AudioSource) gameObject.AddComponent(typeof(AudioSource));
		skidAudioSource = (AudioSource) gameObject.AddComponent(typeof(AudioSource));
		car = GetComponent<CarController>();
		rigidbody = GetComponent<Rigidbody>();
	}

	// Update is called once per frame
	void Update() {
		var isSliding = car.IsGrounded && rigidbody.velocity.magnitude > slideBeginSpeed && car.VelocityAlignmentDifference > slideBeginAngle;

		if (isSliding) {
			if (!slideAudioSource.isPlaying) {
				slideAudioSource.clip = slideStart;
				slideAudioSource.loop = false;
				slideAudioSource.Play();
			} else if (slideAudioSource.clip != slideLoop) {
				slideAudioSource.loop = true;
				slideAudioSource.clip = slideLoop;
				slideAudioSource.Play();
			}
		} else if (slideAudioSource.isPlaying && slideAudioSource.clip != slideEnd) {
			slideAudioSource.clip = slideEnd;
			slideAudioSource.loop = false;
			slideAudioSource.Play();
		}

		// var wheelAlignmentChange = prevWheelAlignmentDifference - car.WheelAlignmentDifference;
		var angularVelocityChange = rigidbody.angularVelocity.magnitude - prevAngularVelocity;
		if (car.IsGrounded && angularVelocityChange > skidVelocity && !skidAudioSource.isPlaying) {
			skidAudioSource.clip = skids[Random.Range(0, skids.Length)];
			skidAudioSource.Play();
		}
		prevAngularVelocity = rigidbody.angularVelocity.magnitude;
	}
}