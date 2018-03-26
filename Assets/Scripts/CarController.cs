using System;
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

    public AnimationCurve accelerationSpeed;

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
        get { return wheels.Any(wheel => wheel.Grounded); }
    }

    private bool isReversing = false;

    private bool isStopped = true;

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
        var surfaceVelocity = transform.InverseTransformDirection(rb.velocity);
        surfaceVelocity.y = 0;
        Speed = surfaceVelocity.z;
        debugger.ShowDebugValue("speed", Speed);
        isStopped = Math.Abs(Speed) < stoppedSpeed;
        WheelOrientation = input.Turning * maxWheelTurn;

        if (input.Accelerator > 0) {
            isReversing = false;
        }

        if ((isReversing || isStopped) && input.Brakes > 0) {
            isReversing = true;
        }
        debugger.ShowDebugValue("isReversing", isReversing);

        var wheelRotation = Quaternion.AngleAxis(WheelOrientation, Vector3.up);
        wheelForwardDirection = (wheelRotation * Vector3.forward).normalized;

        // Drive the car forward in the direction of the wheels
        VelocityAlignmentDifference = Vector3.Angle(wheelForwardDirection, surfaceVelocity);
        if (VelocityAlignmentDifference > 90) {
            VelocityAlignmentDifference = 180 - VelocityAlignmentDifference;
        }

        float acceleration;
        if (!isReversing) {
            acceleration = accelerationSpeed.Evaluate((maxSpeed - Speed) / maxSpeed) * input.Accelerator;
        } else {
            acceleration = -accelerationSpeed.Evaluate((maxReverseSpeed - Speed) / maxReverseSpeed) * input.Brakes;
        }
        // Calculate the grip so it increases rapidly as we get towards fully forward-facing
        var forwardGrip = gripPower.Evaluate(VelocityAlignmentDifference / 90) * surfaceFriction;
        var forwardDrivingForce = wheelForwardDirection * forwardGrip * acceleration * enginePower * Time.deltaTime;
        rb.AddRelativeForce(forwardDrivingForce, ForceMode.VelocityChange);

        if (!isReversing && input.IsHandbraking && Speed > minSlideSpeed) {
            isSliding = true;
            slideTimer = straightenUpTime;
        } else if (VelocityAlignmentDifference < straightenUpAngle) {
            if (slideTimer > 0) {
                slideTimer -= Time.deltaTime;
            } else {
                isSliding = false;
            }
        }

        // Transfer velocity as we turn
        var velocityTransferAmount = Speed * turnVelocityTransferRate * (WheelOrientation / maxWheelTurn) * forwardGrip;
        rb.AddRelativeForce(Vector3.right * velocityTransferAmount * Time.deltaTime, ForceMode.VelocityChange);
        rb.AddRelativeForce(-Vector3.forward * Math.Abs(velocityTransferAmount) * Time.deltaTime, ForceMode.VelocityChange);

        // Turn the car up to a maximum angle against the direction of movement
        var allowedTurnAngle = isSliding ? driftMaxTurnAngle : maxTurnAngle;
        var turnSpeed = isSliding ? driftTurnSpeed : this.turnSpeed;
        rb.AddRelativeTorque(Vector3.Cross(Vector3.forward, wheelForwardDirection) * Mathf.Clamp01((allowedTurnAngle - VelocityAlignmentDifference) / allowedTurnAngle) * turnSpeed * Time.deltaTime, ForceMode.VelocityChange);

        var sideFriction = sidewaysFriction.Evaluate(VelocityAlignmentDifference / 90);
        var sidewaysVelocity = Vector3.Project(surfaceVelocity, Vector3.right);
        rb.AddRelativeForce(-sidewaysVelocity * sideFriction * surfaceFriction * Time.deltaTime, ForceMode.VelocityChange);

        if (!isReversing) {
            float baseBraking = 0;
            if (isSliding) {
                baseBraking = driftBrakePower;
            } else {
                baseBraking = input.Brakes;
            }

            var brakeForce = baseBraking * brakingForceMultiplier * forwardGrip;
            debugger.ShowDebugValue("brakeForce", brakeForce);
            rb.AddForce(-surfaceVelocity.normalized * brakeForce * Time.deltaTime, ForceMode.VelocityChange);
        }
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
        Gizmos.DrawRay(transform.position, transform.TransformDirection(wheelForwardDirection) * 5f);
    }
}