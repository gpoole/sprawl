using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class FenceTools {

	private const float JoinBeamDistance = 2f;

	private const float PostMass = 50f;

	private const float RailMass = 25f;

	private const float RailBreakForce = 250f;

	private const float RailBreakTorque = 250f;

	private const float PostGroundBreakForce = 3000f;

	private const float PostGroundBreakTorque = 3000f;

	[MenuItem("Tools/Scrub fence")]
	public static void ScrubFence() {
		var railGroups = Selection.activeTransform.Find("Rails");
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
			railRigidBody.mass = RailMass;
		}

		var postGroups = Selection.activeTransform.Find("Posts");
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

		foreach (var combineScript in Selection.activeTransform.GetComponentsInChildren<CombineChildrenPlus>()) {
			Object.DestroyImmediate(combineScript);
		}
	}

	private static void InitPost(GameObject post) {
		var beams = Physics.OverlapBox(post.transform.position, post.GetComponent<BoxCollider>().size * JoinBeamDistance)
			.Where(collider => collider.gameObject.tag == "FenceBeam")
			.Select(collider => collider.gameObject);
		var postRigidBody = post.GetComponent<Rigidbody>();
		postRigidBody.mass = PostMass;

		var postFixedJoint = (FixedJoint) post.AddComponent(typeof(FixedJoint));
		postFixedJoint.breakForce = PostGroundBreakForce;
		postFixedJoint.breakTorque = PostGroundBreakTorque;

		foreach (var beam in beams) {
			Rigidbody beamRigidBody = beam.GetComponent<Rigidbody>();
			if (beamRigidBody == null) {
				beamRigidBody = (Rigidbody) beam.AddComponent(typeof(Rigidbody));
				beamRigidBody.mass = RailMass;
			}

			var beamPostJoint = (FixedJoint) post.AddComponent(typeof(FixedJoint));
			beamPostJoint.connectedBody = beamRigidBody;
			beamPostJoint.breakForce = RailBreakForce;
			beamPostJoint.breakTorque = RailBreakTorque;
		}
	}

	private static IEnumerable<GameObject> GetRenderableChildren(Transform target) {
		return target.GetComponentsInChildren<MeshRenderer>().Select(mesh => mesh.gameObject);
	}

	private static void ScrubComponents<T>(GameObject target) where T : Component {
		var components = target.GetComponents<T>();
		foreach (var component in components) {
			Object.DestroyImmediate(component);
		}
	}

}