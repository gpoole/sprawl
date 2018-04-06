using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrackNavigationCheckpoint : MonoBehaviour {

	public List<TrackNavigationCheckpoint> next;

	public TrackNavigationCheckpoint previous;

	public const float MaxNextDistance = 100f;

	void Start() { }

	void Update() {

	}

	public void Recompute() {
		var plane = new Plane(transform.forward, transform.position);
		TrackNavigationCheckpoint newNext = null;
		float currentClosestDistance = MaxNextDistance;
		for (var i = 0; i < transform.parent.childCount; i++) {
			var potentialNext = transform.parent.GetChild(i);
			if (potentialNext != transform) {
				var distance = plane.GetDistanceToPoint(potentialNext.position);
				Debug.LogFormat("Distance to {0}: {1}", potentialNext.name, distance);
				if (distance > 0 && distance < currentClosestDistance) {
					newNext = potentialNext.gameObject.GetComponent<TrackNavigationCheckpoint>();
					currentClosestDistance = distance;
				}
			}
		}

		next = new List<TrackNavigationCheckpoint>();
		if (newNext != null) {
			next.Add(newNext);
		}

		foreach (var checkpoint in next) {
			checkpoint.previous = this;
		}
	}

	void OnDrawGizmos() {
		if (next != null) {
			foreach (var checkpoint in next) {
				if (checkpoint != null) {
					Gizmos.color = Color.blue;
					Gizmos.DrawLine(transform.position, checkpoint.transform.position);
				}
			}
		}
	}
}