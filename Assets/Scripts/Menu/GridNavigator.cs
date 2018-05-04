using System;
using InControl;
using UniRx;

public class GridNavigator {

    private static IObservable<GridDirection> MapToDirection(PlayerAction action, GridDirection direction) {
        return Observable.EveryUpdate()
            .Select(_ => !!action)
            .DistinctUntilChanged()
            .Where(activated => activated)
            .Select(_ => direction);
    }

    public static IObservable<GridDirection> FromMenuActions(MenuController actions) {
        return Observable.Merge(
            MapToDirection(actions.left, GridDirection.Left),
            MapToDirection(actions.right, GridDirection.Right),
            MapToDirection(actions.up, GridDirection.Up),
            MapToDirection(actions.down, GridDirection.Down)
        );
    }
}