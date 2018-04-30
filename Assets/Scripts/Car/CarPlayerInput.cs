using System;
using InControl;
using UnityEngine;

public class CarPlayerInput : MonoBehaviour {

    public float Accelerator {
        get {
            return actions != null ? actions.accelerate : 0f;
        }
    }

    public float Brakes {
        get {
            return actions != null ? actions.brake : 0f;
        }
    }

    public float Steering {
        get {
            return actions != null ? actions.steer : 0f;
        }
    }

    public bool IsHandbraking {
        get {
            return actions != null ? actions.handbrake : false;
        }
    }

    public bool IsResetting {
        get {
            return actions != null ? actions.resetCar : false;
        }
    }

    // private InputDevice device;
    private DrivingPlayerActions actions;

    void Start() {
        actions = new DrivingPlayerActions();

        var playerState = GetComponent<Car>().playerState;
        if (playerState) {
            actions.Device = playerState.player.device;
        } else {
            Debug.Log("No playerState detected, using default input device.");
        }

        // Controller
        actions.accelerate.AddDefaultBinding(InputControlType.RightTrigger);
        actions.brake.AddDefaultBinding(InputControlType.LeftTrigger);
        actions.steerLeft.AddDefaultBinding(InputControlType.LeftStickLeft);
        actions.steerRight.AddDefaultBinding(InputControlType.LeftStickRight);
        actions.handbrake.AddDefaultBinding(InputControlType.Action2);
        actions.resetCar.AddDefaultBinding(InputControlType.Action3);

        // Keyboard
        actions.accelerate.AddDefaultBinding(Key.W);
        actions.brake.AddDefaultBinding(Key.S);
        actions.steerLeft.AddDefaultBinding(Key.A);
        actions.steerRight.AddDefaultBinding(Key.D);
        actions.handbrake.AddDefaultBinding(Key.Space);
        actions.resetCar.AddDefaultBinding(Key.R);
    }

}