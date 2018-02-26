using UnityEngine;
using System;

public class CarController : MonoBehaviour {

    [Range(0f, 50f)]
    public float turningFactor = 10f;

    [Range(0f, 50f)]
    public float accelerationFactor = 10f;

    [Range(0f, 50f)]
    public float frictionFactor = 5f;

    void Start() { }

    public float GetSpeed() {
        return GetComponent<Rigidbody>().velocity.magnitude;
    }

    void FixedUpdate() {
        var transform = GetComponent<Transform>();
        var colliderRb = GetComponent<Rigidbody>();
        var acceleration = Input.GetAxis("Vertical");
        var turning = Input.GetAxis("Horizontal");
        colliderRb.AddRelativeTorque(Vector3.up * turning * colliderRb.velocity.normalized.magnitude * turningFactor);
        if (acceleration > 0) {
            colliderRb.AddForce(transform.forward.normalized * acceleration * accelerationFactor);
        }

        var velocityDirection = colliderRb.velocity.normalized;
        var orientation = new Vector3(transform.right.x, 0, transform.right.z);
        var frictionVector = new Vector3(-velocityDirection.x, 0, -velocityDirection.z);
        var sidewaysness = Mathf.Abs(Vector3.Dot(velocityDirection, orientation));
        Debug.Log(sidewaysness);
        Debug.Log("orientation=" + orientation);
        Debug.Log("velocity=" + velocityDirection);
        colliderRb.AddForce(frictionVector * colliderRb.velocity.magnitude * sidewaysness * frictionFactor);
    }
}
