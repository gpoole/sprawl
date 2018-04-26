using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarLeanEffect : MonoBehaviour {

	public float maxSidewaysTilt = 10f;

	public float maxForwardTilt = 5f;

	public float leanSpeed = 2f;

	public float minTiltSpeed = 15f;

	public Transform carBody;

	private CarController carController;

	new private Rigidbody rigidbody;

	private CarPlayerInput input;

	private float sidewaysTilt;

	private float forwardTilt;

	private float prevSurfaceSpeed;

	void Start() {
		carController = GetComponent<CarController>();
		rigidbody = GetComponent<Rigidbody>();
		input = GetComponent<CarPlayerInput>();
	}

	void Update() {
		var isGrounded = carController.IsGrounded;
		var surfaceSpeed = transform.InverseTransformDirection(rigidbody.velocity).z;

		float sidewaysTilt = 0;
		float forwardTilt = 0;

		if (isGrounded) {
			sidewaysTilt = Mathf.Abs(Mathf.Clamp01(surfaceSpeed / minTiltSpeed)) * input.Turning * maxSidewaysTilt;
			forwardTilt = (Mathf.Clamp(surfaceSpeed - prevSurfaceSpeed, -3f, 3f) / 3f) * maxForwardTilt;
		}

		var rotation = Quaternion.AngleAxis(sidewaysTilt, Vector3.forward) * Quaternion.AngleAxis(forwardTilt, Vector3.left);
		carBody.transform.localRotation = Quaternion.Lerp(carBody.transform.localRotation, rotation, Time.deltaTime * leanSpeed);
		prevSurfaceSpeed = surfaceSpeed;
	}
}