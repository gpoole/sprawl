using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scenes.Physics {
	[RequireComponent(typeof(Rigidbody))]
	public class ConstantForce : MonoBehaviour {

		public Vector3 force;

		public Vector3 torque;

		public ForceMode mode;

		private Rigidbody rigidbody;

		void Start() {
			rigidbody = GetComponent<Rigidbody>();
		}

		void FixedUpdate() {
			if (force != null) {
				rigidbody.AddForce(force, mode);
			}

			if (torque != null) {
				rigidbody.AddTorque(torque, mode);
			}
		}
	}
}