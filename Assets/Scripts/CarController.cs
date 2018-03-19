using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class CarController : MonoBehaviour {

    public float turnVelocityTransferRate = 1f;

    public float driftTurnVelocityTransferRate = 2f;

    public float turnSpeed = 1f;

    public float driftTurnSpeed = 2f;

    public float maxSpeed = 60f;

    public AnimationCurve gripPower;

    public float maxWheelTurn = 10f;

    public float driftMaxTurnAngle = 80f;

    public float maxTurnAngle = 30f;

    public float accelerationFactor = 10f;

    public float reverseAcceleration = 3f;

    public float enginePower = 10f;

    public float maxEngineSpeed = 3f;

    public float surfaceFriction = 1f;

    public float straightenUpTime = 3f;

    public float straightenUpAngle = 15f;

    public float minSlideSpeed = 40f;

    public int playerNumber = 1;

    public AnimationCurve brakingPower;

    public float brakingForceMultiplier = 10f;

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

    private bool isReversing = false;

    private bool isStopped = true;

    private float braking;

    private CarPlayerInput input;

    private Vector3 wheelForwardDirection;

    private CarDebugger debugger;

    private Rigidbody rb;

    private Vector3 prevVelocity = Vector3.zero;

    private CarWheel[] wheels;

    private bool isSliding;

    private float slideTimer;

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
        Speed = forwardVelocity.magnitude;
        debugger.ShowDebugValue("speed", Speed);
        debugger.ShowDebugValue("speed %", Speed / maxSpeed);
        isReversing = Speed < 0;
        isStopped = Speed == 0;

        var wheelRotation = Quaternion.AngleAxis(WheelOrientation, Vector3.up);
        var wheelSidewaysRotation = Quaternion.AngleAxis(90, Vector3.up);
        wheelForwardDirection = transform.TransformDirection(wheelRotation * Vector3.forward).normalized;
        var wheelRight = transform.TransformDirection(wheelRotation * wheelSidewaysRotation * Vector3.forward);
        var motionWheelAlignmentDifference = Vector3.SignedAngle(wheelForwardDirection, rb.velocity, Vector3.up);
        debugger.ShowDebugValue("motionWheelAlignmentDifference", motionWheelAlignmentDifference);

        // Drive the car forward in the direction of the wheels
        float absWheelAlignmentDifference = Math.Abs(motionWheelAlignmentDifference);
        if (absWheelAlignmentDifference > 90) {
            absWheelAlignmentDifference = 180 - absWheelAlignmentDifference;
        }
        debugger.ShowDebugValue("absWheelAlignmentDifference", absWheelAlignmentDifference);
        // Calculate the grip so it increases rapidly as we get towards fully forward-facing
        var forwardGrip = gripPower.Evaluate(absWheelAlignmentDifference / 90);
        var sidewaysGrip = (1 - forwardGrip);
        var forwardMovementForce = EngineSpeed * forwardGrip * surfaceFriction * enginePower * forwardGrip;
        // Only accelerate up to the maximum speed
        if (Speed + forwardMovementForce > maxSpeed) {
            forwardMovementForce = Math.Max(maxSpeed - Speed, 0f);
        }
        var forwardDrivingForce = wheelForwardDirection * forwardMovementForce * Time.deltaTime;
        debugger.ShowDebugValue("forwardGrip", forwardGrip);
        debugger.ShowDebugValue("sidewaysGrip", sidewaysGrip);
        rb.AddForce(forwardDrivingForce, ForceMode.VelocityChange);

        if (input.IsHandbraking && Speed > minSlideSpeed) {
            isSliding = true;
            slideTimer = straightenUpTime;
        } else if (absWheelAlignmentDifference < straightenUpAngle) {
            if (slideTimer > 0) {
                slideTimer -= Time.deltaTime;
            } else {
                isSliding = false;
            }
        }

        debugger.ShowDebugValue("isSliding", isSliding);
        debugger.ShowDebugValue("slideTimer", slideTimer, false);

        // Turn the car up to a maximum angle against the direction of movement
        var allowedTurnAngle = isSliding ? driftMaxTurnAngle : maxTurnAngle;
        var turnSpeed = isSliding ? driftTurnSpeed : this.turnSpeed;
        if (Mathf.Abs(motionWheelAlignmentDifference) < allowedTurnAngle) {
            rb.AddTorque(Vector3.Cross(transform.forward, wheelForwardDirection) * turnSpeed * Time.deltaTime, ForceMode.VelocityChange);
        }

        // Transfer velocity from the direction of movement to the direction of the wheels at an increasing rate depending on
        // how far we're turned away from the direction of movement
        var turnTransferRate = isSliding ? driftTurnVelocityTransferRate : turnVelocityTransferRate;
        var velocityTransferAmount = Speed * turnTransferRate * forwardGrip;
        debugger.ShowDebugValue("velocityTransferAmount", velocityTransferAmount);
        rb.AddForce(wheelForwardDirection * velocityTransferAmount * Time.deltaTime, ForceMode.VelocityChange);
        rb.AddForce(-rb.velocity.normalized * velocityTransferAmount * Time.deltaTime, ForceMode.VelocityChange);

        var sidewaysSpeed = Vector3.Project(rb.velocity, wheelRight);
        rb.AddForce(-sidewaysSpeed * sidewaysGrip * surfaceFriction * Time.deltaTime, ForceMode.VelocityChange);

        var brakeForce = braking * brakingPower.Evaluate(Speed / maxSpeed) * brakingForceMultiplier * forwardGrip * surfaceFriction;
        debugger.ShowDebugValue("brakeForce", brakeForce);
        rb.AddForce(-rb.velocity.normalized * brakeForce * Time.deltaTime, ForceMode.VelocityChange);
    }

    void Update() {
        EngineSpeed = Mathf.Lerp(EngineSpeed, 0, Time.deltaTime);
        if (input.Accelerator > 0) {
            EngineSpeed += input.Accelerator * accelerationFactor * Time.deltaTime;
        } else {
            EngineSpeed += input.Accelerator * reverseAcceleration * Time.deltaTime;
        }
        EngineSpeed = Mathf.Min(EngineSpeed, maxEngineSpeed);
        debugger.ShowDebugValue("engineSpeed", EngineSpeed);

        WheelOrientation = Mathf.Lerp(WheelOrientation, maxWheelTurn * input.Turning, Time.deltaTime * 50f);
        debugger.ShowDebugValue("wheelOrientation", WheelOrientation);

        braking = input.Brakes;
    }

    void FixedUpdate() {
        var isGrounded = wheels.Any(wheel => wheel.grounded);
        if (isGrounded) {
            ApplyDrivingForces();
            prevVelocity = rb.velocity;
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