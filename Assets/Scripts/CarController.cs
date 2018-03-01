using UnityEngine;
using System;
using System.Linq;

public class CarController : MonoBehaviour {

    public float turningRate = 10f;

    public float orientationCorrectionRate = 0.006f;

    public float maxTurningAngle = 30f;

    public float accelerationFactor = 10f;

    public float reverseAcceleration = 3f;

    public float enginePower = 10f;

    public float maxEngineSpeed = 3f;

    public float brakingFactor = 10f;

    public float frictionFactor = 2f;

    public float driftFactor = 1f;

    public float handbrakeDrift = 1f;

    public float handbrakeBrakePower = 1f;

    public float suspensionSpringLength = 0.7f;

    public float suspensionDamping = 0f;

    public float suspensionSpring = 0f;

    public int playerNumber = 1;

    private float wheelOrientation = 0f;

    private bool isReversing = false;

    private bool isStopped = true;

    private float engineSpeed = 0f;

    private Vector3 wheelRight;

    private float[] joystickRange = { 0f, 1f };

    void Start() {
        // FIXME: compensate for the XBox's triggers ranging from -1 to 1 instead of 0 to 1,
        // need to move this to somewhere better and make it more generic
        var joysticks = Input.GetJoystickNames();
        if (joysticks.Length >= playerNumber && joysticks[playerNumber - 1].Contains("Xbox 360 Wired Controller")) {
            joystickRange = new[] { -1f, 1f };
        }
    }

    private float GetTurning() {
        return Input.GetAxis("P" + playerNumber + " Steering");
    }

    private float GetRangedInput(String name) {
        var range = joystickRange[1] - joystickRange[0];
        return (Input.GetAxis(name) - joystickRange[0]) / range;
    }

    private float GetAccelerator() {
        var acceleratorAmount = GetRangedInput("P" + playerNumber + " Accelerator");
        var braking = GetRangedInput("P" + playerNumber + " Braking");
        if (braking > 0 && (isReversing || isStopped)) {
            acceleratorAmount -= braking;
        }
        return acceleratorAmount;
    }

    private float GetBraking() {
        if (isReversing) {
            return 0;
        }
        return GetRangedInput("P" + playerNumber + " Braking");
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

        // var travellingDirection = transform.InverseTransformDirection(new Vector3(colliderRb.velocity.normalized.x, 0, colliderRb.velocity.normalized.z));
        var forwardSpeed = Vector3.Project(colliderRb.velocity, transform.forward).magnitude;
        // Debug.Log("forwardSpeed=" + forwardSpeed);
        isReversing = forwardSpeed < 0;
        isStopped = forwardSpeed == 0;

        var wheelRotation = Quaternion.AngleAxis(wheelOrientation, Vector3.up);
        var wheelSidewaysRotation = Quaternion.AngleAxis(90, Vector3.up);
        var wheelForwardDirection = transform.TransformDirection(wheelRotation * Vector3.forward);
        wheelRight = transform.TransformDirection(wheelRotation * wheelSidewaysRotation * Vector3.forward);

        // Drive the car forward in the direction of the wheels
        var forwardDrivingForce = wheelForwardDirection * engineSpeed * enginePower;
        colliderRb.AddForce(forwardDrivingForce * Time.deltaTime, ForceMode.Impulse);

        // Turn the car to match the orientation of the wheels depending on forward travel speed
        // var forwardTravelSpeed = travellingDirection;
        // FIXME: transform.up instead of Vector3.up?
        var bodyWheelAlignmentDifference = -Vector3.SignedAngle(wheelForwardDirection, transform.forward, Vector3.up);
        var alignToWheelsForce = Vector3.up * turningRate * bodyWheelAlignmentDifference * forwardSpeed;
        colliderRb.AddRelativeTorque(alignToWheelsForce * Time.deltaTime, ForceMode.Impulse);

        // Turn the car to match the direction of movement, unless we're overriding it
        var motionWheelAlignmentDifference = Vector3.SignedAngle(wheelForwardDirection, colliderRb.velocity, Vector3.up);
        if (wheelOrientation == 0) {
            colliderRb.AddRelativeTorque(Vector3.up * motionWheelAlignmentDifference * forwardSpeed * orientationCorrectionRate * Time.deltaTime, ForceMode.Impulse);
        }

        // Add resistance to travelling perpendicular to the wheels
        var sidewaysSpeed = Vector3.Project(colliderRb.velocity, wheelRight);
        colliderRb.AddForce(-sidewaysSpeed * frictionFactor * Time.deltaTime, ForceMode.Impulse);

        var brakingAmount = GetBraking();
        if (brakingAmount > 0 && colliderRb.velocity.magnitude > 0) {
            colliderRb.AddForce(-colliderRb.velocity * brakingAmount * brakingFactor * Time.deltaTime, ForceMode.Impulse);
        }
    }

    void Update() {
        foreach (var wheel in GetWheels()) {
            wheel.springFactor = suspensionSpring;
            wheel.dampingFactor = suspensionDamping;
            wheel.targetLength = suspensionSpringLength;
        }

        var accelerator = GetAccelerator();
        engineSpeed = Mathf.Lerp(engineSpeed, 0, Time.deltaTime);
        if (accelerator > 0) {
            engineSpeed += accelerator * accelerationFactor * Time.deltaTime;
        } else {
            engineSpeed += accelerator * reverseAcceleration * Time.deltaTime;
        }
        engineSpeed = Mathf.Min(engineSpeed, maxEngineSpeed);


        var turning = GetTurning();
        // wheelOrientation = Mathf.Lerp(wheelOrientation, maxTurningAngle * turning, Time.deltaTime * 50f);
        wheelOrientation = maxTurningAngle * turning;
    }


    void FixedUpdate() {
        if (IsGrounded()) {
            ApplyDrivingForces();
        }
    }

    void OnDrawGizmos() {
        var wheelRotation = Quaternion.AngleAxis(wheelOrientation, Vector3.up);
        var wheelForwardDirection = transform.TransformDirection(wheelRotation * Vector3.forward);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, wheelForwardDirection * 20f);
        Gizmos.DrawRay(transform.position, wheelRight * 20f);
    }
}
