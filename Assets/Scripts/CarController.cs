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
        // colliderRb.AddForce(transform.TransformDirection(Vector3.forward) * acceleration * accelerationFactor);
        // var momentumRotation = Quaternion.FromToRotation(transform.TransformDirection(Vector3.forward), wheelForwardDirection);
        // colliderRb.AddTorque(momentumRotation * )
        // var momentumRotation = Vector3.Slerp(transform.TransformDirection(Vector3.forward), wheelForwardDirection, Time.deltaTime * 20f);
        // Quaternion.
        // colliderRb.AddTorque();
        // Debug.Log(wheelRotation);
        // Debug.Log(wheelRotation * transform.forward);
        // Debug.Log(Quaternion.LookRotation())
        // colliderRb.AddTorque(wheelRotation * transform.position);
        // colliderRb.AddTorque((wheelRotation * transform.forward) * 50f);
        // Vector3.AngleBetween()
        // var momentumRotation = Quaternion.FromToRotation(transform.forward, colliderRb.velocity.normalized);
        // Debug.Log("forward=" + transform.forward);
        // Debug.Log("velocity=" + colliderRb.velocity);
        // Debug.Log(Vector3.Angle(transform.forward, colliderRb.velocity.normalized));
        // colliderRb.AddRelativeTorque(wheelRotation * colliderRb.velocity.normalized);

        // Rotate the car towards the wheels depending on how fast we're going
        // FIXME: align to road I guess
        var travellingDirection = new Vector3(colliderRb.velocity.x, 0, colliderRb.velocity.z);
        var forwardDirection = new Vector3(transform.forward.normalized.x, 0, transform.forward.normalized.z);
        if (travellingDirection.magnitude > 0) {
            var steeringDifference = -Vector3.SignedAngle(travellingDirection, forwardDirection, Vector3.up);
            Debug.Log(steeringDifference);
            // Debug.Log(steeringDifference);
            colliderRb.AddRelativeTorque(Vector3.up * steeringDifference * turningFactor * Time.deltaTime, ForceMode.Impulse);
        }

        // Debug.Log(steeringDifference);
        // Debug.Log("surf=" + surfaceVelocity);
        // Debug.Log("forward=" + wheelForwardDirection);
        // if (surfaceVelocity.magnitude > 0) {
        //     Debug.Log("diff=" + (surfaceVelocity.normalized - forwardDirection));
        // }
        // var rotationToTravellingDirection = Quaternion.FromToRotation(forwardDirection, travellingDirection.normalized);
        // Debug.Log(rotationToTravellingDirection);
        // colliderRb.AddRelativeTorque(Vector3.RotateTowards());

        // colliderRb.AddRelativeTorque(Vector3.up * wheelOrientation * turningFactor * Time.deltaTime, ForceMode.Impulse);

        // colliderRb.AddRelativeTorque(Vector3.up * wheelOrientation * turningFactor);

        var velocityDirection = colliderRb.velocity.normalized;
        var orientation = new Vector3(transform.right.x, 0, transform.right.z);
        var frictionVector = new Vector3(-velocityDirection.x, 0, -velocityDirection.z);
        var sidewaysness = Mathf.Abs(Vector3.Dot(velocityDirection, orientation));
        colliderRb.AddForce(frictionVector * colliderRb.velocity.magnitude * sidewaysness * frictionFactor);
    }
}
