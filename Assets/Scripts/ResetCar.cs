using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetCar : MonoBehaviour {
    protected Vector3 startPosition;

	protected Quaternion startRotation;

	// Use this for initialization
	void Start () {
		var transform = GetComponent<Transform>();
		startPosition = transform.position;
		startRotation = transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKey(KeyCode.R)) {
			var transform = GetComponent<Transform>();
			transform.position = startPosition;
			transform.rotation = startRotation;
			GetComponent<Rigidbody>().velocity = Vector3.zero;
			GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}
	}
}
