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

    public float startDelay = 0.5f;

    private MenuController controllerActions;

    void OnEnable() {
        Setup();
    }

    void OnDisable() {
        Teardown();
    }

    void OnDestroy() {
        Teardown();
    }

    void Setup() {
        controllerActions = new MenuController();
        WatchForInput();
    }

    void Teardown() {
        if (controllerActions != null) {
            controllerActions.Destroy();
            controllerActions = null;
        }
    }

    void WatchForInput() {
        MapControllerAction(controllerActions.up, InputAction.Up);
        MapControllerAction(controllerActions.left, InputAction.Left);
        MapControllerAction(controllerActions.right, InputAction.Right);
        MapControllerAction(controllerActions.down, InputAction.Down);
        MapControllerAction(controllerActions.ok, InputAction.Ok);
        MapControllerAction(controllerActions.back, InputAction.Back);
    }

    void MapControllerAction(PlayerAction playerAction, InputAction inputAction) {
        StartCoroutine(WatchForAction(playerAction, inputAction));
    }

    IEnumerator WatchForAction(PlayerAction playerAction, InputAction inputAction) {
        // Wait for a short time to prevent cross-screen input
        yield return new WaitForSeconds(startDelay);
        // Ignore the button being held down when we start
        yield return new WaitWhile(() => playerAction.IsPressed);
        while (true) {
            yield return new WaitUntil(() => playerAction.IsPressed);
            SendInputAction(inputAction, playerAction.Device);
            yield return new WaitWhile(() => playerAction.IsPressed);
        }
    }

    void SendInputAction(InputAction action, InputDevice device) {
        ExecuteEvents.Execute<IMenuInputEventHandler>(gameObject, null, (receiver, _) => {
            Debug.LogFormat("Sending {0}", action);
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