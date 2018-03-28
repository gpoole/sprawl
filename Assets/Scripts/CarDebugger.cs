using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class CarDebugger : MonoBehaviour {

    protected struct VectorDebug {
        public Color Color;
        public Vector3 Vector;
        public float Scale;
    }

    Vector3 initialPosition;

    Quaternion initialRotation;

    public Transform[] warpPoints;

    public Text carStatus;

    public string[] filterDebugMessages;

    public bool autoMinMax = true;

    private Dictionary<string, object> debugValues = new Dictionary<string, object>();

    private Dictionary<string, VectorDebug> debugVectors = new Dictionary<string, VectorDebug>();

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
            if (ShowValue(entry.Key)) {
                carStatus.text += entry.Key + "=" + entry.Value + "\n";
            }
        }
    }

    bool ShowValue(string label) {
        return (filterDebugMessages.Length == 0 || filterDebugMessages.Any(search => label.StartsWith(search)));
    }

    void OnDrawGizmos() {
        foreach (KeyValuePair<string, VectorDebug> entry in debugVectors) {
            if (ShowValue(entry.Key)) {
                Gizmos.color = entry.Value.Color;
                var vector = transform.TransformDirection(entry.Value.Vector) * entry.Value.Scale;
                Gizmos.DrawRay(transform.position, vector);
#if UNITY_EDITOR
                Handles.Label(transform.position + vector, entry.Key + "\n" + entry.Value.Vector);
#endif
            }
        }
    }

    public void ShowDebugValue(string label, object value) {
        debugValues[label] = value;
    }

    public void ShowDebugValue(string label, Vector3 value, Color color, float scale = 1f) {
        if (debugVectors.ContainsKey(label)) {
            VectorDebug existing = debugVectors[label];
            existing.Vector = value;
            existing.Color = color;
            existing.Scale = scale;
            debugVectors[label] = existing;
        } else {
            debugVectors[label] = new VectorDebug { Vector = value, Color = color, Scale = scale };
        }
    }

    public void ShowDebugValue(string label, float value, bool showMinMax = true) {
        debugValues[label] = value;

        if (autoMinMax && showMinMax) {
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
    }

    void WarpTo(Vector3 position, Quaternion rotation) {
        var t = GetComponent<Transform>();
        var rb = GetComponent<Rigidbody>();
        t.position = position;
        t.rotation = rotation;
        rb.velocity = Vector3.zero;
    }
}