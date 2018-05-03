using System;
using System.Collections;
using System.Linq;
using InControl;
using UnityEngine;
using UnityEngine.EventSystems;

public enum InputAction {
    Ok,
    Back,
    Up,
    Down,
    Left,
    Right
}

public interface IMenuInputEventHandler : IEventSystemHandler {

    void OnInputAction(InputAction action, InputDevice input);

    void OnInputOk(InputDevice input);

    void OnInputBack(InputDevice input);

}

public class MenuPlayerInput : MonoBehaviour {

    private MenuActions[] controllers;

    void Start() {
        controllers = InputManager.Devices.Select(device => new MenuActions { Device = device }).ToArray();

    }

    IEnumerator WatchForInput(MenuActions actions) {
        while (true) {
            yield return new WaitUntil(() => actions.up || actions.left || actions.right || actions.down || actions.ok || actions.back);

            if (actions.up) {
                SendInputAction(InputAction.Up, actions.ActiveDevice);
            }

            if (actions.left) {
                SendInputAction(InputAction.Down, actions.ActiveDevice);
            }

            if (actions.right) {
                SendInputAction(InputAction.Right, actions.ActiveDevice);
            }

            if (actions.down) {
                SendInputAction(InputAction.Down, actions.ActiveDevice);
            }

            if (actions.ok) {
                SendInputAction(InputAction.Ok, actions.ActiveDevice);
            }

            if (actions.back) {
                SendInputAction(InputAction.Back, actions.ActiveDevice);
            }
        }
    }

    void SendInputAction(InputAction action, InputDevice device) {
        ExecuteEvents.Execute<IMenuInputEventHandler>(gameObject, null, (receiver, _) => {
            receiver.OnInputAction(action, device);

            if (action == InputAction.Back) {
                receiver.OnInputBack(device);
            }

            if (action == InputAction.Ok) {
                receiver.OnInputOk(device);
            }
        });
    }

}