using System;
using InControl;
using UnityEngine;

public class CarPlayerInput : MonoBehaviour {

    public float Accelerator {
        get {
            return device != null ? device.RightTrigger : 0f;
        }
    }

    public float Brakes {
        get {
            return device != null ? device.LeftTrigger : 0f;
        }
    }

    public float Turning {
        get {
            return device != null ? device.LeftStickX : 0f;
        }
    }

    public bool IsHandbraking {
        get {
            return device != null ? device.Action2 : false; // b button
        }
    }

    public bool IsResetting {
        get {
            bool controllerReset = false;
            if (device != null) {
                controllerReset = device.GetControl(InputControlType.Back) || device.GetControl(InputControlType.Action3);
            }
            return controllerReset || Input.GetKey(KeyCode.R);
        }
    }

    private InputDevice device;

    void Start() {
        var playerState = GetComponent<Car>().playerState;
        if (playerState) {
            device = playerState.player.device;
        } else {
            Debug.Log("No playerState detected, using default input device.");
            device = InputManager.ActiveDevice;
            InputManager.OnActiveDeviceChanged += (activeDevice) => device = activeDevice;
        }
    }

}