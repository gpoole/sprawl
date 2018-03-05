using UnityEngine;
using System;
using System.Linq;

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

    public float wheelOrientation {
        get;
        private set;
    }

    private bool isReversing = false;

    private bool isStopped = true;

    public float engineSpeed {
        get;
        private set;
    }

    public float speed {
        get;
        private set;
    }

    private Vector3 wheelForwardDirection;

    private float[] joystickRange = { 0f, 1f };

    void Start() {
        engineSpeed = 0;
        speed = 0;
        wheelOrientation = 0;

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

        speed = Vector3.Project(colliderRb.velocity, transform.forward).magnitude;
        isReversing = speed < 0;
        isStopped = speed == 0;

        var wheelRotation = Quaternion.AngleAxis(wheelOrientation, Vector3.up);
        var wheelSidewaysRotation = Quaternion.AngleAxis(90, Vector3.up);
        wheelForwardDirection = transform.TransformDirection(wheelRotation * Vector3.forward);
        var wheelRight = transform.TransformDirection(wheelRotation * wheelSidewaysRotation * Vector3.forward);

        // Drive the car forward in the direction of the wheels
        var forwardDrivingForce = wheelForwardDirection * engineSpeed * enginePower;
        colliderRb.AddForce(forwardDrivingForce * Time.deltaTime, ForceMode.Impulse);

        // Turn the car to match the orientation of the wheels depending on forward travel speed
        // FIXME: transform.up instead of Vector3.up?
        var bodyWheelAlignmentDifference = -Vector3.SignedAngle(wheelForwardDirection, transform.forward, Vector3.up);
        var turnMultiplier = Mathf.Lerp(maxTurningRate, minTurningRate, speed / maxSpeed);
        var alignToWheelsForce = Vector3.up * bodyWheelAlignmentDifference * turnMultiplier;
        colliderRb.AddRelativeTorque(alignToWheelsForce * Time.deltaTime, ForceMode.Impulse);

        // Transfer velocity from the direction of movement to the direction of the wheels
        if (Math.Abs(bodyWheelAlignmentDifference) > 0) {
            var wheelForwardForce = wheelForwardDirection * turnVelocityTransferPower * (speed / maxSpeed);
            colliderRb.AddForce(wheelForwardForce);
            colliderRb.AddForce(colliderRb.velocity.normalized * -turnVelocityTransferPower * (speed / maxSpeed));
        }

        // Turn the car to match the direction of movement
        var motionWheelAlignmentDifference = Vector3.SignedAngle(wheelForwardDirection, colliderRb.velocity, Vector3.up);
        var orientationCorrectionRate = Mathf.Lerp(maxOrientationCorrectionRate, minOrientationCorrectionRate, speed / maxSpeed);
        colliderRb.AddRelativeTorque(Vector3.up * motionWheelAlignmentDifference * orientationCorrectionRate * Time.deltaTime, ForceMode.Impulse);

        // Add resistance to travelling perpendicular to the wheels
        var sidewaysSpeed = Vector3.Project(colliderRb.velocity, wheelRight);
        colliderRb.AddForce(-sidewaysSpeed * frictionFactor * Time.deltaTime, ForceMode.Impulse);

        var brakingAmount = GetBraking();
        if (brakingAmount > 0 && colliderRb.velocity.magnitude > 0) {
            colliderRb.AddForce(-colliderRb.velocity * brakingAmount * brakingPower * Time.deltaTime, ForceMode.Impulse);
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
        wheelOrientation = Mathf.Lerp(wheelOrientation, maxTurningAngle * turning, Time.deltaTime * 50f);
    }


    void FixedUpdate() {
        if (IsGrounded()) {
            ApplyDrivingForces();
        } else {
            speed = 0;
        }
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawRay(GetComponent<Transform>().position, GetComponent<Rigidbody>().velocity);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(GetComponent<Transform>().position, wheelForwardDirection * 5f);
    }
}
