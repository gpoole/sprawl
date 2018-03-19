using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class CentreOfGravity : MonoBehaviour {

    public bool useUpdate;

	public bool useFixedUpdate;

	public bool useLateUpdate;

	public bool spin;

	private Transform centreOfMass;

	new private Rigidbody rigidbody;

	// Use this for initialization
	void Start () {
		rigidbody = GetComponent<Rigidbody>();
		centreOfMass = transform.Find("CentreOfMass");
		rigidbody.centerOfMass = centreOfMass.localPosition;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		if (useFixedUpdate) {
			rigidbody.centerOfMass = centreOfMass.localPosition;
		}
	}

	void LateUpdate() {
		if (useLateUpdate) {
			rigidbody.centerOfMass = centreOfMass.localPosition;
		}
	}

	void Update() {
		if (useUpdate) {
			rigidbody.centerOfMass = centreOfMass.localPosition;
		}

		if (spin) {
			rigidbody.AddRelativeTorque(new Vector3(0, 20f, 0), ForceMode.VelocityChange);
		}
	}

	void OnDrawGizmos() {
        if (rigidbody != null) {
            var comWorld = transform.TransformPoint(rigidbody.centerOfMass);
            Gizmos.DrawSphere(comWorld, 0.1f);
            Handles.Label(comWorld, rigidbody.centerOfMass.ToString());
        }
	}
}
