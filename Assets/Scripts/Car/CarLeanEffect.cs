using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLeanEffect : MonoBehaviour {

	public float maxSidewaysTilt = 10f;

	public float maxForwardTilt = 5f;

	public float tiltResetSpeed = 2f;

	public float tiltForwardSpeed = 2f;

	public float minTiltSpeed = 15f;

	public Transform carBody;

	private CarController carController;

	new private Rigidbody rigidbody;

	private CarPlayerInput input;

	private float sidewaysTilt;

	private float forwardTilt;

	private float prevForwardSpeed;

	void Start() {
		carController = GetComponent<CarController>();
		rigidbody = GetComponent<Rigidbody>();
		input = GetComponent<CarPlayerInput>();
	}

	void FixedUpdate() {
		var isGrounded = carController.IsGrounded;

		if (isGrounded) {
			var surfaceVelocity = transform.InverseTransformDirection(rigidbody.velocity);
			var forwardSpeed = surfaceVelocity.z;
			var sidewaysSpeed = surfaceVelocity.x;
			sidewaysTilt = -Mathf.Clamp(sidewaysSpeed / 20f, -1, 1) * maxSidewaysTilt;
			forwardTilt = Mathf.Lerp(forwardTilt, Mathf.Clamp((forwardSpeed - prevForwardSpeed) / 0.5f, -1f, 1f) * maxForwardTilt, Time.deltaTime * tiltForwardSpeed);
			prevForwardSpeed = forwardSpeed;
		} else {
			sidewaysTilt = Mathf.Lerp(sidewaysTilt, 0, Time.deltaTime);
			forwardTilt = Mathf.Lerp(forwardTilt, 0, Time.deltaTime);
			prevForwardSpeed = 0;
		}

		carBody.transform.localRotation = Quaternion.AngleAxis(sidewaysTilt, Vector3.forward) * Quaternion.AngleAxis(forwardTilt, Vector3.left);
	}
}