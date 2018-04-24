using System;
using UnityEngine;

public class DriftCamera : MonoBehaviour {
    [Serializable]
    public class AdvancedOptions {
        public bool updateCameraInUpdate;
        public bool updateCameraInFixedUpdate = true;
        public bool updateCameraInLateUpdate;
    }

    public DriftCameraRig rig;
    public float smoothing = 6f;
    public AdvancedOptions advancedOptions;

    bool m_ShowingSideView;

    private void Start() {
        transform.position = rig.positionTarget.position;
        transform.LookAt(rig.lookAtTarget);
    }

    private void FixedUpdate() {
        if (advancedOptions.updateCameraInFixedUpdate)
            UpdateCamera();
    }

    private void Update() {
        if (advancedOptions.updateCameraInUpdate)
            UpdateCamera();
    }

    private void LateUpdate() {
        if (advancedOptions.updateCameraInLateUpdate)
            UpdateCamera();
    }

    private void UpdateCamera() {
        transform.position = Vector3.Lerp(transform.position, rig.positionTarget.position, Time.deltaTime * smoothing);
        transform.LookAt(rig.lookAtTarget);
    }
}