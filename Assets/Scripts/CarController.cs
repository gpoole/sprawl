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
        debugger.ShowDebugValue("surfaceVelocity", surfaceVelocity, Color.yellow);
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
        VelocityAlignmentDifference = -Vector3.SignedAngle(wheelForwardDirection, surfaceVelocity, Vector3.up);
        if (Math.Abs(VelocityAlignmentDifference) > 90) {
            VelocityAlignmentDifference = (Mathf.Sign(VelocityAlignmentDifference) * 180) - VelocityAlignmentDifference;
        }
        var absVelocityAlignmentDifference = Mathf.Abs(VelocityAlignmentDifference);

        float acceleration;
        if (!isReversing) {
            acceleration = accelerationSpeed.Evaluate((maxSpeed - Speed) / maxSpeed) * input.Accelerator;
        } else {
            acceleration = -accelerationSpeed.Evaluate((maxReverseSpeed - Speed) / maxReverseSpeed) * input.Brakes;
        }
        // Calculate the grip so it increases rapidly as we get towards fully forward-facing
        debugger.ShowDebugValue("surfaceFriction", surfaceFriction);
        var forwardGrip = gripPower.Evaluate(absVelocityAlignmentDifference / 90) * surfaceFriction;
        debugger.ShowDebugValue("forwardGrip", forwardGrip);
        var forwardDrivingForce = wheelForwardDirection * forwardGrip * acceleration * enginePower;
        rb.AddRelativeForce(forwardDrivingForce * Time.deltaTime, ForceMode.VelocityChange);
        debugger.ShowDebugValue("forwardDrivingForce", forwardDrivingForce, isSliding ? Color.magenta : Color.green);

        if (!isReversing && input.IsHandbraking && Speed > minSlideSpeed) {
            isSliding = true;
            slideTimer = straightenUpTime;
        } else if (isSliding) {
            if (absVelocityAlignmentDifference < straightenUpAngle && slideTimer > 0) {
                slideTimer -= Time.deltaTime;
            } else if (absVelocityAlignmentDifference > straightenUpAngle && slideTimer < straightenUpTime) {
                slideTimer += Time.deltaTime;
            } else if (slideTimer <= 0) {
                isSliding = false;
            }
        }

        debugger.ShowDebugValue("slideTimer", slideTimer, false);
        debugger.ShowDebugValue("isSliding", isSliding);

        debugger.ShowDebugValue("VelocityAlignmentDifference", VelocityAlignmentDifference);

        // Transfer velocity as we turn
        var velocityTransferAmount = Speed * turnVelocityTransferRate * Mathf.Clamp(VelocityAlignmentDifference / 45, -1, 1) * forwardGrip;
        var turnForceRight = Vector3.right * velocityTransferAmount;
        debugger.ShowDebugValue("turnForceRight", turnForceRight, Color.blue, 0.5f);
        rb.AddRelativeForce(turnForceRight * Time.deltaTime, ForceMode.VelocityChange);
        var turnForceForward = -Vector3.forward * Math.Abs(velocityTransferAmount);
        debugger.ShowDebugValue("turnForceForward", turnForceForward, Color.red, 0.5f);
        rb.AddRelativeForce(turnForceForward * Time.deltaTime, ForceMode.VelocityChange);

        // Turn the car towards the direction it's travelling
        rb.AddRelativeTorque(Vector3.Cross(Vector3.forward, surfaceVelocity) * Mathf.Clamp01((45 - absVelocityAlignmentDifference) / 45) * forwardGrip * Time.deltaTime, ForceMode.VelocityChange);

        // Turn the car up to a maximum angle against the direction of movement
        var allowedTurnAngle = isSliding ? driftMaxTurnAngle : maxTurnAngle;
        var turnSpeed = isSliding ? driftTurnSpeed : this.turnSpeed;
        rb.AddRelativeTorque(Vector3.Cross(Vector3.forward, wheelForwardDirection) * Mathf.Clamp01((allowedTurnAngle - absVelocityAlignmentDifference) / allowedTurnAngle) * turnSpeed * Time.deltaTime, ForceMode.VelocityChange);

        var sidewaysVelocity = Vector3.Project(surfaceVelocity, Vector3.right);
        var sideFriction = sidewaysFriction.Evaluate(sidewaysVelocity.magnitude);
        var sidewaysFrictionForce = -sidewaysVelocity * sideFriction * surfaceFriction;
        debugger.ShowDebugValue("sidewaysVelocity", sidewaysVelocity, Color.magenta);
        debugger.ShowDebugValue("sidewaysFrictionForce", sidewaysFrictionForce, Color.red);
        rb.AddRelativeForce(sidewaysFrictionForce * Time.deltaTime, ForceMode.VelocityChange);

        if (!isReversing) {
            var brakeForce = Vector3.back * input.Brakes * brakingForceMultiplier * forwardGrip;
            debugger.ShowDebugValue("brakeForce", brakeForce, Color.red);
            rb.AddRelativeForce(brakeForce * Time.deltaTime, ForceMode.VelocityChange);
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
}