﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWheel : MonoBehaviour {

    public float targetLength = 0.5f;

    public float dampingFactor;

    public float springFactor;

    public float maxAngleToSurface = 45f;

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
        get;
        private set;
    }

    private GameObject car;

    private Rigidbody carRigidBody;

    private CarController carController;

    private Transform visualWheel;

    private Vector3 suspensionTop;

    private float prevWheelOrientation;

    void Start() {
        if (transform.childCount > 0) {
            visualWheel = transform.GetChild(0);
            WheelHeight = visualWheel.TransformVector(visualWheel.GetComponent<MeshRenderer>().bounds.extents).y;
        }
        carController = GetComponentInParent<CarController>();
        car = carController.gameObject;
        carRigidBody = car.GetComponent<Rigidbody>();
        isFrontWheel = transform.localPosition.z > 0;
        suspensionTop = Vector3.up * (targetLength / 2);
        prevWheelOrientation = carController.WheelOrientation;
    }

    void Update() {
        if (carController) {
            if (isFrontWheel) {
                var wheelOrientation = Mathf.Lerp(prevWheelOrientation, carController.WheelOrientation, Time.deltaTime * 5f);
                transform.localRotation = Quaternion.AngleAxis(wheelOrientation * visualTurnMultiplier, Vector3.up);
                prevWheelOrientation = wheelOrientation;
            }

            if (visualWheel) {
                if (IsGrounded) {
                    visualWheel.Rotate(Vector3.forward, visualRotationSpeed * (carController.Speed / carController.maxSpeed) * Time.deltaTime);
                } else {
                    visualWheel.Rotate(Vector3.forward, visualRotationSpeed * carController.EngineSpeed * Time.deltaTime);
                }

                var visualSuspensionLength = Mathf.Clamp((1 - prevCompression) * targetLength, targetLength * 0.75f, targetLength);
                WheelBottom = transform.TransformPoint(suspensionTop + (Vector3.down * visualSuspensionLength));
                visualWheel.position = WheelBottom - transform.TransformDirection(Vector3.down * WheelHeight);
            }
        }
    }

    void FixedUpdate() {
        RaycastHit hit;
        if (Physics.Raycast(transform.TransformPoint(suspensionTop), transform.TransformDirection(Vector3.down), out hit, targetLength)) {
            var surfaceAlignment = 1 - Mathf.Clamp01(Mathf.Abs(90 - Vector3.Angle(hit.normal, transform.right)) / maxAngleToSurface);
            var compressionRatio = 1 - (hit.distance / targetLength);
            var springForce = compressionRatio * surfaceAlignment * springFactor;
            var dampingForce = (prevCompression - compressionRatio) * dampingFactor;
            // FIXME: this should be relative to the downward force being exerted
            var totalForce = springForce - dampingForce;
            prevCompression = compressionRatio;
            carRigidBody.AddForceAtPosition(transform.TransformDirection(Vector3.up) * totalForce * Time.deltaTime, transform.position, ForceMode.VelocityChange);
            IsGrounded = surfaceAlignment > 0;
        } else {
            prevCompression = 0f;
            IsGrounded = false;
        }
    }

    void OnDrawGizmos() {
        var transform = GetComponent<Transform>();
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.TransformPoint(suspensionTop), transform.TransformDirection(Vector3.down * (1 - prevCompression) * targetLength));
        Gizmos.DrawSphere(WheelBottom, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.TransformPoint(suspensionTop), 0.1f);
    }
}