using System;
using InControl;
using UnityEngine;

public class CarPlayerInput : MonoBehaviour {

    public float Accelerator {
        get {
            return input != null ? input.RightTrigger : 0f;
        }
    }

    public float Brakes {
        get {
            return input != null ? input.LeftTrigger : 0f;
        }
    }

    public float Turning {
        get {
            return input != null ? input.LeftStickX : 0f;
        }
    }

    public bool IsHandbraking {
        get {
            return input != null ? input.Action2 : false; // b button
        }
    }

    public bool IsResetting {
        get {
            bool controllerReset = false;
            if (input != null) {
                controllerReset = input.GetControl(InputControlType.Back) || input.GetControl(InputControlType.Action3);
            }
            return controllerReset || Input.GetKey(KeyCode.R);
        }
    }

    private InputDevice input;

    private CarController car;

    void Start() {
        car = GetComponent<CarController>();
        input = GameManager.Instance.players[car.playerId].device;
    }

}