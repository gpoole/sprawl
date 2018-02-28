using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarDebugger : MonoBehaviour {

    void Update() {
        if (Input.GetKeyUp(KeyCode.B)) {
            var rb = GetComponent<Rigidbody>();
            var t = GetComponent<Transform>();
            var mesh = GetComponent<BoxCollider>();
            rb.AddExplosionForce(750f, t.TransformPoint(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) + (Vector3.down * 2f), 50f);
        }
    }
}
