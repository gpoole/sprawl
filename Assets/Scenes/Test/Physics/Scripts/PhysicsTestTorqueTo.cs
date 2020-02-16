using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PhysicsTestTorqueTo : MonoBehaviour {

	public float rotation;

	public float turnSpeed;

	new private Rigidbody rigidbody;

	void Start() {
		rigidbody = GetComponent<Rigidbody>();
	}

	void Update() {
		var currentRotation = transform.rotation.eulerAngles.y;
		var turnAmount = rotation - currentRotation;
		var newVelocity = (turnAmount * turnSpeed) - rigidbody.angularVelocity.y;
		rigidbody.AddTorque(Vector3.up * newVelocity, ForceMode.VelocityChange);
	}
}