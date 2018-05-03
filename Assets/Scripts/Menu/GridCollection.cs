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