using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWheel : MonoBehaviour {

    public float targetLength = 0.5f;

    public float dampingFactor;

    public float springFactor;

    public float visualTurnMultiplier = 1.5f;

    public float visualRotationSpeed = 1f;

    private float prevCompression = 0f;

    private bool isFrontWheel;

    private GameObject car;

    private Transform visualWheel;

    public bool grounded {
        get;
        private set;
    }

    void Start() {
        var transform = GetComponent<Transform>();
        visualWheel = transform.GetChild(0);
        car = transform.parent.parent.gameObject;
        isFrontWheel = transform.localPosition.z > 0;
    }

    void Update() {
        var carController = car.GetComponent<CarController>();
        if (isFrontWheel) {
            transform.localRotation = Quaternion.AngleAxis(carController.WheelOrientation * visualTurnMultiplier, Vector3.up);
        }

        if (grounded) {
            visualWheel.Rotate(Vector3.forward, visualRotationSpeed * (carController.Speed / carController.maxSpeed) * Time.deltaTime);
        } else {
            visualWheel.Rotate(Vector3.forward, visualRotationSpeed * (carController.EngineSpeed / carController.maxEngineSpeed) * Time.deltaTime);
        }

        visualWheel.localPosition = Vector3.down * (1 - prevCompression) * targetLength;
    }

    // Update is called once per frame
    void FixedUpdate() {
        var transform = GetComponent<Transform>();
        // FIXME: probably better way to do that...
        var rb = car.GetComponent<Rigidbody>();
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, targetLength * 2)) {
            var compressionRatio = 1f - (hit.distance / targetLength);
            var springForce = compressionRatio * springFactor;
            var dampingForce = (prevCompression - compressionRatio) * dampingFactor;
            var totalForce = springForce - dampingForce;
            prevCompression = compressionRatio;
            rb.AddForceAtPosition(transform.TransformDirection(Vector3.up) * totalForce, transform.position, ForceMode.Acceleration);
            grounded = true;
        } else {
            grounded = false;
        }
    }

    void OnDrawGizmos() {
        var transform = GetComponent<Transform>();
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(new Vector3(0f, (1 - prevCompression) * -targetLength, 0f)));
    }
}