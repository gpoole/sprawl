using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackNavigationCheckpoint : MonoBehaviour {

	public List<TrackNavigationCheckpoint> next;

	public List<TrackNavigationCheckpoint> previous;

	public const float MaxNextDistance = 100f;

	void Start() { }

	void Update() {

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