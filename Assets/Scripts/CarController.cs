using System;
using System.Linq;
using UnityEngine;

public class CarController : MonoBehaviour {

    public float minTurningRate = 1f;

    public float maxTurningRate = 2f;

    public float turnVelocityTransferPower = 1f;

    public float maxSpeed = 60f;

    public float minOrientationCorrectionRate = 0.1f;

    public float maxOrientationCorrectionRate = 0.2f;

    public float maxTurningAngle = 30f;

    public float accelerationFactor = 10f;

    public float reverseAcceleration = 3f;

    public float enginePower = 10f;

    public float maxEngineSpeed = 3f;

    public float brakingPower = 10f;

    public float frictionFactor = 2f;

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

    private CarPlayerInput input;

    private Vector3 wheelForwardDirection;

    private CarDebugger debugger;

    void Start() {
        EngineSpeed = 0;
        Speed = 0;
        WheelOrientation = 0;

        debugger = GetComponent<CarDebugger>();
        input = GetComponent<CarPlayerInput>();
    }

    private CarWheel[] GetWheels() {
        return GetComponentsInChildren<CarWheel>();
    }

    private bool IsGrounded() {
        return GetWheels().Any(wheel => wheel.grounded);
    }

    private void ApplyDrivingForces() {
        var transform = GetComponent<Transform>();
        var rb = GetComponent<Rigidbody>();

        Speed = Vector3.Project(rb.velocity, transform.forward).magnitude;
        debugger.ShowDebugValue("speed", Speed);
        debugger.ShowDebugValue("speed %", Speed / maxSpeed);
        isReversing = Speed < 0;
        isStopped = Speed == 0;

        var wheelRotation = Quaternion.AngleAxis(WheelOrientation, Vector3.up);
        var wheelSidewaysRotation = Quaternion.AngleAxis(90, Vector3.up);
        wheelForwardDirection = transform.TransformDirection(wheelRotation * Vector3.forward);
        var wheelRight = transform.TransformDirection(wheelRotation * wheelSidewaysRotation * Vector3.forward);
        var motionWheelAlignmentDifference = Vector3.SignedAngle(wheelForwardDirection, rb.velocity, Vector3.up);
        debugger.ShowDebugValue("motionWheelAlignmentDifference", motionWheelAlignmentDifference);

        // Drive the car forward in the direction of the wheels
        var forwardDrivingGrip = Mathf.Max((90f - Math.Abs(motionWheelAlignmentDifference)) / 90f, 0);
        var forwardDrivingForce = wheelForwardDirection * EngineSpeed * enginePower * forwardDrivingGrip * Time.deltaTime;
        debugger.ShowDebugValue("forwardDrivingGrip", forwardDrivingGrip);
        rb.AddForce(forwardDrivingForce, ForceMode.VelocityChange);

        // Turn the car to match the orientation of the wheels depending on forward travel speed
        // FIXME: transform.up instead of Vector3.up?
        var bodyWheelAlignmentDifference = -Vector3.SignedAngle(wheelForwardDirection, transform.forward, Vector3.up);
        var turnMultiplier = Mathf.Lerp(maxTurningRate, minTurningRate, Speed / maxSpeed);
        var alignToWheelsForce = Vector3.up * WheelOrientation * turnMultiplier * Time.deltaTime;
        rb.AddRelativeTorque(alignToWheelsForce, ForceMode.VelocityChange);

        // Transfer velocity from the direction of movement to the direction of the wheels
        if (Math.Abs(bodyWheelAlignmentDifference) > 0) {
            var wheelForwardForce = wheelForwardDirection * turnVelocityTransferPower * (Speed / maxSpeed) * Time.deltaTime;
            rb.AddForce(wheelForwardForce, ForceMode.VelocityChange);
            var velocityDirectionChangeForce = rb.velocity.normalized * -turnVelocityTransferPower * (Speed / maxSpeed) * Time.deltaTime;
            rb.AddForce(velocityDirectionChangeForce, ForceMode.VelocityChange);
        }

        // Turn the car to match the direction of movement, except for at very low speeds where it gets twisty
        if (Speed > 2) {
            var orientationCorrectionRate = Mathf.Lerp(maxOrientationCorrectionRate, minOrientationCorrectionRate, Speed / maxSpeed);
            debugger.ShowDebugValue("orientationCorrectionRate", orientationCorrectionRate);
            rb.AddRelativeTorque(Vector3.up * motionWheelAlignmentDifference * orientationCorrectionRate * Time.deltaTime, ForceMode.VelocityChange);
        }

        // Add resistance to travelling perpendicular to the wheels
        var sidewaysSpeed = Vector3.Project(rb.velocity, wheelRight);
        rb.AddForce(-sidewaysSpeed * frictionFactor * Time.deltaTime, ForceMode.VelocityChange);

        if (braking > 0 && rb.velocity.magnitude > 0) {
            rb.AddForce(-rb.velocity * braking * brakingPower * Time.deltaTime, ForceMode.VelocityChange);
        }
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

        WheelOrientation = Mathf.Lerp(WheelOrientation, maxTurningAngle * input.Turning, Time.deltaTime * 50f);
        debugger.ShowDebugValue("wheelOrientation", WheelOrientation);

        braking = input.Brakes;
    }

    void FixedUpdate() {
        if (IsGrounded()) {
            ApplyDrivingForces();
        } else {
            Speed = 0;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(GetComponent<Transform>().position, GetComponent<Rigidbody>().velocity);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(GetComponent<Transform>().position, wheelForwardDirection * 5f);
    }
}