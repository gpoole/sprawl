﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarWheel : MonoBehaviour {

    public float targetLength = 0.5f;

    public float dampingFactor;

    public float springFactor;

    private float prevCompression = 0f;

    // Update is called once per frame
    void FixedUpdate() {
        var transform = GetComponent<Transform>();
        // FIXME: probably better way to do that...
        var rb = transform.parent.parent.gameObject.GetComponent<Rigidbody>();
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.down), out hit, targetLength * 2)) {
            var compressionRatio = 1f - (hit.distance / targetLength);
            var springForce = compressionRatio * springFactor;
            var dampingForce = (prevCompression - compressionRatio) * dampingFactor;
            var totalForce = springForce - dampingForce;
            prevCompression = compressionRatio;
            rb.AddForceAtPosition((transform.TransformDirection(Vector3.up) * totalForce) / Time.deltaTime, transform.position, ForceMode.Impulse);
        }
    }
}