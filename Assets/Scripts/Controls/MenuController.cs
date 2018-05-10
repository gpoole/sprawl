using InControl;

public class MenuController : PlayerActionSet {

    public PlayerAction up;

    public PlayerAction down;

    public PlayerAction left;

    public PlayerAction right;

    public PlayerAction ok;

    public PlayerAction back;

    public MenuController() {
        up = CreatePlayerAction("Up");
        down = CreatePlayerAction("Down");
        left = CreatePlayerAction("Left");
        right = CreatePlayerAction("Right");
        ok = CreatePlayerAction("Ok");
        back = CreatePlayerAction("Back");

        up.AddDefaultBinding(InputControlType.DPadUp);
        up.AddDefaultBinding(InputControlType.LeftStickUp);
        down.AddDefaultBinding(InputControlType.DPadDown);
        down.AddDefaultBinding(InputControlType.LeftStickDown);
        left.AddDefaultBinding(InputControlType.DPadLeft);
        left.AddDefaultBinding(InputControlType.LeftStickLeft);
        right.AddDefaultBinding(InputControlType.DPadRight);
        right.AddDefaultBinding(InputControlType.LeftStickRight);
        ok.AddDefaultBinding(InputControlType.Start);
        ok.AddDefaultBinding(InputControlType.Action1);
        back.AddDefaultBinding(InputControlType.Back);
        back.AddDefaultBinding(InputControlType.Action2);

        up.AddDefaultBinding(Key.W);
        up.AddDefaultBinding(Key.UpArrow);
        down.AddDefaultBinding(Key.S);
        down.AddDefaultBinding(Key.DownArrow);
        left.AddDefaultBinding(Key.A);
        left.AddDefaultBinding(Key.LeftArrow);
        right.AddDefaultBinding(Key.D);
        right.AddDefaultBinding(Key.RightArrow);
        ok.AddDefaultBinding(Key.Space);
        ok.AddDefaultBinding(Key.Return);
        back.AddDefaultBinding(Key.Backspace);
        back.AddDefaultBinding(Key.Delete);
        back.AddDefaultBinding(Key.Escape);
    }

}