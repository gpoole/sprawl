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

    public bool Grounded {
        get;
        private set;
    }

    public float Compression {
        get { return prevCompression; }
    }

    private GameObject car;

    private Rigidbody carRigidBody;

    private CarController carController;

    private Transform visualWheel;

    private float wheelHeight;

    void Start() {
        if (transform.childCount > 0) {
            visualWheel = transform.GetChild(0);
            wheelHeight = visualWheel.GetComponent<MeshRenderer>().bounds.extents.y;
        }
        car = transform.parent.parent.gameObject;
        carRigidBody = car.GetComponent<Rigidbody>();
        isFrontWheel = transform.localPosition.z > 0;
        carController = car.GetComponent<CarController>();
    }

    void Update() {
        if (carController) {
            if (isFrontWheel) {
                transform.localRotation = Quaternion.AngleAxis(carController.WheelOrientation * visualTurnMultiplier, Vector3.up);
            }

            if (visualWheel) {
                if (Grounded) {
                    visualWheel.Rotate(Vector3.forward, visualRotationSpeed * (carController.Speed / carController.maxSpeed) * Time.deltaTime);
                } else {
                    visualWheel.Rotate(Vector3.forward, visualRotationSpeed * (carController.EngineSpeed / carController.maxEngineSpeed) * Time.deltaTime);
                }

                visualWheel.localPosition = (Vector3.down * (1 - prevCompression) * targetLength) + new Vector3(0, wheelHeight, 0);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        var tyreId = String.Format("wheel{0}{1}", isFrontWheel ? "Front" : "Back", transform.localPosition.x > 0 ? "Left" : "Right");
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, targetLength)) {
            var compressionRatio = 1f - (hit.distance / targetLength);
            var springForce = compressionRatio * springFactor;
            var dampingForce = (prevCompression - compressionRatio) * dampingFactor;
            var totalForce = springForce - dampingForce;
            prevCompression = compressionRatio;
            carRigidBody.AddForceAtPosition(transform.TransformDirection(Vector3.up) * totalForce * Time.deltaTime, transform.position, ForceMode.VelocityChange);
            Grounded = true;
        } else {
            prevCompression = 0f;
            Grounded = false;
        }
    }

    void OnDrawGizmos() {
        var transform = GetComponent<Transform>();
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.down * (1 - prevCompression) * targetLength));
    }
}