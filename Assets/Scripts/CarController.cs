using System;
using System.Linq;
using UnityEngine;

public class CarController : MonoBehaviour {

    public float turnVelocityTransferRate = 1f;

    public float driftTurnVelocityTransferRate = 2f;

    public float turnSpeed = 1f;

    public float driftTurnSpeed = 2f;

    public float maxSpeed = 60f;

    public float minGrip = 0.2f;

    public float maxGrip = 1f;

    public float maxWheelTurn = 10f;

    public float driftMaxTurnAngle = 80f;

    public float maxTurnAngle = 30f;

    public float accelerationFactor = 10f;

    public float reverseAcceleration = 3f;

    public float enginePower = 10f;

    public float maxEngineSpeed = 3f;

    public float brakingPower = 10f;

    public float sidewaysFriction = 2f;

    public float forwardFriction = 1f;

    public float suspensionSpringLength = 0.7f;

    public float suspensionDamping = 0f;

    public float suspensionSpring = 0f;

    public int playerNumber = 1;

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

    private Vector3 originalCentreOfMass;

    private CarPlayerInput input;

    private Vector3 wheelForwardDirection;

    private CarDebugger debugger;

    private Rigidbody rb;

    private Vector3 prevVelocity = Vector3.zero;

    void Start() {
        EngineSpeed = 0;
        Speed = 0;
        WheelOrientation = 0;

        debugger = GetComponent<CarDebugger>();
        input = GetComponent<CarPlayerInput>();
        rb = GetComponent<Rigidbody>();
        originalCentreOfMass = transform.Find("CentreOfMass").localPosition;
    }

    private CarWheel[] GetWheels() {
        return GetComponentsInChildren<CarWheel>();
    }

    private bool IsGrounded() {
        return GetWheels().Any(wheel => wheel.grounded);
    }

    public void Reset() {
        rb.centerOfMass = originalCentreOfMass;
        EngineSpeed = 0;
        Speed = 0;
        WheelOrientation = 0;
        rb.ResetInertiaTensor();
    }

    private void UpdateCentreOfMass() {
        var forwardAcceleration = Mathf.Clamp(Vector3.Project(rb.velocity, transform.forward).magnitude - Vector3.Project(prevVelocity, transform.forward).magnitude, -0.4f, 0.4f);
        var sidewaysAcceleration = Mathf.Clamp(Vector3.Project(rb.velocity, transform.right).magnitude - Vector3.Project(prevVelocity, transform.right).magnitude, -0.4f, 0.4f);
        rb.centerOfMass = Vector3.Lerp(rb.centerOfMass, originalCentreOfMass - (Vector3.forward * forwardAcceleration * 2f), Time.deltaTime);
        rb.centerOfMass = Vector3.Lerp(rb.centerOfMass, originalCentreOfMass + (Vector3.up * forwardAcceleration * 1.5f), Time.deltaTime);
        rb.centerOfMass = Vector3.Lerp(rb.centerOfMass, originalCentreOfMass - (Vector3.right * sidewaysAcceleration * 2.5f), Time.deltaTime);
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
        // a = y - 1 / 8100
        // a = (minGrip - 1) / 8100
        // a = minGrip / 32400
        // Calculate the grip so it increases rapidly as we get towards fully forward-facing
        var forwardDrivingGrip = (((maxGrip - minGrip) / 8100) * (absWheelAlignmentDifference - 90) * (absWheelAlignmentDifference - 90)) + minGrip;
        var forwardMovementForce = EngineSpeed * enginePower * forwardFriction * forwardDrivingGrip;
        // Only accelerate up to the maximum speed
        if (Speed + forwardMovementForce > maxSpeed) {
            forwardMovementForce = Math.Max(maxSpeed - Speed, 0f);
        }
        var forwardDrivingForce = wheelForwardDirection * forwardMovementForce * Time.deltaTime;
        debugger.ShowDebugValue("forwardDrivingGrip", forwardDrivingGrip);
        rb.AddForce(forwardDrivingForce, ForceMode.VelocityChange);

        // Turn the car up to a maximum angle against the direction of movement
        var allowedTurnAngle = input.IsHandbraking ? driftMaxTurnAngle : maxTurnAngle;
        var turnSpeed = input.IsHandbraking ? driftTurnSpeed : this.turnSpeed;
        if (Mathf.Abs(motionWheelAlignmentDifference) < allowedTurnAngle) {
            // rb.AddRelativeTorque(Vector3.up * WheelOrientation * turnSpeed * Time.deltaTime, ForceMode.VelocityChange);
            rb.AddTorque(Vector3.Cross(transform.forward, wheelForwardDirection) * turnSpeed * Time.deltaTime, ForceMode.VelocityChange);
        }

        // Transfer velocity from the direction of movement to the direction of the wheels at an increasing rate depending on
        // how far we're turned away from the direction of movement
        var turnTransferRate = input.IsHandbraking ? driftTurnVelocityTransferRate : turnVelocityTransferRate;
        var velocityTransferAmount = Speed * turnTransferRate * forwardDrivingGrip;
        debugger.ShowDebugValue("velocityTransferAmount", velocityTransferAmount);
        rb.AddForce(wheelForwardDirection * velocityTransferAmount * Time.deltaTime, ForceMode.VelocityChange);
        rb.AddForce(-rb.velocity.normalized * velocityTransferAmount * Time.deltaTime, ForceMode.VelocityChange);

        var sidewaysSpeed = Vector3.Project(rb.velocity, wheelRight);
        rb.AddForce(-sidewaysSpeed * sidewaysFriction * Time.deltaTime, ForceMode.VelocityChange);

        var brakeForce = braking * brakingPower * forwardDrivingGrip;
        debugger.ShowDebugValue("brakeForce", brakeForce);
        rb.AddForce(-rb.velocity.normalized * brakeForce * Time.deltaTime, ForceMode.VelocityChange);
    }

    void Update() {
        foreach (var wheel in GetWheels()) {
            wheel.springFactor = suspensionSpring;
            wheel.dampingFactor = suspensionDamping;
            wheel.targetLength = suspensionSpringLength;
        }

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
        if (IsGrounded()) {
            ApplyDrivingForces();
            UpdateCentreOfMass();
            prevVelocity = rb.velocity;
        } else {
            Speed = 0;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, GetComponent<Rigidbody>().velocity);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, wheelForwardDirection * 5f);
        Gizmos.color = Color.magenta;
        Gizmos.DrawSphere(transform.TransformPoint(rb.centerOfMass), 0.1f);
    }
}