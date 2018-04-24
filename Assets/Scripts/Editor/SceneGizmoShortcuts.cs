using System.Collections;
using UnityEditor;
using UnityEngine;

class SceneGizmoShortcuts {

	[MenuItem("Gizmo/Front View _1")]
	static void FrontView() {
		GetSceneView().orthographic = true;
		GetSceneView().LookAtDirect(GetSceneView().pivot,
			Quaternion.LookRotation(Vector3.forward));
	}

	[MenuItem("Gizmo/Side View _3")]
	static void SideView() {
		GetSceneView().orthographic = true;
		GetSceneView().LookAtDirect(GetSceneView().pivot,
			Quaternion.LookRotation(Vector3.right));
	}

	[MenuItem("Gizmo/Top View _7")]
	static void TopView() {
		GetSceneView().orthographic = true;
		GetSceneView().LookAtDirect(GetSceneView().pivot,
			Quaternion.LookRotation(Vector3.down));
	}

	[MenuItem("Gizmo/Perspective View _5")]
	static void PerspectiveView() {
		GetSceneView().orthographic = !GetSceneView().orthographic;
		GetSceneView().LookAtDirect(GetSceneView().pivot,
			Quaternion.LookRotation(Vector3.forward + Vector3.right + Vector3.down));
	}

	static SceneView GetSceneView() {
		SceneView activeSceneView = null;

		if (SceneView.lastActiveSceneView != null) {
			activeSceneView = SceneView.lastActiveSceneView;
		} else if (SceneView.sceneViews.Count != 0) {
			activeSceneView = (SceneView.sceneViews[0] as SceneView);
		}

		return activeSceneView;
	}
}