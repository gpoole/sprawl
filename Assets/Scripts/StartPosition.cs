using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartPosition : MonoBehaviour {

	private static readonly Vector3 carBox = new Vector3(1.5f, 1.5f, 3f);

	void OnDrawGizmos() {
		var color = Color.gray;
		color.a = 0.5f;
		Gizmos.color = color;
		Gizmos.DrawCube(transform.position, carBox);
	}

}