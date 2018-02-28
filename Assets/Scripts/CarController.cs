﻿using UnityEngine;
using System;
using System.Linq;

public class CarController : MonoBehaviour {

    public float turningFactor = 10f;

    public float orientationCorrectionRate = 0.006f;

    public float maxTurningAngle = 30f;

    public float accelerationFactor = 10f;

    public float reverseAcceleration = 3f;

    public float enginePower = 10f;

    public float brakingFactor = 10f;

    public float frictionFactor = 2f;

    public float driftFactor = 1f;

    public float handbrakeDrift = 1f;

    public float suspensionSpringLength = 0.7f;

    public float suspensionDamping = 0f;

    public float suspensionSpring = 0f;

    public int playerNumber = 1;

    private float wheelOrientation = 0f;

    private bool isReversing = false;

    private float engineSpeed = 0f;

    void Update() {
        foreach (var wheel in GetWheels()) {
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

    private float GetBraking() {
        var brakingAmount = Input.GetAxis("P" + playerNumber + " Braking");
        var accelerator = GetAccelerator();
        if (accelerator < 0 && !isReversing) {
            brakingAmount = Math.Abs(accelerator);
        }
        return brakingAmount;
    }

    private bool IsHandbraking() {
        return Input.GetButton("P" + playerNumber + " Handbrake");
    }

    private CarWheel[] GetWheels() {
        return GetComponentsInChildren<CarWheel>();
    }

    private bool IsGrounded() {
        return GetWheels().Any(wheel => wheel.grounded);
    }

    private void ApplyDrivingForces() {
        var transform = GetComponent<Transform>();
        var colliderRb = GetComponent<Rigidbody>();

        var travellingDirection = new Vector3(colliderRb.velocity.x, 0, colliderRb.velocity.z);
        var relativeMovementDirection = transform.InverseTransformDirection(travellingDirection);
        isReversing = relativeMovementDirection.z < 0;

        var turning = GetTurning();
        wheelOrientation = Mathf.Lerp(wheelOrientation, maxTurningAngle * turning, Time.deltaTime * 50f);
        var wheelRotation = Quaternion.AngleAxis(wheelOrientation, Vector3.up);
        // FIXME: needs to be relative to the road surface
        var wheelForwardDirection = wheelRotation * new Vector3(transform.forward.normalized.x, 0, transform.forward.normalized.z);

        // Turn the car in the direction we're turning
        var reverseMultiplier = (isReversing) ? -1 : 1;
        colliderRb.AddRelativeTorque(Vector3.up * turning * relativeMovementDirection.z * turningFactor * reverseMultiplier);

        var accelerator = GetAccelerator();
        engineSpeed = Mathf.Lerp(engineSpeed, 0, Time.deltaTime);
        if (accelerator > 0) {
            engineSpeed += accelerator * accelerationFactor * Time.deltaTime;
        } else {
            engineSpeed += accelerator * reverseAcceleration * Time.deltaTime;
        }
        colliderRb.AddForce(wheelForwardDirection * engineSpeed * enginePower);

        // Assist steering by pushing the car sideways depending on how fast we're going
        if (turning != 0 && !isReversing) {
            colliderRb.AddRelativeForce(turning * relativeMovementDirection.z * driftFactor, 0, 0);
        }

        // Rotate the car towards the direction of travel
        // FIXME: align to road I guess
        var forwardDirection = new Vector3(transform.forward.normalized.x, 0, transform.forward.normalized.z);
        float steeringDifference;
        // forward
        if (!isReversing) {
            steeringDifference = -Vector3.SignedAngle(travellingDirection, forwardDirection, Vector3.up);
        } else {
            steeringDifference = -Vector3.SignedAngle(travellingDirection, -forwardDirection, Vector3.up);
        }

        if (IsHandbraking()) {
            if (!isReversing) {
                colliderRb.AddForce(wheelForwardDirection * travellingDirection.magnitude * handbrakeDrift);
                colliderRb.AddForce(-travellingDirection / 3f);
            }
        } else {
            if (travellingDirection.magnitude != 0) {
                colliderRb.AddRelativeTorque(Vector3.up * steeringDifference * travellingDirection.magnitude * orientationCorrectionRate * Time.deltaTime, ForceMode.Impulse);
            }
        }

        var velocityDirection = colliderRb.velocity.normalized;
        var orientation = new Vector3(transform.right.x, 0, transform.right.z);
        var frictionVector = new Vector3(-velocityDirection.x, 0, -velocityDirection.z);
        var sidewaysness = Mathf.Abs(Vector3.Dot(velocityDirection, orientation));
        colliderRb.AddForce(frictionVector * colliderRb.velocity.magnitude * sidewaysness * frictionFactor);

        var brakingAmount = GetBraking();
        if (brakingAmount > 0) {
            colliderRb.AddForce(-travellingDirection.normalized * brakingAmount * brakingFactor);
        }
    }

    void FixedUpdate() {
        if (IsGrounded()) {
            ApplyDrivingForces();
        }
    }
}
