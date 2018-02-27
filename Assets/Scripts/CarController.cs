using UnityEngine;
using System;

public class CarController : MonoBehaviour {

    public float turningFactor = 10f;

    public float maxTurningAngle = 30f;

    public float accelerationFactor = 10f;

    public float frictionFactor = 2f;

    public float suspensionSpringLength = 0.7f;

    public float suspensionDamping = 0f;

    public float suspensionSpring = 0f;

    private float wheelOrientation = 0f;

    void Update() {
        foreach (var wheel in GetComponentsInChildren<CarWheel>()) {
            wheel.springFactor = suspensionSpring;
            wheel.dampingFactor = suspensionDamping;
            wheel.targetLength = suspensionSpringLength;
        }
    }

    public float GetSpeed() {
        return GetComponent<Rigidbody>().velocity.magnitude;
    }

    void FixedUpdate() {
        var transform = GetComponent<Transform>();
        var colliderRb = GetComponent<Rigidbody>();

        var turning = Input.GetAxis("Horizontal");
        wheelOrientation = Mathf.Lerp(wheelOrientation, maxTurningAngle * turning, Time.deltaTime * 10f);

        var wheelRotation = Quaternion.AngleAxis(wheelOrientation, Vector3.up);
        // FIXME: needs to be relative to the road surface
        var wheelForwardDirection = wheelRotation * new Vector3(transform.forward.normalized.x, 0, transform.forward.normalized.z);
        // Debug.Log(wheelForwardDirection);

        var acceleration = Input.GetAxis("Vertical");
        colliderRb.AddForce(wheelForwardDirection * acceleration * accelerationFactor);

        // Rotate the car towards the direction of travel
        // FIXME: align to road I guess
        var travellingDirection = new Vector3(colliderRb.velocity.x, 0, colliderRb.velocity.z);
        if (travellingDirection.magnitude > 0) {
            var forwardDirection = new Vector3(transform.forward.normalized.x, 0, transform.forward.normalized.z);
            var steeringDifference = -Vector3.SignedAngle(travellingDirection, forwardDirection, Vector3.up);
            // We're reversing
            if (Math.Abs(steeringDifference) > 90) {
                steeringDifference = -Vector3.SignedAngle(travellingDirection, -forwardDirection, Vector3.up);
            }
            colliderRb.AddRelativeTorque(Vector3.up * steeringDifference * turningFactor * Time.deltaTime, ForceMode.Impulse);
        }

        var velocityDirection = colliderRb.velocity.normalized;
        var orientation = new Vector3(transform.right.x, 0, transform.right.z);
        var frictionVector = new Vector3(-velocityDirection.x, 0, -velocityDirection.z);
        var sidewaysness = Mathf.Abs(Vector3.Dot(velocityDirection, orientation));
        colliderRb.AddForce(frictionVector * colliderRb.velocity.magnitude * sidewaysness * frictionFactor);
    }
}
