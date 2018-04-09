using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class TrackNavigation : MonoBehaviour {

	public TrackNavigationCheckpoint start;

	public MeshCollider innerBounds;

	public MeshCollider outerBounds;

	public static TrackNavigation Instance {
		get;
		private set;
	}

	public const int MaxSkipCheckpoints = 2;

	protected const float ForwardProjectDistance = 120f;

	protected const int MaxIterations = 250;

	void Awake() {
		Instance = this;
	}

	void Start() {

	}

	void Update() {

	}

	[MenuItem("Track/Generate checkpoints")]
	public static void GenerateCheckpoints() {
		var nav = GameObject.FindObjectOfType<TrackNavigation>();

		if (!nav) {
			Debug.LogError("Can't find TrackNavigation instance.");
			return;
		}

		nav.start.next = new List<TrackNavigationCheckpoint>();
		nav.start.previous = null;
		foreach (var checkpoint in nav.transform.GetComponentsInChildren<TrackNavigationCheckpoint>()) {
			if (checkpoint.name.StartsWith("Checkpoint")) {
				DestroyImmediate(checkpoint.gameObject);
			}
		}

		TrackNavigationCheckpoint current = nav.start;
		for (var i = 0; i < MaxIterations; i++) {
			var projectedPoint = current.transform.position + (current.transform.forward.normalized * ForwardProjectDistance);
			var closestInnerPoint = NearestPointOnBounds(nav.innerBounds, projectedPoint);
			var closestOuterPoint = NearestPointOnBounds(nav.outerBounds, projectedPoint);
			var midPoint = closestInnerPoint + ((closestOuterPoint - closestInnerPoint) / 2);

			var rotation = Quaternion.LookRotation(midPoint - current.transform.position);
			current.transform.rotation = rotation;

			TrackNavigationCheckpoint next = null;
			if (current != nav.start && Vector3.Distance(midPoint, nav.start.transform.position) < ForwardProjectDistance) {
				next = nav.start;
			} else {
				var newCheckpoint = new GameObject();
				newCheckpoint.transform.parent = nav.transform;
				newCheckpoint.name = "Checkpoint" + (i + 1);
				newCheckpoint.transform.position = midPoint;
				newCheckpoint.transform.rotation = rotation;
				next = (TrackNavigationCheckpoint) newCheckpoint.AddComponent(typeof(TrackNavigationCheckpoint));
			}

			next.previous = new List<TrackNavigationCheckpoint> { current };
			current.next = new List<TrackNavigationCheckpoint> { next };

			if (next == nav.start) {
				break;
			}
			current = next;
		}
	}

	private static Vector3 NearestPointOnBounds(MeshCollider bounds, Vector3 worldPoint) {
		var mesh = bounds.sharedMesh;
		var point = bounds.transform.InverseTransformPoint(worldPoint);
		float minDistanceSqr = Mathf.Infinity;
		Vector3 nearestVertex = Vector3.zero;
		// scan all vertices to find nearest
		foreach (Vector3 vertex in mesh.vertices) {
			Vector3 diff = point - vertex;
			float distSqr = diff.sqrMagnitude;
			if (distSqr < minDistanceSqr) {
				minDistanceSqr = distSqr;
				nearestVertex = vertex;
			}
		}
		// convert nearest vertex back to world space
		return bounds.transform.TransformPoint(nearestVertex);
	}

	public TrackNavigationCheckpoint UpdateCurrentCheckpoint(TrackNavigationCheckpoint currentCheckpoint, Vector3 currentPosition) {
		var current = currentCheckpoint;
		for (var i = 0; i < MaxSkipCheckpoints; i++) {
			TrackNavigationCheckpoint nextNearest = null;
			foreach (var next in current.next) {
				if (next.HasPassed(currentPosition) && (nextNearest == null || next.Distance(currentPosition) < nextNearest.Distance(currentPosition))) {
					nextNearest = next;
				}
			}

			if (nextNearest != null) {
				current = nextNearest;
			} else {
				break;
			}
		}
		return current;
	}

}