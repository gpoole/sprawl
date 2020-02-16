using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWheelSuspension : MonoBehaviour {

    public float targetLength;

    public float dampingFactor;

    public float springFactor;

    public float maxAngleToSurface = 45f;

    public bool isGrounded {
        get;
        private set;
    }

    public Rigidbody connectedBody;

    private float prevCompression = 0f;

    private float wheelHeight;

    private Transform visualWheel;

    private Vector3 suspensionTop;

    private Vector3 wheelBottom;

    void Start() {
        if (transform.childCount > 0) {
            visualWheel = transform.GetChild(0);
            wheelHeight = visualWheel.TransformVector(visualWheel.GetComponent<MeshRenderer>().bounds.extents).y;
        }
        suspensionTop = Vector3.up * (targetLength / 2);

        if (connectedBody == null) {
            connectedBody = GetComponentInParent<Rigidbody>();
        }
    }

    void FixedUpdate() {
        RaycastHit hit;
        if (Physics.Raycast(transform.TransformPoint(suspensionTop), transform.TransformDirection(Vector3.down), out hit, targetLength)) {
            var surfaceAlignment = 1 - Mathf.Clamp01(Mathf.Abs(90 - Vector3.Angle(hit.normal, transform.right)) / maxAngleToSurface);
            var compressionRatio = 1 - (hit.distance / targetLength);
            var springForce = compressionRatio * surfaceAlignment * springFactor;
            var dampingForce = (prevCompression - compressionRatio) * dampingFactor;
            var totalForce = springForce - dampingForce;
            prevCompression = compressionRatio;
            connectedBody.AddForceAtPosition(transform.TransformDirection(Vector3.up) * totalForce * Time.deltaTime, transform.position, ForceMode.VelocityChange);
            isGrounded = surfaceAlignment > 0;

            var visualSuspensionLength = Mathf.Clamp((1 - compressionRatio) * targetLength, targetLength * 0.75f, targetLength);
            wheelBottom = transform.TransformPoint(suspensionTop + (Vector3.down * visualSuspensionLength));
            visualWheel.position = wheelBottom - transform.TransformDirection(Vector3.down * wheelHeight);
        } else {
            prevCompression = 0f;
            isGrounded = false;
        }
    }

    void OnDrawGizmos() {
        var transform = GetComponent<Transform>();
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.TransformPoint(suspensionTop), transform.TransformDirection(Vector3.down * (1 - prevCompression) * targetLength));
        Gizmos.DrawSphere(wheelBottom, 0.1f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(transform.TransformPoint(suspensionTop), 0.1f);
    }
}