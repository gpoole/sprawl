using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarSuspensionSound : MonoBehaviour {

	public AudioClip[] compress;

	public AudioClip[] extend;

	[Range(0, 1)]
	public float changeTrigger;

	private AudioSource audioSource;

	private CarWheel wheel;

	private float prevCompression;

	void Start () {
		audioSource = (AudioSource)gameObject.AddComponent(typeof(AudioSource));
		wheel = GetComponent<CarWheel>();
	}
	
	void Update () {
		var compressionChange = wheel.Compression - prevCompression;
		if (!audioSource.isPlaying && Mathf.Abs(compressionChange) > changeTrigger) {
			if (compressionChange > 0) {
				audioSource.clip = compress[Random.Range(0, compress.Length)];
				audioSource.Play();
			} else if (compressionChange < 0) {
				audioSource.clip = extend[Random.Range(0, extend.Length)];
				audioSource.Play();
			}
		}
	}
}
