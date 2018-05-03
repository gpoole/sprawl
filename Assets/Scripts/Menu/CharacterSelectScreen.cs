using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UniRx;
using UnityEngine;

public class CharacterSelectScreen : MonoBehaviour {

    private const int Columns = 3;

    public class PlayerSelection {

        public Player player;

        public ReactiveProperty<GameCharacter> character;

        public BoolReactiveProperty confirmed = new BoolReactiveProperty(false);

        public PlayerSelection(Player player, GameCharacter character) {
            this.player = player;
            this.character = new ReactiveProperty<GameCharacter>(character);
        }

    }

    public GameCharacterList characterSet;

    public GameObject confirmLabel;

    public ReactiveCollection<PlayerSelection> playerSelections = new ReactiveCollection<PlayerSelection>();

    private List<MenuActions> controllerActions = new List<MenuActions>();

    private GridCollection<GameCharacter> characterGrid;

    private MainMenuManager mainMenuManager;

    void Start() {
        mainMenuManager = GetComponentInParent<MainMenuManager>();
        characterGrid = new GridCollection<GameCharacter>(characterSet.characters, Columns);

        var unassignedDevices = InputManager.Devices.Where(device => !GameManager.Instance.players.Any(player => player.device == device));
        foreach (var device in unassignedDevices) {
            StartCoroutine(WatchDeviceForJoin(device));
        }

        foreach (var existingPlayer in GameManager.Instance.players) {
            AddPlayerSelection(existingPlayer);
        }

        StartCoroutine(WatchForStart());
    }

    IEnumerator WatchForStart() {
        confirmLabel.SetActive(false);
        var anyPlayerInput = new MenuActions();

        while (true) {
            if (playerSelections.All(playerSelection => playerSelection.confirmed.Value) && playerSelections.Count > 0) {
                confirmLabel.SetActive(true);
                if (anyPlayerInput.ok) {
                    // FIXME: these NEED to be destroyed sometime before the game starts
                    // foreach (var controller in controllerActions) {
                    //     controller.Destroy();
                    // }
                    mainMenuManager.activeScreen.Value = MainMenuManager.Screen.TrackSelect;
                    break;
                }
            } else {
                confirmLabel.SetActive(false);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IObservable<bool> OnButton(PlayerAction action) {
        return Observable.EveryUpdate()
            .Select(_ => !!action)
            .DistinctUntilChanged()
            .Where(pressed => pressed);
    }

    void WatchForSelection(PlayerSelection playerSelection, MenuActions controller) {
        var playerRemoved = playerSelections.ObserveRemove().Where(ev => ev.Value == playerSelection);
        var gridNavigation = GridNavigator.FromMenuActions(controller);

        gridNavigation
            .Where(_ => !playerSelection.confirmed.Value)
            .TakeUntil(playerRemoved)
            .Subscribe(direction => {
                playerSelection.character.Value = characterGrid.GetFrom(playerSelection.character.Value, direction);
            })
            .AddTo(this);

        OnButton(controller.ok)
            .Where(_ => !playerSelection.confirmed.Value)
            .TakeUntil(playerRemoved)
            .Subscribe(_ => {
                playerSelection.confirmed.Value = true;
            })
            .AddTo(this);

        OnButton(controller.back)
            .TakeUntil(playerRemoved)
            .Subscribe(_ => {
                if (playerSelection.confirmed.Value) {
                    playerSelection.confirmed.Value = false;
                } else {
                    GameManager.Instance.RemovePlayer(playerSelection.player);
                    playerSelections.Remove(playerSelection);
                    controllerActions.Remove(controller);
                    controller.Destroy();
                    StartCoroutine(WatchDeviceForJoin(controller.Device));
                }
            })
            .AddTo(this);
    }

    IEnumerator WatchDeviceForJoin(InputDevice device) {
        var waitActions = new MenuActions { Device = device };
        yield return OnButton(waitActions.ok).Take(1).ToYieldInstruction();
        var newPlayer = GameManager.Instance.AddPlayer(device);
        AddPlayerSelection(newPlayer, waitActions);
        yield return new WaitUntil(() => !waitActions.ok);
    }

    void AddPlayerSelection(Player player, MenuActions actions = null) {
        if (actions == null) {
            actions = new MenuActions { Device = player.device };
        }

        var playerSelection = new PlayerSelection(player, player.character ? player.character : characterSet.characters.First());
        playerSelections.Add(playerSelection);
        controllerActions.Add(actions);
        WatchForSelection(playerSelection, actions);
    }

}