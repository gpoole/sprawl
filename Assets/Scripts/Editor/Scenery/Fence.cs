using System.Collections;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Fence))]
public class FenceEditor : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();

        var fence = (Fence) target;
        if (GUILayout.Button("Update posts")) {
            fence.ApplySettings();
        }
    }
}