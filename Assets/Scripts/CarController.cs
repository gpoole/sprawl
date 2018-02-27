using UnityEngine;
using System;

public class CarController : MonoBehaviour {

    [Range(0f, 50f)]
    public float turningFactor = 10f;

    [Range(0f, 50f)]
    public float accelerationFactor = 10f;

    [Range(0f, 5f)]
    public float frictionFactor = 2f;

    public float suspensionSpringLength = 0.7f;

    public float suspensionBounce = 20f;

    private float wheelOrientation = 0f;

    void Start() { }

    public float GetSpeed() {
        return GetComponent<Rigidbody>().velocity.magnitude;
    }

    void FixedUpdate() {
        var transform = GetComponent<Transform>();
        var colliderRb = GetComponent<Rigidbody>();

        var turning = Input.GetAxis("Horizontal");
        wheelOrientation = Mathf.Lerp(wheelOrientation, 30f * turning, Time.deltaTime * 10f);

        var wheelRotation = Quaternion.AngleAxis(wheelOrientation, Vector3.up);
        var wheelForwardDirection = wheelRotation * transform.forward.normalized;

        var acceleration = Input.GetAxis("Vertical");
        colliderRb.AddForce(wheelForwardDirection * acceleration * accelerationFactor);

        colliderRb.AddRelativeTorque(Vector3.up * wheelOrientation * turningFactor);

        var velocityDirection = colliderRb.velocity.normalized;
        var orientation = new Vector3(transform.right.x, 0, transform.right.z);
        var frictionVector = new Vector3(-velocityDirection.x, 0, -velocityDirection.z);
        var sidewaysness = Mathf.Abs(Vector3.Dot(velocityDirection, orientation));
        colliderRb.AddForce(frictionVector * colliderRb.velocity.magnitude * sidewaysness * frictionFactor);
    }
}
