using UnityEngine;

public class NullDebugVectorTracker : IDebugVectorTracker {
    public void Log(string label, Vector3 vector, Color color, DebugVectorSpace space) { }
    public void Log(string label, Vector3 vector, DebugVectorSpace space) { }
}