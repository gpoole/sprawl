using System;
using InControl;
using UnityEngine;

public class CarPlayerInput : MonoBehaviour {

    public int playerNumber = 1;

    public float Accelerator {
        get {
            return controller.RightTrigger;
        }
    }

    public float Brakes {
        get {
            return controller.LeftTrigger;
        }
    }

    public float Turning {
        get {
            return controller.LeftStickX;
        }
    }

    public bool IsHandbraking {
        get {
            return controller.Action3; // x button
        }
    }

    public bool IsResetting {
        get {
            return controller.GetControl(InputControlType.Back);
        }
    }

    private InputDevice controller;

    void Start() {
        if (playerNumber <= InputManager.Devices.Count) {
            controller = InputManager.Devices[playerNumber - 1];
        } else {
            Debug.LogWarning(String.Format("PlayerInput: player {0} is greater than the number of controlers ({1})", playerNumber, InputManager.Devices.Count));
        }
    }

}