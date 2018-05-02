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

    public class CharacterSelection {
        public GameCharacter character;
        public ReactiveCollection<Player> players;
    }

    public GameCharacter[] characters;

    public ReactiveCollection<PlayerSelection> playerSelections = new ReactiveCollection<PlayerSelection>();

    void Start() {
        foreach (var device in InputManager.Devices) {
            StartCoroutine(ListenForJoin(device));
        }

        // Listen for keyboard too
        StartCoroutine(ListenForJoin(null));
    }

    IEnumerator ListenForSelection(PlayerSelection playerSelection, MenuActions controller) {
        // FIXME: wait for all confirmed condition...
        while (true) {
            if (!playerSelection.confirmed.Value && (controller.left || controller.right || controller.up || controller.down)) {
                var index = Array.IndexOf(characters.ToArray(), playerSelection.character.Value);
                if (controller.left) {
                    playerSelection.character.Value = index - 1 > 0 ? characters[index - 1] : characters.First();
                }
                if (controller.right) {
                    playerSelection.character.Value = index + 1 < characters.Length ? characters[index + 1] : characters.Last();
                }
                if (controller.down) {
                    playerSelection.character.Value = index - Columns > 0 ? characters[index - Columns] : characters.First();
                }
                if (controller.up) {
                    playerSelection.character.Value = index + Columns < characters.Length ? characters[index + Columns] : characters.Last();
                }
            }

            if (!playerSelection.confirmed.Value && controller.ok) {
                playerSelection.confirmed.Value = true;
            }

            if (playerSelection.confirmed.Value && controller.back) {
                playerSelection.confirmed.Value = false;
            }

            yield return null;
        }
    }

    IEnumerator ListenForJoin(InputDevice device) {
        if (GameManager.Instance.players.Any(player => player.device == device)) {
            yield break;
        }
        var controller = new MenuActions { Device = device };
        // FIXME: bail on screen change, or will this get destroyed automatically?
        // Do I need to destroy the controller?
        yield return new WaitUntil(() => controller.join);
        var newPlayer = GameManager.Instance.AddPlayer(device);
        var playerSelection = new PlayerSelection(newPlayer, characters.First());
        playerSelections.Add(playerSelection);
        yield return new WaitWhile(() => controller.join);
        StartCoroutine(ListenForSelection(playerSelection, controller));
    }

}