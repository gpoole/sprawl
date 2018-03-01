using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CarDebugger : MonoBehaviour {

    Vector3 initialPosition;

    Quaternion initialRotation;

    public Transform[] warpPoints;

    public Text carStatus;

    void Start() {
        initialPosition = GetComponent<Transform>().position;
        initialRotation = GetComponent<Transform>().rotation;
    }

    void Update() {
        var rb = GetComponent<Rigidbody>();
        var t = GetComponent<Transform>();

        if (Input.GetKeyUp(KeyCode.B)) {
            var mesh = GetComponent<BoxCollider>();
            rb.AddExplosionForce(750f * rb.mass, t.TransformPoint(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) + (Vector3.down * 2f), 50f);
        }

        if (Input.GetKeyUp(KeyCode.R)) {
            WarpTo(initialPosition, initialRotation);
        }

        for (var i = 0; i < warpPoints.Length; i++) {
            if (Input.GetKeyUp(i.ToString())) {
                WarpTo(warpPoints[i].position, warpPoints[i].rotation);
            }
        }

        var car = GetComponent<CarController>();
        carStatus.text = "Engine speed: " + car.engineSpeed.ToString() + "\n";
        carStatus.text += "Speed: " + car.speed;
    }

    void WarpTo(Vector3 position, Quaternion rotation) {
        var t = GetComponent<Transform>();
        var rb = GetComponent<Rigidbody>();
        t.position = position;
        t.rotation = rotation;
        rb.velocity = Vector3.zero;
    }
}
