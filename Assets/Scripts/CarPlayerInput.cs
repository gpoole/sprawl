using System;
using InControl;
using UnityEngine;

public class CarPlayerInput : MonoBehaviour {

    public float Accelerator {
        get {
            return input.RightTrigger;
        }
    }

    public float Brakes {
        get {
            return input.LeftTrigger;
        }
    }

    public float Turning {
        get {
            return input.LeftStickX;
        }
    }

    public bool IsHandbraking {
        get {
            return input.Action2; // b button
        }
    }

    public bool IsResetting {
        get {
            return input.GetControl(InputControlType.Back) || Input.GetKey(KeyCode.R);
        }
    }

    private InputDevice input {
        get {
            return GameManager.Instance.players[car.playerId].device;
        }
    }

    private CarController car;

    void Awake() {
        car = GetComponent<CarController>();
    }

}