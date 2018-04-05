using System;
using System.Linq;
using UnityEngine;

public class CarController : MonoBehaviour {

    public int playerId;

    public float stoppedSpeed = 0.5f;

    public float maxSpeed = 60f;

    public float maxReverseSpeed = -30f;

    public AnimationCurve accelerationSpeed;

    public float accelerationMultiplier = 10f;

    public float brakingForceMultiplier = 10f;

    public AnimationCurve gripPower;

    public AnimationCurve sidewaysFriction;

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
        get { return wheels.Any(wheel => wheel.IsGrounded); }
    }

    public bool IsDrifting {
        get;
        private set;
    }

    private bool isReversing = false;

    private bool isStopped = true;

    private CarPlayerInput input;

    private Vector3 wheelForwardDirection;

    private CarDebugger debugger;

    private Rigidbody rb;

    private CarWheel[] wheels;

    private float driftTimer;

    private float surfaceFriction = 1f;

    void Start() {
        Speed = 0;
        WheelOrientation = 0;

        debugger = GetComponent<CarDebugger>();
        input = GetComponent<CarPlayerInput>();
        rb = GetComponent<Rigidbody>();
        wheels = GetComponentsInChildren<CarWheel>();
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
        Debug("speed", Speed);
        Debug("surfaceVelocity", surfaceVelocity, Color.yellow);
        isStopped = Math.Abs(Speed) < stoppedSpeed;
        WheelOrientation = input.Turning * maxWheelTurn;

        RaycastHit roadSurface;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out roadSurface, 1f)) {
            surfaceFriction = roadSurface.collider.material.dynamicFriction;
        }
        Debug("surfaceFriction", surfaceFriction);

        if (input.Accelerator > 0) {
            isReversing = false;
        }

        if ((isReversing || isStopped) && input.Brakes > 0) {
            isReversing = true;
        }
        Debug("isReversing", isReversing);

        var wheelRotation = Quaternion.AngleAxis(WheelOrientation, Vector3.up);
        wheelForwardDirection = (wheelRotation * Vector3.forward).normalized;

        var worldWheelForward = transform.TransformDirection(wheelForwardDirection) * (isReversing ? -1 : 1);
        var worldWheelRight = Vector3.Cross(transform.up, worldWheelForward);
        float rightVerticality = (1 - Mathf.Clamp01(Math.Abs(90f - Vector3.Angle(worldWheelRight, Vector3.up)) / 90f));
        float travelVerticality = (Vector3.Angle(worldWheelForward, Vector3.up) / 90f) * rightVerticality;
        Debug("travelVerticality", travelVerticality);

        // Drive the car forward in the direction of the wheels
        VelocityAlignmentDifference = -Vector3.SignedAngle(wheelForwardDirection, surfaceVelocity, Vector3.up);
        if (Math.Abs(VelocityAlignmentDifference) > 90) {
            VelocityAlignmentDifference = (Mathf.Sign(VelocityAlignmentDifference) * 180) - VelocityAlignmentDifference;
        }
        var absVelocityAlignmentDifference = Mathf.Abs(VelocityAlignmentDifference);

        float acceleration;
        var relativeSpeed = Math.Abs(Speed) / maxSpeed;
        if (!isReversing) {
            acceleration = accelerationSpeed.Evaluate(Speed / maxSpeed) * input.Accelerator;
        } else {
            acceleration = -accelerationSpeed.Evaluate(Speed / maxReverseSpeed) * input.Brakes;
        }
        Debug("acceleration", acceleration);
        // Calculate the grip so it increases rapidly as we get towards fully forward-facing
        Debug("relativeSpeed", relativeSpeed);
        var forwardDrivingForce = wheelForwardDirection * acceleration * accelerationMultiplier * gripPower.Evaluate(((90 - absVelocityAlignmentDifference) / 90) / surfaceFriction) * travelVerticality;
        rb.AddRelativeForce(forwardDrivingForce * Time.deltaTime, ForceMode.VelocityChange);
        Debug("forwardDrivingForce", forwardDrivingForce, IsDrifting ? Color.magenta : Color.green);

        EngineSpeed = Mathf.Clamp01((Math.Abs(acceleration) * Mathf.Clamp01(0.2f / relativeSpeed) * 0.8f) + relativeSpeed);
        Debug("EngineSpeed", EngineSpeed);

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

        Debug("driftTimer", driftTimer, false);
        Debug("isDrifting", IsDrifting);

        Debug("VelocityAlignmentDifference", VelocityAlignmentDifference);

        var turnSpeed = turning.Evaluate(relativeSpeed);
        Debug("turnSpeed", turnSpeed);

        // Transfer velocity as we turn
        var velocityTransferAmount = Speed * Mathf.Clamp(VelocityAlignmentDifference / 90f, -1, 1) * turnVelocityTransferRate * turnSpeed * surfaceFriction * travelVerticality;
        var turnForceRight = (isReversing ? Vector3.left : Vector3.right) * velocityTransferAmount;
        Debug("turnForceRight", turnForceRight, Color.blue);
        rb.AddRelativeForce(turnForceRight * Time.deltaTime, ForceMode.VelocityChange);

        // Turn the car towards the direction it's travelling
        rb.AddRelativeTorque(Vector3.Cross(Vector3.forward, surfaceVelocity) * turnToVelocitySpeed * surfaceFriction * travelVerticality * Time.deltaTime, ForceMode.VelocityChange);

        // Turn the car up to a maximum angle against the direction of movement
        var allowedTurnAngle = IsDrifting ? driftMaxTurnAngle : maxTurnAngle;
        rb.AddRelativeTorque(Vector3.up * input.Turning * Mathf.Clamp((allowedTurnAngle - absVelocityAlignmentDifference) / allowedTurnAngle, 0.2f, 1f) * turnSpeed * (IsDrifting ? driftTurnMultiplier : turnMultiplier) * travelVerticality * Time.deltaTime, ForceMode.VelocityChange);

        var sidewaysVelocity = Vector3.Project(surfaceVelocity, Vector3.right);
        var sideFriction = sidewaysFriction.Evaluate(sidewaysVelocity.magnitude * surfaceFriction) * sidewaysFrictionMultiplier;
        var sidewaysFrictionForce = -sidewaysVelocity.normalized * sideFriction * travelVerticality * surfaceFriction;
        Debug("sidewaysVelocity", sidewaysVelocity, Color.magenta);
        Debug("sidewaysFrictionForce", sidewaysFrictionForce, Color.red);
        rb.AddRelativeForce(sidewaysFrictionForce * Time.deltaTime, ForceMode.VelocityChange);

        Vector3 brakeForce = Vector3.zero;
        if (!isReversing) {
            float baseBrakeForce = input.Brakes;
            if (IsDrifting) {
                baseBrakeForce = 1.0f;
            }
            brakeForce = Vector3.back * baseBrakeForce * brakingForceMultiplier * surfaceFriction * travelVerticality * Mathf.Clamp01((45f - absVelocityAlignmentDifference) / 45f);
        }
        Debug("brakeForce", brakeForce, Color.red);
        rb.AddRelativeForce(brakeForce * Time.deltaTime, ForceMode.VelocityChange);
    }

    void FixedUpdate() {
        if (IsGrounded) {
            ApplyDrivingForces();
        } else {
            Speed = 0;
            driftTimer = 0;
            IsDrifting = false;
        }

        if (input.IsResetting) {
            RaceManager.Instance.ResetPlayer(playerId);
        }
    }

    void Debug(string label, object value) {
        if (debugger) {
            debugger.Log(label, value);
        }
    }

    void Debug(string label, float value, bool showMinMax = true) {
        if (debugger) {
            debugger.Log(label, value, showMinMax);
        }
    }

    void Debug(string label, Vector3 vector, Color color) {
        if (debugger) {
            debugger.Log(label, vector, color);
        }
    }

    void OnCollisionEnter() {
        IsDrifting = false;
    }
}