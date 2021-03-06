﻿using System;
using System.Linq;
using UniRx;
using UnityEngine;

public class CarController : MonoBehaviour {

    public bool drivingEnabled = false;

    public float stoppedSpeed = 0.5f;

    public float maxSpeed = 60f;

    public float maxReverseSpeed = -30f;

    public AnimationCurve accelerationSpeed;

    public float accelerationMultiplier = 10f;

    public float brakingForceMultiplier = 10f;

    public float sidewaysFrictionMultiplier = 3f;

    public AnimationCurve turning;

    public float turnMultiplier = 15f;

    public float turnToVelocitySpeed = 0.1f;

    public float turnVelocityTransferRate = 1f;

    public float maxWheelTurn = 10f;

    public float maxTurnAngle = 30f;

    public float driftTurnMultiplier = 33f;

    public float driftMaxTurnAngle = 80f;

    public float minDriftSpeed = 40f;

    public float straightenUpTime = 3f;

    public float straightenUpAngle = 15f;

    public float maxVerticalAngle = 80f;

    public float downForce = 10000f;

    public float turnFrictionMultiplier = 10f;

    public Transform downForcePoint;

    public Transform centreOfMass;

    public float VelocityAlignmentDifference {
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
        get;
        private set;
    }

    public bool IsOnTrack {
        get;
        private set;
    }

    public bool IsDrifting {
        get;
        private set;
    }

    public CarWheel[] wheels {
        get;
        private set;
    }

    private bool isReversing = false;

    private bool isStopped = true;

    private CarPlayerInput input;

    private Vector3 wheelForwardDirection;

    private Rigidbody rb;

    private float driftTimer;

    private float surfaceFriction = 1f;

    private DebugVectorTracker vectorTracker;

    private DebugValueTracker valueTracker;

    private PlayerState playerState;

    void Start() {
        playerState = GetComponent<Car>().playerState;
        valueTracker = DebugValueTracker.Instance;

        if (playerState == null) {
            drivingEnabled = true;
        }

        Speed = 0;
        WheelOrientation = 0;

        input = GetComponent<CarPlayerInput>();
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<CarWheel>();
        vectorTracker = GetComponent<DebugVectorTracker>();

        playerState.mode
            .Where(mode => mode == PlayerState.PlayerMode.Racing)
            .Subscribe(_ => {
                drivingEnabled = true;
            })
            .AddTo(this);

        playerState.mode
            .Where(mode => mode == PlayerState.PlayerMode.Finished)
            .Subscribe(_ => {
                drivingEnabled = false;
            })
            .AddTo(this);
    }

    public void Reset() {
        Speed = 0;
        WheelOrientation = 0;
        rb.ResetInertiaTensor();
        rb.velocity = Vector3.zero;
    }

    private void ApplyDrivingForces() {
        var surfaceVelocity = transform.InverseTransformDirection(rb.velocity);
        surfaceVelocity.y = 0;
        Speed = surfaceVelocity.z;
        TrackValue("speed", Speed);
        TrackVector("surfaceVelocity", surfaceVelocity, Color.yellow);
        isStopped = Math.Abs(Speed) < stoppedSpeed;

        if (!drivingEnabled) {
            rb.AddRelativeForce(-surfaceVelocity, ForceMode.Acceleration);
        }

        RaycastHit roadSurface;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out roadSurface, 1f)) {
            surfaceFriction = roadSurface.collider.material.dynamicFriction;
        }
        TrackValue("surfaceFriction", surfaceFriction);

        if (input.Accelerator > 0) {
            isReversing = false;
        }

        if ((isReversing || isStopped) && input.Brakes > 0) {
            isReversing = true;
        }
        TrackValue("isReversing", isReversing);

        var worldWheelForward = transform.TransformDirection(wheelForwardDirection) * (isReversing ? -1 : 1);
        var worldWheelRight = Vector3.Cross(transform.up, worldWheelForward);
        float rightVerticality = (1 - Mathf.Clamp01(Math.Abs(90f - Vector3.Angle(worldWheelRight, Vector3.up)) / 90f));
        float forwardVerticality = (Vector3.Angle(worldWheelForward, Vector3.up) / 90f);
        float travelVerticality = forwardVerticality * rightVerticality;
        TrackValue("travelVerticality", travelVerticality);

        VelocityAlignmentDifference = -Vector3.SignedAngle(wheelForwardDirection, surfaceVelocity, Vector3.up);
        if (Math.Abs(VelocityAlignmentDifference) > 90) {
            VelocityAlignmentDifference = (Mathf.Sign(VelocityAlignmentDifference) * 180) - VelocityAlignmentDifference;
        }
        var absVelocityAlignmentDifference = Mathf.Abs(VelocityAlignmentDifference);
        TrackValue("VelocityAlignmentDifference", VelocityAlignmentDifference);

        float acceleration;
        var relativeSpeed = Math.Abs(Speed) / maxSpeed;
        if (!isReversing) {
            acceleration = accelerationSpeed.Evaluate(Speed / maxSpeed) * input.Accelerator;
        } else {
            acceleration = -accelerationSpeed.Evaluate(Speed / maxReverseSpeed) * input.Brakes;
        }
        TrackValue("acceleration", acceleration);
        TrackValue("relativeSpeed", relativeSpeed);

        if (!isReversing && input.IsHandbraking && Speed > minDriftSpeed) {
            IsDrifting = true;
            driftTimer = straightenUpTime;
        } else if (IsDrifting) {
            if ((absVelocityAlignmentDifference < straightenUpAngle || Speed < minDriftSpeed) && driftTimer > 0) {
                driftTimer -= Time.deltaTime;
            } else if ((absVelocityAlignmentDifference > straightenUpAngle && Speed > minDriftSpeed) && driftTimer < straightenUpTime) {
                driftTimer += Time.deltaTime;
            } else if (driftTimer <= 0) {
                IsDrifting = false;
            }
        }
        TrackValue("driftTimer", driftTimer, false);
        TrackValue("isDrifting", IsDrifting);

        if (drivingEnabled) {
            var forwardDrivingForce = wheelForwardDirection * acceleration * accelerationMultiplier * surfaceFriction * travelVerticality;
            rb.AddRelativeForce(forwardDrivingForce * Time.deltaTime, ForceMode.VelocityChange);
            TrackVector("forwardDrivingForce", forwardDrivingForce, IsDrifting ? Color.magenta : Color.green);

            Vector3 brakeForce = Vector3.zero;
            if (!isReversing) {
                float baseBrakeForce = input.Brakes;
                if (IsDrifting) {
                    baseBrakeForce = 1.0f;
                }
                brakeForce = Vector3.back * baseBrakeForce * brakingForceMultiplier * surfaceFriction * travelVerticality * Mathf.Clamp01((45f - absVelocityAlignmentDifference) / 45f);
            }
            TrackVector("brakeForce", brakeForce, Color.red);
            rb.AddRelativeForce(brakeForce * Time.deltaTime, ForceMode.VelocityChange);
        }

        EngineSpeed = Mathf.Clamp01((Math.Abs(acceleration) * Mathf.Clamp01(0.2f / relativeSpeed) * 0.8f) + relativeSpeed);
        TrackValue("EngineSpeed", EngineSpeed);

        var turnSpeed = turning.Evaluate(relativeSpeed);
        TrackValue("turnSpeed", turnSpeed);

        // Turn the car towards the direction it's travelling
        rb.AddRelativeTorque(Vector3.Cross(Vector3.forward, surfaceVelocity) * turnToVelocitySpeed * surfaceFriction * Time.deltaTime, ForceMode.VelocityChange);

        // Turn the car up to a maximum angle against the direction of movement
        var allowedTurnAngle = IsDrifting ? driftMaxTurnAngle : maxTurnAngle;
        var turnRate = Mathf.Clamp((allowedTurnAngle - absVelocityAlignmentDifference) / allowedTurnAngle, 0.5f, 1f);
        var turnAmount = WheelOrientation / maxWheelTurn;
        rb.AddRelativeTorque(Vector3.up * turnAmount * turnSpeed * turnRate * (IsDrifting ? driftTurnMultiplier : turnMultiplier) * Time.deltaTime, ForceMode.VelocityChange);

        // Add increasing friction against velocity direction the more the wheels are angled against the direction of movement
        var turnFriction = -surfaceVelocity.normalized * (Speed / maxSpeed) * Mathf.Clamp01(absVelocityAlignmentDifference / 45f) * surfaceFriction * turnFrictionMultiplier * Time.deltaTime;
        rb.AddForce(turnFriction, ForceMode.VelocityChange);
        TrackVector("turnFriction", turnFriction, Color.green);

        // Turn the car's velocity
        var turnForce = Vector3.right * input.Steering * (Speed / maxSpeed) * turnSpeed * turnFrictionMultiplier;
        rb.AddRelativeForce(turnForce * Time.deltaTime, ForceMode.VelocityChange);
        TrackVector("turnForce", turnForce, Color.magenta);

        // Stop the car from sliding sideways
        var sidewaysVelocity = Vector3.Project(surfaceVelocity, Vector3.right);
        var sideFriction = sidewaysVelocity.magnitude * surfaceFriction * sidewaysFrictionMultiplier;
        var sidewaysFrictionForce = -sidewaysVelocity.normalized * sideFriction * rightVerticality * surfaceFriction;
        TrackVector("sidewaysVelocity", sidewaysVelocity, Color.magenta);
        TrackVector("sidewaysFrictionForce", sidewaysFrictionForce, Color.red);
        rb.AddRelativeForce(sidewaysFrictionForce * Time.deltaTime, ForceMode.VelocityChange);

        if (centreOfMass) {
            rb.centerOfMass = centreOfMass.localPosition;
        }
    }

    void ApplyDownForce() {
        var localVelocity = transform.InverseTransformDirection(rb.velocity);
        var forwardVelocity = Mathf.Abs(localVelocity.z);

        if (downForcePoint != null) {
            Vector3 downForceVector = Vector3.down * downForce * (forwardVelocity / maxSpeed);
            TrackVector("downForce", downForceVector, Color.cyan);
            rb.AddForceAtPosition(downForceVector, downForcePoint.position);
        }
    }

    void FixedUpdate() {
        // WheelOrientation = Mathf.Lerp(WheelOrientation, input.Steering * maxWheelTurn, Time.deltaTime * 50f);
        WheelOrientation = input.Steering * maxWheelTurn;
        TrackValue("wheelOrientation", WheelOrientation);
        var wheelRotation = Quaternion.AngleAxis(WheelOrientation, Vector3.up);
        wheelForwardDirection = (wheelRotation * Vector3.forward).normalized;
        IsGrounded = wheels.Any(wheel => wheel.IsGrounded);

        ApplyDownForce();

        if (IsGrounded) {
            ApplyDrivingForces();
        } else {
            Speed = 0;
            driftTimer = 0;
            IsDrifting = false;
        }
    }

    void Update() {
        IsOnTrack = Physics.Raycast(transform.position, Vector3.down, Mathf.Infinity, LayerMask.GetMask("Track"));
    }

    void TrackValue(string label, object value) {
        if (valueTracker) {
            valueTracker.Log(label, value);
        }
    }

    void TrackValue(string label, float value, bool showMinMax = true) {
        if (valueTracker) {
            valueTracker.Log(label, value, showMinMax);
        }
    }

    void TrackVector(string label, Vector3 value, Color color) {
        if (vectorTracker) {
            vectorTracker.Log(label, value, color);
        }
    }

    void OnCollisionEnter() {
        IsDrifting = false;
    }
}