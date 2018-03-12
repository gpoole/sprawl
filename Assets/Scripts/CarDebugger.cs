using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CarDebugger : MonoBehaviour {

    Vector3 initialPosition;

    Quaternion initialRotation;

    public Transform[] warpPoints;

    public Text carStatus;

    public string[] filterDebugMessages;

    private Dictionary<string, object> debugValues = new Dictionary<string, object>();

    private CarPlayerInput input;

    void Start() {
        initialPosition = GetComponent<Transform>().position;
        initialRotation = GetComponent<Transform>().rotation;
        input = GetComponent<CarPlayerInput>();
    }

    void Update() {
        var rb = GetComponent<Rigidbody>();
        var t = GetComponent<Transform>();

        // FIXME: change to input
        if (Input.GetKeyUp(KeyCode.B)) {
            var mesh = GetComponent<BoxCollider>();
            rb.AddExplosionForce(750f * rb.mass, t.TransformPoint(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) + (Vector3.down * 2f), 50f);
        }

        if (input.IsResetting) {
            WarpTo(initialPosition, initialRotation);
            debugValues.Clear();
            GetComponent<CarController>().Reset();
        }

        for (var i = 0; i < warpPoints.Length; i++) {
            if (Input.GetKeyUp(i.ToString())) {
                WarpTo(warpPoints[i].position, warpPoints[i].rotation);
            }
        }

        carStatus.text = "";
        foreach (KeyValuePair<string, object> entry in debugValues) {
            if (filterDebugMessages.Length == 0 || filterDebugMessages.Any(search => entry.Key.StartsWith(search))) {
                carStatus.text += entry.Key + "=" + entry.Value + "\n";
            }
        }
    }

    public void ShowDebugValue(string label, object value) {
        debugValues[label] = value;
    }

    public void ShowDebugValue(string label, float value) {
        debugValues[label] = value;

        var maxLabel = label + " (max)";
        if (debugValues.ContainsKey(maxLabel)) {
            debugValues[maxLabel] = Mathf.Max(value, (float) debugValues[maxLabel]);
        } else {
            debugValues[maxLabel] = value;
        }

        var minLabel = label + " (min)";
        if (debugValues.ContainsKey(minLabel)) {
            debugValues[minLabel] = Mathf.Min(value, (float) debugValues[minLabel]);
        } else {
            debugValues[minLabel] = value;
        }
    }

    void WarpTo(Vector3 position, Quaternion rotation) {
        var t = GetComponent<Transform>();
        var rb = GetComponent<Rigidbody>();
        t.position = position;
        t.rotation = rotation;
        rb.velocity = Vector3.zero;
    }
}