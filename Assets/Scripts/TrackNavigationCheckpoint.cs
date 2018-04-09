using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackNavigationCheckpoint : MonoBehaviour {

	public List<TrackNavigationCheckpoint> next;

	public List<TrackNavigationCheckpoint> previous;

	public const float MaxNextDistance = 100f;

	private Plane passPlane;

	void Start() {
		passPlane = new Plane(transform.forward, transform.position);
	}

	void Update() {

	}

	public bool HasPassed(Vector3 position) {
		return passPlane.GetSide(position);
	}

	public float Distance(Vector3 position) {
		return Vector3.Distance(position, transform.position);
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