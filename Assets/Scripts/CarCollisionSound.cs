using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarCollisionSound : MonoBehaviour {

	public AudioClip[] collisionSounds;

	public float minVelocity;

	private AudioSource source;

	// Use this for initialization
	void Start () {
		source = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void OnCollisionEnter(Collision collision) {
		if (!source.isPlaying && collision.relativeVelocity.magnitude > minVelocity) {
			source.clip = collisionSounds[Random.Range(0, collisionSounds.Length)];
			source.Play();
		}
	}
}
