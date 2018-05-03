using InControl;
using UniRx;

public class MenuControls {

    public enum Action {
        Up,
        Down,
        Left,
        Right,
        Ok,
        Back
    }

    private MenuActions actions;

    private IObservable<Action> allControls;

    private IObservable<Action> navigationControls;

    private IObservable<Action> directionalControls;

    public MenuControls(MenuActions actions) {
        this.actions = actions;
    }

    private static IObservable<Action> MapToAction(PlayerAction action, Action direction) {
        return Observable.EveryUpdate()
            .Select(_ => !!action)
            .DistinctUntilChanged()
            .Where(activated => activated)
            .Select(_ => direction);
    }

    public IObservable<Action> AllControls() {
        if (allControls == null) {
            allControls = Observable.Merge(
                NavigationControls(),
                DirectionalControls()
            );
        }
        return allControls;
    }

    public IObservable<Action> NavigationControls() {
        if (navigationControls == null) {
            navigationControls = Observable.Merge(
                MapToAction(actions.ok, Action.Ok),
                MapToAction(actions.back, Action.Back)
            );
        }
        return navigationControls;
    }

    public IObservable<Action> DirectionalControls() {
        if (directionalControls == null) {
            directionalControls = Observable.Merge(
                MapToAction(actions.left, Action.Left),
                MapToAction(actions.right, Action.Right),
                MapToAction(actions.up, Action.Up),
                MapToAction(actions.down, Action.Down)
            );
        }
        return directionalControls;
    }

}