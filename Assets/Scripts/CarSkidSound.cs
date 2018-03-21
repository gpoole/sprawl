using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSkidSound : MonoBehaviour {

	public AudioClip start;

	public AudioClip loop;

	public AudioClip end;

	public float skidBeginSpeed;

	public float skidBeginAngle;

	private AudioSource audioSource;

	private CarController car;

	new private Rigidbody rigidbody;

	// Use this for initialization
	void Start () {
		audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
		car = GetComponent<CarController>();
		rigidbody = GetComponent<Rigidbody>();
	}
	
	// Update is called once per frame
	void Update () {
		var isSkidding = car.IsGrounded && rigidbody.velocity.magnitude > skidBeginSpeed && car.WheelAlignmentDifference > skidBeginAngle;

		if (isSkidding) {
			if (!audioSource.isPlaying) {
				audioSource.clip = start;
				audioSource.loop = false;
				audioSource.Play();
			} else if (audioSource.clip != loop) {
				audioSource.loop = true;
				audioSource.clip = loop;
				audioSource.Play();
			}
		} else if (audioSource.isPlaying && audioSource.clip != end) {
			audioSource.clip = end;
			audioSource.loop = false;
			audioSource.Play();
		}
	}
}
