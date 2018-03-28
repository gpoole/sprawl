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

    public bool IsGrounded {
        get;
        private set;
    }

    public float Compression {
        get { return prevCompression; }
    }

    public float WheelHeight {
        get;
        private set;
    }

    public Vector3 WheelBottom {
        get { return visualWheel.position + new Vector3(0, WheelHeight, 0); }
    }

    public RaycastHit HitSurface {
        get;
        private set;
    }

    private GameObject car;

    private Rigidbody carRigidBody;

    private CarController carController;

    private Transform visualWheel;

    void Start() {
        if (transform.childCount > 0) {
            visualWheel = transform.GetChild(0);
            WheelHeight = visualWheel.GetComponent<MeshRenderer>().bounds.extents.y;
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
                if (IsGrounded) {
                    visualWheel.Rotate(Vector3.forward, visualRotationSpeed * (carController.Speed / carController.maxSpeed) * Time.deltaTime);
                } else {
                    visualWheel.Rotate(Vector3.forward, visualRotationSpeed * carController.EngineSpeed * Time.deltaTime);
                }

                visualWheel.localPosition = (Vector3.down * (1 - prevCompression) * targetLength) + new Vector3(0, WheelHeight, 0);
            }
        }
    }

    // Update is called once per frame
    void FixedUpdate() {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, targetLength)) {
            var compressionRatio = 1f - (hit.distance / targetLength);
            var springForce = compressionRatio * springFactor;
            var dampingForce = (prevCompression - compressionRatio) * dampingFactor;
            var totalForce = springForce - dampingForce;
            prevCompression = compressionRatio;
            carRigidBody.AddForceAtPosition(transform.TransformDirection(Vector3.up) * totalForce * Time.deltaTime, transform.position, ForceMode.VelocityChange);
            IsGrounded = true;
            HitSurface = hit;
        } else {
            prevCompression = 0f;
            IsGrounded = false;
        }
    }

    void OnDrawGizmos() {
        var transform = GetComponent<Transform>();
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.TransformDirection(Vector3.down * (1 - prevCompression) * targetLength));
    }
}