using UnityEngine;

public interface IDebugVectorTracker {
    void Log(string label, Vector3 value, Color color, DebugVectorSpace space = DebugVectorSpace.Local);
    void Log(string label, Vector3 value, DebugVectorSpace space = DebugVectorSpace.Local);
}