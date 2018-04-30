using InControl;

public class DrivingPlayerActions : PlayerActionSet {

    public PlayerAction accelerate;

    public PlayerAction brake;

    public PlayerOneAxisAction steer;

    public PlayerAction steerLeft;

    public PlayerAction steerRight;

    public PlayerAction handbrake;

    public PlayerAction resetCar;

    public DrivingPlayerActions() {
        accelerate = CreatePlayerAction("Accelerate");
        brake = CreatePlayerAction("Brake");
        steerLeft = CreatePlayerAction("Steer left");
        steerRight = CreatePlayerAction("Steer right");
        steer = CreateOneAxisPlayerAction(steerLeft, steerRight);
        handbrake = CreatePlayerAction("Handbrake");
        resetCar = CreatePlayerAction("Reset car");
    }

}