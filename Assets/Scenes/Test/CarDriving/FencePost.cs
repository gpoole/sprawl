using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FencePost : MonoBehaviour {

	public float crossBeamMass = 25f;

	public float postMass = 100f;

	public float crossBeamBreakForce = 5000f;

	public float crossBeamBreakTorque = 5000f;

	public float postGroundBreakForce = 100f;

	public float joinBeamDistance = 2f;

	void Start() {
		var postRigidBody = (Rigidbody) gameObject.AddComponent(typeof(Rigidbody));
		postRigidBody.mass = postMass;

		var postGroundJoint = (FixedJoint) gameObject.AddComponent(typeof(FixedJoint));
		postGroundJoint.connectedBody = GameObject.Find("Ground").GetComponent<Rigidbody>();
		postGroundJoint.breakForce = postGroundBreakForce;

		var beams = Physics.OverlapBox(transform.position, GetComponent<BoxCollider>().size * joinBeamDistance)
			.Where(collider => collider.gameObject.tag == "FenceBeam")
			.Select(collider => collider.gameObject);
		Debug.LogFormat("Found {0} beams", beams.Count());

		foreach (var beam in beams) {
			if (beam.GetComponent<Rigidbody>() == null) {
				var beamRigidBody = (Rigidbody) beam.AddComponent(typeof(Rigidbody));
				beamRigidBody.mass = crossBeamMass;
			}

			var beamPostJoint = (FixedJoint) beam.AddComponent(typeof(FixedJoint));
			beamPostJoint.connectedBody = postRigidBody;
			beamPostJoint.breakForce = crossBeamBreakForce;
			beamPostJoint.breakTorque = crossBeamBreakTorque;
			beamPostJoint.enablePreprocessing = false;
		}
	}
}