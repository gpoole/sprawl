using System;
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

    public bool grounded {
        get;
        private set;
    }

    private GameObject car;

    private Transform visualWheel;

    private CarDebugger debugger;

    void Start() {
        visualWheel = transform.GetChild(0);
        car = transform.parent.parent.gameObject;
        debugger = car.GetComponent<CarDebugger>();
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
        var rb = car.GetComponent<Rigidbody>();
        var tyreId = String.Format("wheel{0}{1}", isFrontWheel ? "Front" : "Back", transform.localPosition.x > 0 ? "Left" : "Right");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, targetLength)) {
            var compressionRatio = 1f - (hit.distance / targetLength);
            var springForce = compressionRatio * springFactor;
            var dampingForce = (prevCompression - compressionRatio) * dampingFactor;
            var totalForce = springForce - dampingForce;
            debugger.ShowDebugValue(tyreId + "Compression", compressionRatio);
            debugger.ShowDebugValue(tyreId + "SpringForce", springForce);
            debugger.ShowDebugValue(tyreId + "DampingForce", springForce);
            prevCompression = compressionRatio;
            rb.AddForceAtPosition(transform.TransformDirection(Vector3.up) * totalForce * Time.deltaTime, transform.position, ForceMode.VelocityChange);
            grounded = true;
        } else {
            prevCompression = 0f;
            grounded = false;
        }
    }

    void OnDrawGizmos() {
        var transform = GetComponent<Transform>();
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.down * (1 - prevCompression) * targetLength));
    }
}