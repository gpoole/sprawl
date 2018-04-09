using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour {

    public enum Category {
        GameLogic,
        CarPhysics,
    }

    protected struct VectorDebug {
        public Color Color;
        public Vector3 Vector;
    }

    public Text debugText;

    public string[] filterDebugMessages;

    public Category[] filterCategories;

    public bool autoMinMax = true;

    private Dictionary<Category, Dictionary<string, object>> debugValues = new Dictionary<Category, Dictionary<string, object>>();

    private Dictionary<string, VectorDebug> debugVectors = new Dictionary<string, VectorDebug>();

    private bool showMessages {
        get { return debugText; }
    }

    void Start() { }

    void Update() {
        if (showMessages) {
            debugText.text = "";
            foreach (KeyValuePair<Category, Dictionary<string, object>> category in debugValues) {
                if (ShowCategory(category.Key)) {
                    foreach (KeyValuePair<string, object> entry in category.Value) {
                        if (ShowValue(entry.Key)) {
                            debugText.text += entry.Key + "=" + entry.Value + "\n";
                        }
                    }
                }
            }
        }
    }

    bool ShowValue(string label) {
        return (filterDebugMessages.Length == 0 || filterDebugMessages.Any(search => label.StartsWith(search)));
    }

    bool ShowCategory(Category category) {
        return (filterCategories.Length == 0 || filterCategories.Contains(category));
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

    Dictionary<string, object> GetCategoryVars(Category category) {
        if (!debugValues.ContainsKey(category)) {
            debugValues[category] = new Dictionary<string, object>();
        }
        return debugValues[category];
    }

    public void Log(Category category, string label, object value) {
        if (!showMessages) {
            return;
        }
        GetCategoryVars(category) [label] = value;
    }

    public void Log(Category category, string label, float value, bool showMinMax = true) {
        if (!showMessages) {
            return;
        }
        var categoryVars = GetCategoryVars(category);

        categoryVars[label] = value;

        if (autoMinMax && showMinMax) {
            var maxLabel = label + " (max)";
            if (categoryVars.ContainsKey(maxLabel)) {
                categoryVars[maxLabel] = Mathf.Max(value, (float) categoryVars[maxLabel]);
            } else {
                categoryVars[maxLabel] = value;
            }

            var minLabel = label + " (min)";
            if (categoryVars.ContainsKey(minLabel)) {
                categoryVars[minLabel] = Mathf.Min(value, (float) categoryVars[minLabel]);
            } else {
                categoryVars[minLabel] = value;
            }
        }
    }
}