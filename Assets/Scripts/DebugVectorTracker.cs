using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DebugVectorTracker : MonoBehaviour {

	[Serializable]
	public class VectorDebug {
		public string label;
		public Color color;
		public Vector3 vector;
		public bool show = true;
	}

	public List<VectorDebug> vectors = new List<VectorDebug>();

	private bool showMessages;

	void Start() { }

	void OnDrawGizmos() {
		foreach (var debugVector in vectors) {
			if (debugVector.show) {
				Gizmos.color = debugVector.color;
				var debugRay = transform.TransformDirection(debugVector.vector).normalized * Mathf.Log((debugVector.vector.magnitude * 5) + 1);
				Gizmos.DrawRay(transform.position, debugRay);
#if UNITY_EDITOR
				Handles.Label(transform.position + debugRay, debugVector.label + "\n" + debugVector.vector);
#endif
			}
		}
	}

	public void Log(string label, Vector3 value, Color color) {
		var entry = vectors.FirstOrDefault(vectorDebug => vectorDebug.label == label);
		if (entry == null) {
			entry = new VectorDebug();
			entry.label = label;
			vectors.Add(entry);
		}

		entry.vector = value;
		entry.color = color;
	}
}