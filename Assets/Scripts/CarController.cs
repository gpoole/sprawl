﻿using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CarController : MonoBehaviour {

    public float turnVelocityTransferRate = 1f;

    public float driftTurnVelocityTransferRate = 2f;

    public float stoppedSpeed = 0.5f;

    public float turnSpeed = 1f;

    public float driftTurnSpeed = 2f;

    public float maxSpeed = 60f;

    public float maxReverseSpeed = -30f;

    public AnimationCurve gripPower;

    public AnimationCurve sidewaysFriction;

    public float maxWheelTurn = 10f;

    public float driftBrakePower = 1f;

    public float driftMaxTurnAngle = 80f;

    public float maxTurnAngle = 30f;

    public float engineSpeedIncreaseRate = 1f;

    public float engineSpeedDecreaseRate = 1f;

    public float enginePower = 10f;

    public float maxEngineSpeed = 3f;

    public float defaultSurfaceFriction = 1f;

    public float straightenUpTime = 3f;

    public float straightenUpAngle = 15f;

    public float minSlideSpeed = 40f;

    public int playerNumber = 1;

    public AnimationCurve brakingPower;

    public float brakingForceMultiplier = 10f;

    public float WheelAlignmentDifference {
        get;
        private set;
    }

    public float WheelOrientation {
        get;
        private set;
    }

    public float EngineSpeed {
        get;
        private set;
    }

    public float Speed {
        get;
        private set;
    }

    public bool IsGrounded {
        get { return wheels.Any(wheel => wheel.Grounded); }
    }

    private bool isReversing = false;

    private bool isStopped = true;

    private float braking;

    private CarPlayerInput input;

    private Vector3 wheelForwardDirection;

    private CarDebugger debugger;

    private Rigidbody rb;

    private CarWheel[] wheels;

    private bool isSliding;

    private float slideTimer;

    private float surfaceFriction = 1f;

    void Start() {
        EngineSpeed = 0;
        Speed = 0;
        WheelOrientation = 0;

        debugger = GetComponent<CarDebugger>();
        input = GetComponent<CarPlayerInput>();
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<CarWheel>();
    }

    public void Reset() {
        EngineSpeed = 0;
        Speed = 0;
        WheelOrientation = 0;
        rb.ResetInertiaTensor();
    }

    private void ApplyDrivingForces() {
        var forwardVelocity = Vector3.Project(rb.velocity, transform.forward);
        Speed = forwardVelocity.z;
        debugger.ShowDebugValue("speed", Speed);
        isReversing = Speed < 0;
        isStopped = Math.Abs(Speed) < stoppedSpeed;
        debugger.ShowDebugValue("isReversing", isReversing);

        var wheelRotation = Quaternion.AngleAxis(WheelOrientation, Vector3.up);
        var wheelSidewaysRotation = Quaternion.AngleAxis(90, Vector3.up);
        wheelForwardDirection = transform.TransformDirection(wheelRotation * Vector3.forward).normalized;
        var wheelRight = transform.TransformDirection(wheelRotation * wheelSidewaysRotation * Vector3.forward);

        // Drive the car forward in the direction of the wheels
        WheelAlignmentDifference = Math.Abs(Vector3.SignedAngle(wheelForwardDirection, rb.velocity, transform.up));
        if (WheelAlignmentDifference > 90) {
            WheelAlignmentDifference = 180 - WheelAlignmentDifference;
        }
        debugger.ShowDebugValue("absWheelAlignmentDifference", WheelAlignmentDifference);
        // Calculate the grip so it increases rapidly as we get towards fully forward-facing
        var forwardGrip = gripPower.Evaluate(WheelAlignmentDifference / 90);
        var forwardMovementForce = EngineSpeed * forwardGrip * surfaceFriction * enginePower * forwardGrip;
        debugger.ShowDebugValue("forwardMovementForce", forwardMovementForce);
        var forwardDrivingForce = wheelForwardDirection * forwardMovementForce * Time.deltaTime;
        debugger.ShowDebugValue("forwardGrip", forwardGrip);
        rb.AddForce(forwardDrivingForce, ForceMode.Acceleration);

        if (!isReversing && input.IsHandbraking && Speed > minSlideSpeed) {
            isSliding = true;
            slideTimer = straightenUpTime;
        } else if (WheelAlignmentDifference < straightenUpAngle) {
            if (slideTimer > 0) {
                slideTimer -= Time.deltaTime;
            } else {
                isSliding = false;
            }
        }

        debugger.ShowDebugValue("isSliding", isSliding);
        debugger.ShowDebugValue("slideTimer", slideTimer, false);

        var angleToMovement = Mathf.Abs(Vector3.SignedAngle(transform.forward, rb.velocity, transform.up));
        if (angleToMovement > 90) {
            angleToMovement = 180 - angleToMovement;
        }
        var sidewaysness = angleToMovement / 90;

        // Transfer velocity from the direction of movement to the direction of the wheels at an increasing rate depending on
        // how far we're turned away from the direction of movement
        var velocityTransferAmount = Speed * turnVelocityTransferRate * sidewaysness * forwardGrip;
        debugger.ShowDebugValue("velocityTransferAmount", velocityTransferAmount);
        rb.AddForce(wheelForwardDirection * velocityTransferAmount * Time.deltaTime, ForceMode.VelocityChange);

        // Turn the car up to a maximum angle against the direction of movement
        var allowedTurnAngle = isSliding ? driftMaxTurnAngle : maxTurnAngle;
        var turnSpeed = isSliding ? driftTurnSpeed : this.turnSpeed;
        if (WheelAlignmentDifference < allowedTurnAngle) {
            rb.AddTorque(Vector3.Cross(transform.forward, wheelForwardDirection) * turnSpeed * Time.deltaTime, ForceMode.VelocityChange);
        }

        var sideFriction = sidewaysFriction.Evaluate(sidewaysness);
        var sideways = Vector3.Project(transform.InverseTransformDirection(rb.velocity), Vector3.right);
        rb.AddForce(-sideways * sideFriction * surfaceFriction * Time.deltaTime, ForceMode.VelocityChange);

        if (!isReversing) {
            float baseBraking = 0;
            if (isSliding) {
                baseBraking = driftBrakePower;
            } else {
                baseBraking = braking;
            }

            var brakeForce = baseBraking * brakingPower.Evaluate(Speed / maxSpeed) * brakingForceMultiplier * forwardGrip * surfaceFriction;
            debugger.ShowDebugValue("brakeForce", brakeForce);
            rb.AddForce(-rb.velocity.normalized * brakeForce * Time.deltaTime, ForceMode.VelocityChange);
        }
    }

    void Update() {
        if (input.Accelerator > 0) {
            EngineSpeed += input.Accelerator * engineSpeedIncreaseRate * Time.deltaTime;
        }

        if ((isReversing || isStopped) && input.Brakes > 0) {
            EngineSpeed -= input.Brakes * engineSpeedIncreaseRate * Time.deltaTime;
        }

        EngineSpeed = Mathf.Clamp(EngineSpeed, -maxEngineSpeed, maxEngineSpeed);
        EngineSpeed = Mathf.Lerp(EngineSpeed, 0, engineSpeedDecreaseRate * Time.deltaTime);
        debugger.ShowDebugValue("engineSpeed", EngineSpeed);

        WheelOrientation = Mathf.Lerp(WheelOrientation, maxWheelTurn * input.Turning, Time.deltaTime * 50f);
        debugger.ShowDebugValue("wheelOrientation", WheelOrientation);

        braking = input.Brakes;
    }

    void FixedUpdate() {
        if (IsGrounded) {
            RaycastHit roadSurface;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out roadSurface, 1f)) {
                surfaceFriction = roadSurface.collider.material.dynamicFriction;
            } else {
                surfaceFriction = defaultSurfaceFriction;
            }
            debugger.ShowDebugValue("surfaceFriction", surfaceFriction);

            ApplyDrivingForces();
        } else {
            Speed = 0;
            slideTimer = 0;
            isSliding = false;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, GetComponent<Rigidbody>().velocity);
        if (isSliding) {
            Gizmos.color = Color.magenta;
        } else {
            Gizmos.color = Color.green;
        }
        Gizmos.DrawRay(transform.position, wheelForwardDirection * 5f);
    }
}