using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

public enum GridDirection {
    Up,
    Down,
    Left,
    Right
}

public class GridCollectionUtils {
    public static GridDirection DirectionFromMenuAction(InputAction action) {
        switch (action) {
            case InputAction.Up:
                return GridDirection.Up;
            case InputAction.Down:
                return GridDirection.Down;
            case InputAction.Left:
                return GridDirection.Left;
            case InputAction.Right:
                return GridDirection.Right;
        }
        throw new ArgumentException(String.Format("No direction maps to input action {0}", action));
    }
}

public class GridCollection<T> : Collection<T> {

    private int columns;

    public GridCollection(IList<T> collection, int columns) : base(collection) {
        this.columns = columns;
    }

    public T GetFrom(T start, GridDirection direction, int distance = 1) {
        var index = IndexOf(start);
        if (direction == GridDirection.Left) {
            index -= distance;
        } else if (direction == GridDirection.Right) {
            index += distance;
        } else if (direction == GridDirection.Down) {
            index += columns * distance;
        } else if (direction == GridDirection.Up) {
            index -= columns * distance;
        }

        if (index < 0) {
            return this.First();
        } else if (index >= Count) {
            return this.Last();
        }

        return this [index];
    }

}