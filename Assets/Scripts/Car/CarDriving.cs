using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CarPlayerInput))]
public class CarDriving : MonoBehaviour {

    public float engineForce;

    public float turnSpeed;

    public float turnForce;

    public float maxTurnAngle;

    public float maxSpeed;

    public float engineSpeed {
        get;
        private set;
    }

    public bool isGrounded {
        get;
        private set;
    }

    public Vector3 surfaceVelocity {
        get;
        private set;
    }

    public Transform centreOfMass;

    // How far to cast rays down to hit the surface
    private const float SurfaceRaycastLength = 2f;

    private Vector3 surfaceForward;

    private Vector3 surfaceRight;

    private Rigidbody rigidbody;

    private CarWheelSuspension[] carWheels;

    private CarPlayerInput input;

    private IDebugVectorTracker debugVector;

    void Start() {
        rigidbody = GetComponent<Rigidbody>();
        carWheels = GetComponentsInChildren<CarWheelSuspension>();
        input = GetComponent<CarPlayerInput>();

        debugVector = DebugVectorTracker.Create(gameObject);
    }

    void FixedUpdate() {
        rigidbody.centerOfMass = centreOfMass.localPosition;
        UpdateDrivingState();
        if (isGrounded) {
            ApplyGroundAcceleration();
            ApplyGroundTurning();
        }
    }

    void UpdateDrivingState() {
        isGrounded = carWheels.Any(carWheel => carWheel.isGrounded);

        if (isGrounded) {
            RaycastHit surfaceHit;
            if (Physics.Raycast(transform.position, Vector3.down, out surfaceHit, SurfaceRaycastLength)) {
                surfaceForward = Vector3.ProjectOnPlane(transform.forward, surfaceHit.normal);
                debugVector.Log("surfaceForward", surfaceForward, DebugVectorSpace.World);
                surfaceRight = Vector3.Cross(surfaceForward, surfaceHit.normal);
                surfaceVelocity = Vector3.ProjectOnPlane(rigidbody.velocity, surfaceHit.normal);
                debugVector.Log("surfaceVelocity", surfaceVelocity, DebugVectorSpace.World);
            }
        }
    }

    void ApplyGroundAcceleration() {
        // Add acceleration force rear wheel drive style
        var accelerationForce = surfaceForward * input.Accelerator * engineForce;
        rigidbody.AddForce(accelerationForce, ForceMode.Acceleration);
        debugVector.Log("accelerationForce", accelerationForce);
    }

    void ApplyGroundTurning() {
        // Change the car's velocity in the direction we're turning
        var forwardSpeed = Vector3.Project(surfaceVelocity, surfaceForward).magnitude;
        var turnForceVector = -surfaceRight * (turnForce * input.Steering) * (forwardSpeed / maxSpeed);
        rigidbody.AddForce(turnForceVector, ForceMode.VelocityChange);
        debugVector.Log("turnForce", turnForceVector, DebugVectorSpace.World);

        // Turn the car to face the direction of tyres
        // var travelAngleDifference = Vector3.SignedAngle(surfaceForward, surfaceVelocity, Vector3.up);
        // Note: Maybe this should be measured around car up not world up
        var travelAngleDifference = Vector3.SignedAngle(surfaceForward, surfaceVelocity, Vector3.up);
        DebugValueTracker.Instance.Log("travelAngleDifference", travelAngleDifference);
        // torque velocity is degrees per second
        var turnVelocity = (travelAngleDifference * turnSpeed) - rigidbody.angularVelocity.y;
        rigidbody.AddRelativeTorque(Vector3.up * turnVelocity, ForceMode.VelocityChange);

        // Decrease angular velocity based on surface friction
        const float turnVelocityLoss = 0.1f;
        var frictionTorque = Vector3.up * rigidbody.angularVelocity.y * turnVelocityLoss;
        rigidbody.AddRelativeTorque(-frictionTorque, ForceMode.VelocityChange);
    }
}