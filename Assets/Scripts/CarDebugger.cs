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

    private bool showMessages {
        get { return carStatus; }
    }

    private int playerId;

    void Start() {
        initialPosition = GetComponent<Transform>().position;
        initialRotation = GetComponent<Transform>().rotation;
        input = GetComponent<CarPlayerInput>();
        playerId = GetComponent<CarController>().playerId;
        if (playerId == 0) {
            var statusUi = GameObject.Find("DebugInfo");
            if (statusUi) {
                carStatus = statusUi.GetComponent<Text>();
            }
        }
    }

    void Update() {
        var rb = GetComponent<Rigidbody>();
        var t = GetComponent<Transform>();

        // FIXME: change to input
        if (Input.GetKeyUp(KeyCode.B)) {
            var mesh = GetComponent<BoxCollider>();
            rb.AddExplosionForce(750f * rb.mass, t.TransformPoint(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) + (Vector3.down * 2f), 50f);
        }

        if (showMessages) {
            carStatus.text = "";
            foreach (KeyValuePair<string, object> entry in debugValues) {
                if (ShowValue(entry.Key)) {
                    carStatus.text += entry.Key + "=" + entry.Value + "\n";
                }
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
                var debugRay = transform.TransformDirection(entry.Value.Vector).normalized * Mathf.Log((entry.Value.Vector.magnitude * 10) + 1);
                Gizmos.DrawRay(transform.position, debugRay);
#if UNITY_EDITOR
                Handles.Label(transform.position + debugRay, entry.Key + "\n" + entry.Value.Vector);
#endif
            }
        }
    }

    public void Log(string label, Vector3 value, Color color) {
        if (debugVectors.ContainsKey(label)) {
            VectorDebug existing = debugVectors[label];
            existing.Vector = value;
            existing.Color = color;
            debugVectors[label] = existing;
        } else {
            debugVectors[label] = new VectorDebug { Vector = value, Color = color };
        }
    }

    public void Log(string label, object value) {
        if (!showMessages) {
            return;
        }
        debugValues[label] = value;
    }

    public void Log(string label, float value, bool showMinMax = true) {
        if (!showMessages) {
            return;
        }
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
}