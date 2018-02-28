using UnityEngine;
using System;

public class CarController : MonoBehaviour {

    public float turningFactor = 10f;

    public float maxTurningAngle = 30f;

    public float accelerationFactor = 10f;

    public float frictionFactor = 2f;

    public float driftFactor = 1f;

    public float suspensionSpringLength = 0.7f;

    public float suspensionDamping = 0f;

    public float suspensionSpring = 0f;

    public int playerNumber = 1;

    private float wheelOrientation = 0f;

    void Update() {
        foreach (var wheel in GetComponentsInChildren<CarWheel>()) {
            wheel.springFactor = suspensionSpring;
            wheel.dampingFactor = suspensionDamping;
            wheel.targetLength = suspensionSpringLength;
        }
    }

    private float GetTurning() {
        return Input.GetAxis("P" + playerNumber + " Steering");
    }

    private float GetAccelerator() {
        return Input.GetAxis("P" + playerNumber + " Accelerator");
    }

    void FixedUpdate() {
        var transform = GetComponent<Transform>();
        var colliderRb = GetComponent<Rigidbody>();

        var turning = GetTurning();
        wheelOrientation = Mathf.Lerp(wheelOrientation, maxTurningAngle * turning, Time.deltaTime * 50f);

        var travellingDirection = new Vector3(colliderRb.velocity.x, 0, colliderRb.velocity.z);
        var relativeMovementDirection = transform.InverseTransformDirection(travellingDirection);
        var isReversing = relativeMovementDirection.z < 0;

        var wheelRotation = Quaternion.AngleAxis(wheelOrientation, Vector3.up);
        // FIXME: needs to be relative to the road surface
        var wheelForwardDirection = wheelRotation * new Vector3(transform.forward.normalized.x, 0, transform.forward.normalized.z);
        // Debug.Log(wheelForwardDirection);

        var accelerator = GetAccelerator();
        colliderRb.AddForce(wheelForwardDirection * accelerator * accelerationFactor);

        // Assist steering by pushing the car sideways depending on how fast we're going
        if (turning != 0) {
            colliderRb.AddRelativeForce(turning * relativeMovementDirection.z * driftFactor, 0, 0);
        }

        // Rotate the car towards the direction of travel
        // FIXME: align to road I guess
        if (travellingDirection.magnitude != 0) {
            var forwardDirection = new Vector3(transform.forward.normalized.x, 0, transform.forward.normalized.z);
            float steeringDifference;
            // forward
            if (!isReversing) {
                steeringDifference = -Vector3.SignedAngle(travellingDirection, forwardDirection, Vector3.up);
            } else {
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
