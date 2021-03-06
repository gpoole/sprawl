﻿using System;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

public class TrackNavigation : MonoBehaviour {

	public TrackNavigationCheckpoint start;

	public MeshFilter innerBounds;

	public MeshFilter outerBounds;

	public static TrackNavigation Instance {
		get;
		private set;
	}

	public const int MaxSkipCheckpoints = 2;

	protected const float VerticalOffset = 2f;

	protected const float ForwardProjectDistance = 60f;

	protected const int MaxIterations = 250;

	void Awake() {
		Instance = this;
	}

	void Start() {

	}

	void Update() {

	}

#if UNITY_EDITOR
	[MenuItem("Tools/Track/Generate checkpoints")]
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

			// fudge the offset up to make sure we hit the track with the raycast
			midPoint.y += VerticalOffset;

			RaycastHit trackHit;
			if (Physics.Raycast(midPoint, Vector3.down, out trackHit, Mathf.Infinity, LayerMask.GetMask("Track"))) {
				midPoint.y = trackHit.point.y + VerticalOffset;
			} else {
				Debug.LogErrorFormat("Failed to locate track under checkpoint {0}", i + 1);
			}

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
				next.order = i + 1;
			}

			next.previous = new List<TrackNavigationCheckpoint> { current };
			current.next = new List<TrackNavigationCheckpoint> { next };

			if (next == nav.start) {
				break;
			}
			current = next;
		}
	}
#endif

	private static Vector3 NearestPointOnBounds(MeshFilter bounds, Vector3 worldPoint) {
		var mesh = bounds.sharedMesh;
		var point = bounds.transform.InverseTransformPoint(worldPoint);
		float minDistanceSqr = Mathf.Infinity;
		Vector3 nearestVertex = Vector3.zero;
		foreach (Vector3 vertex in mesh.vertices) {
			Vector3 diff = point - vertex;
			float distSqr = diff.sqrMagnitude;
			if (distSqr < minDistanceSqr) {
				minDistanceSqr = distSqr;
				nearestVertex = vertex;
			}
		}
		return bounds.transform.TransformPoint(nearestVertex);
	}

	public TrackNavigationCheckpoint UpdateCurrentCheckpoint(TrackNavigationCheckpoint currentCheckpoint, Vector3 currentPosition) {
		var current = currentCheckpoint;
		for (var i = 0; i < MaxSkipCheckpoints; i++) {
			TrackNavigationCheckpoint nextNearest = null;
			foreach (var next in current.next) {
				if (next.HasPassed(currentPosition) && (nextNearest == null || next.PointDistance(currentPosition) < nextNearest.PointDistance(currentPosition))) {
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