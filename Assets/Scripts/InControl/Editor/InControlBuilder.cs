#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace InControl {
	internal class InControlBuilder {
		[MenuItem("GameObject/Create Other/InControl/Manager", false, 1)]
		static void CreateInputManager() {
			MonoBehaviour component;
			if (component = GameObject.FindObjectOfType<InControlManager>()) {
				Selection.activeGameObject = component.gameObject;

				Debug.LogError("InControlManager component is already attached to selected object.");
				return;
			}

			GameObject gameObject = GameObject.Find("InControl") ?? new GameObject("InControl");
			gameObject.AddComponent<InControlManager>();
			Selection.activeGameObject = gameObject;

			Debug.Log("InControl manager object has been created.");
		}
	}
}
#endif