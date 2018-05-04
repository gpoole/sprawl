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

    public HideShowAnimation confirmPrompt;

    public ReactiveCollection<PlayerSelection> playerSelections = new ReactiveCollection<PlayerSelection>();

    private List<MenuController> controllerActions = new List<MenuController>();

    private GridCollection<GameCharacter> characterGrid;

    private MenuScreenManager menuScreenManager;

    void Start() {
        menuScreenManager = GetComponentInParent<MenuScreenManager>();
        characterGrid = new GridCollection<GameCharacter>(characterSet.characters, Columns);

        playerSelections.ObserveAdd()
            .Select(ev => ev.Value)
            .SelectMany(playerSelection => {
                var playerRemoved = playerSelections.ObserveRemove().Where(ev => ev.Value == playerSelection);
                return playerSelection.confirmed.TakeUntil(playerRemoved).Select(_ => Unit.Default);
            })
            .Merge(playerSelections.ObserveRemove().Select(_ => Unit.Default))
            .Select(_ => AllPlayersReady())
            .Subscribe(allReady => {
                if (allReady) {
                    confirmPrompt.Show();
                } else {
                    confirmPrompt.Hide();
                }
            })
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
            menuScreenManager.GoTo("TrackSelect");
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

        if (assignedPlayer.confirmed.Value) {
            assignedPlayer.confirmed.Value = false;
        } else {
            GameManager.Instance.RemovePlayer(assignedPlayer.player);
            playerSelections.Remove(assignedPlayer);
        }

    }

    bool AllPlayersReady() {
        return playerSelections.All(playerSelection => playerSelection.confirmed.Value) && playerSelections.Count > 0;
    }

    PlayerSelection GetSelectionForDevice(InputDevice input) {
        return playerSelections.FirstOrDefault(playerSelection => playerSelection.player.device == input);
    }

}