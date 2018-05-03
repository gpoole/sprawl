using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InControl;
using UniRx;
using UnityEngine;

public class CharacterSelectScreen : MonoBehaviour, IMenuInputEventHandler {

    private const int Columns = 3;

    public class PlayerSelection {

        public Player player;

        public ReactiveProperty<GameCharacter> character;

        public BoolReactiveProperty confirmed;

        public PlayerSelection(Player player, GameCharacter character, bool confirmed) {
            this.player = player;
            this.character = new ReactiveProperty<GameCharacter>(character);
            this.confirmed = new BoolReactiveProperty(confirmed);
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

        confirmLabel.SetActive(false);

        playerSelections.ObserveAdd()
            .Select(ev => ev.Value)
            .SelectMany(playerSelection => {
                var playerRemoved = playerSelections.ObserveRemove().Where(ev => ev.Value == playerSelection);
                return playerSelection.confirmed.TakeUntil(playerRemoved).Select(_ => Unit.Default);
            })
            .Merge(playerSelections.ObserveRemove().Select(_ => Unit.Default))
            .Select(_ => AllPlayersReady())
            .Subscribe(confirmLabel.SetActive)
            .AddTo(this);
    }

    public void OnInputAction(InputAction action, InputDevice input) {
        var assignedPlayer = GetSelectionForDevice(input);

        if (assignedPlayer == null) {
            return;
        }

        switch (action) {
            case InputAction.Up:
            case InputAction.Down:
            case InputAction.Left:
            case InputAction.Right:
                assignedPlayer.character.Value = characterGrid.GetFrom(assignedPlayer.character.Value, GridCollectionUtils.DirectionFromMenuAction(action));
                break;
        }
    }

    public void OnInputOk(InputDevice input) {
        if (AllPlayersReady()) {
            mainMenuManager.activeScreen.Value = MainMenuManager.Screen.TrackSelect;
        } else {
            var assignedPlayer = GetSelectionForDevice(input);
            if (assignedPlayer != null) {
                // Existing player, confirm selection
                if (!assignedPlayer.confirmed.Value) {
                    assignedPlayer.confirmed.Value = true;
                }
            } else {
                // New player, add them!
                var newPlayer = GameManager.Instance.AddPlayer(input);
                var playerSelection = new PlayerSelection(newPlayer, characterSet.characters.First(), false);
                playerSelections.Add(playerSelection);
            }
        }
    }

    public void OnInputBack(InputDevice input) {
        var assignedPlayer = GetSelectionForDevice(input);
        if (assignedPlayer == null) {
            // Nobody's in here, back fully out
            Debug.LogError("Go back to main screen");
            return;
        }

        assignedPlayer.confirmed.Value = false;
    }

    bool AllPlayersReady() {
        return playerSelections.All(playerSelection => playerSelection.confirmed.Value) && playerSelections.Count > 0;
    }

    PlayerSelection GetSelectionForDevice(InputDevice input) {
        return playerSelections.FirstOrDefault(playerSelection => playerSelection.player.device == input);
    }

}