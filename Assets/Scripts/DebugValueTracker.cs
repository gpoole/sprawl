using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;

public class DebugValueTracker : MonoBehaviour {

    [Serializable]
    public class DebugValue {
        [HideInInspector]
        public string label;
        [HideInInspector]
        public object value;
        public bool show = true;

        public override string ToString() {
            return String.Format("{0}: {1}", label, value);
        }
    }

    [Serializable]
    public class NumericDebugValue<T> : DebugValue {
        [HideInInspector]
        new public T value;
        public bool showMinMax = true;
        [HideInInspector]
        public T max;
        [HideInInspector]
        public T min;

        public override string ToString() {
            var output = String.Format("{0}: {1:0.000}", label, value);
            if (showMinMax) {
                output += String.Format("\n{0} (min): {1:0.000}\n{0} (max): {2:0.000}", label, min, max);
            }
            return output;
        }

    }

    public List<DebugValue> values = new List<DebugValue>();

    private Text debugText;

    void Awake() {
        debugText = GetComponent<Text>();
    }

    void Start() {
        debugText.text = "";
    }

    void Update() {
        debugText.text = "";
        foreach (var entry in values) {
            if (entry.show) {
                debugText.text += entry.ToString() + "\n";
            }
        }
    }

    public void Log(string label, object value) {
        var entry = values.SingleOrDefault(currentEntry => currentEntry.label == label);
        if (entry == null) {
            entry = new DebugValue { label = label, value = value };
            values.Add(entry);
        } else {
            entry.value = value;
        }
    }

    public void Log(string label, float value, bool showMinMax) {
        var entry = (NumericDebugValue<float>) values.SingleOrDefault(currentEntry => currentEntry.label == label);
        if (entry == null) {
            entry = new NumericDebugValue<float> { label = label, value = value, max = value, min = value, showMinMax = showMinMax };
            values.Add(entry);
        } else {
            entry.value = value;
            entry.max = Math.Max(entry.max, value);
            entry.min = Math.Min(entry.min, value);
        }
    }
}