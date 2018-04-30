using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Fence : MonoBehaviour {

	public float joinBeamDistance = 2f;

	public float postMass = 50f;

	public float railMass = 25f;

	public float railBreakForce = 20000f;

	public float railBreakTorque = Mathf.Infinity;

	public float postGroundBreakForce = 60000f;

	public float postGroundBreakTorque = Mathf.Infinity;

	public void ApplySettings() {
		var railGroups = transform.Find("Rails");
		Debug.Assert(railGroups, "No rails found, is this an autofence fence?");
		var rails = GetRenderableChildren(railGroups);
		foreach (var rail in rails) {
			ScrubComponents<BoxCollider>(rail);
			ScrubComponents<Animator>(rail);
			ScrubComponents<FixedJoint>(rail);
			ScrubComponents<Rigidbody>(rail);
			rail.tag = "FenceBeam";
			rail.AddComponent(typeof(BoxCollider));
			var railRigidBody = (Rigidbody) rail.AddComponent(typeof(Rigidbody));
			railRigidBody.mass = railMass;
		}

		var postGroups = transform.Find("Posts");
		var posts = GetRenderableChildren(postGroups);
		Debug.Assert(postGroups, "No posts found, is this an autofence fence?");
		foreach (var post in posts) {
			ScrubComponents<Animator>(post);
			ScrubComponents<BoxCollider>(post);
			ScrubComponents<FixedJoint>(post);
			ScrubComponents<Rigidbody>(post);
			post.AddComponent(typeof(BoxCollider));
			post.AddComponent(typeof(Rigidbody));
			InitPost(post);
		}

		foreach (var combineScript in transform.GetComponentsInChildren<CombineChildrenPlus>()) {
			Object.DestroyImmediate(combineScript);
		}
	}

	private void InitPost(GameObject post) {
		var beams = Physics.OverlapBox(post.transform.position, post.GetComponent<BoxCollider>().size * joinBeamDistance)
			.Where(collider => collider.gameObject.tag == "FenceBeam")
			.Select(collider => collider.gameObject);
		var postRigidBody = post.GetComponent<Rigidbody>();
		postRigidBody.mass = postMass;

		var postFixedJoint = (FixedJoint) post.AddComponent(typeof(FixedJoint));
		postFixedJoint.breakForce = postGroundBreakForce;
		postFixedJoint.breakTorque = postGroundBreakTorque;

		foreach (var beam in beams) {
			Rigidbody beamRigidBody = beam.GetComponent<Rigidbody>();
			if (beamRigidBody == null) {
				beamRigidBody = (Rigidbody) beam.AddComponent(typeof(Rigidbody));
				beamRigidBody.mass = railMass;
			}

			var beamPostJoint = (FixedJoint) post.AddComponent(typeof(FixedJoint));
			beamPostJoint.connectedBody = beamRigidBody;
			beamPostJoint.breakForce = railBreakForce;
			beamPostJoint.breakTorque = railBreakTorque;
		}
	}

	private IEnumerable<GameObject> GetRenderableChildren(Transform target) {
		return target.GetComponentsInChildren<MeshRenderer>().Select(mesh => mesh.gameObject);
	}

	private void ScrubComponents<T>(GameObject target) where T : Component {
		var components = target.GetComponents<T>();
		foreach (var component in components) {
			Object.DestroyImmediate(component);
		}
	}

}