using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System;
using UnityEngine;
using UnityEngine.UI;

public class DebugVectorTracker : MonoBehaviour, IDebugVectorTracker {

	[Serializable]
	public class VectorDebug {
		public string label;
		public Color color;
		public Vector3 vector;
		public DebugVectorSpace space;
		public bool show = true;
		public bool solo = false;
	}

	public List<VectorDebug> vectors = new List<VectorDebug>();

	private static readonly Color[] Colors = {
		Color.blue,
		Color.cyan,
		Color.green,
		Color.magenta,
		Color.red,
		Color.yellow,
	};

	private int lastAutoColourIndex = 0;

	private bool showMessages;

	public static IDebugVectorTracker Create(GameObject target) {
		if (GameManager.Instance && !GameManager.Instance.debugMode) {
			return new NullDebugVectorTracker();
		}

		var existingTracker = target.GetComponent<DebugVectorTracker>();
		if (existingTracker != null) {
			return existingTracker;
		}
		return (DebugVectorTracker) target.AddComponent(typeof(DebugVectorTracker));
	}

	void OnDrawGizmos() {
		var hasSolo = vectors.Any(debugVector => debugVector.solo);
		foreach (var debugVector in vectors) {
			if ((!hasSolo && debugVector.show) || (hasSolo && debugVector.solo)) {
				Gizmos.color = debugVector.color;

				var transformedVector = debugVector.vector;
				if (debugVector.space == DebugVectorSpace.Local) {
					transformedVector = transform.TransformDirection(debugVector.vector);
				}
				var debugRay = transformedVector.normalized * Mathf.Log((debugVector.vector.magnitude * 5) + 1);
				Gizmos.DrawRay(transform.position, debugRay);
#if UNITY_EDITOR
				Handles.Label(transform.position + debugRay, debugVector.label + "\n" + debugVector.vector);
#endif
			}
		}
	}

	public void Log(string label, Vector3 value, Color color, DebugVectorSpace space = DebugVectorSpace.Local) {
		var entry = vectors.FirstOrDefault(vectorDebug => vectorDebug.label == label);
		if (entry == null) {
			entry = new VectorDebug();
			entry.label = label;
			entry.color = color;
			vectors.Add(entry);
		}

		entry.vector = value;
	}

	public void Log(string label, Vector3 value, DebugVectorSpace space = DebugVectorSpace.Local) {
		var colorIndex = lastAutoColourIndex++;
		if (lastAutoColourIndex >= Colors.Length) {
			lastAutoColourIndex = 0;
		}
		Log(label, value, Colors[colorIndex], space);
	}
}