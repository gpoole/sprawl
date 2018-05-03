﻿using System;
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

    void Start() {
        foreach (var device in InputManager.Devices) {
            WatchDeviceForJoin(device);
        }

        WaitForStart();
    }

    void WaitForStart() {
        confirmLabel.SetActive(false);
        var confirmStream = playerSelections
            .ObserveAdd()
            .Select(ev => ev.Value)
            .SelectMany(playerSelection => {
                var playerRemoved = playerSelections.ObserveRemove().Where(ev => ev.Value == playerSelection);
                return playerSelection.confirmed
                    .TakeUntil(playerRemoved)
                    .Select(_ => Unit.Default);
            })
            .Merge(playerSelections.ObserveRemove().Select(_ => Unit.Default))
            .Select(_ => playerSelections.All(playerSelection => playerSelection.confirmed.Value) && playerSelections.Count > 0);

        confirmStream.Subscribe(confirmLabel.SetActive).AddTo(this);

        confirmStream
            .Where(allConfirmed => allConfirmed == true)
            .SelectMany(_ =>
                Observable
                .EveryUpdate()
                .Where(__ => controllerActions.Any(actions => actions.ok))
                .TakeUntil(confirmStream.Where(allConfirmed => allConfirmed == false))
            ).Subscribe(_ => {
                Debug.Log("Ready to start");
            })
            .AddTo(this);
    }

    IObservable<bool> OnButton(PlayerAction action) {
        return Observable.EveryUpdate()
            .Select(_ => !!action)
            .DistinctUntilChanged()
            .Where(pressed => pressed);
    }

    int GetSelectedIndex(PlayerSelection playerSelection) {
        var characters = characterSet.characters;
        return Array.IndexOf(characters.ToArray(), playerSelection.character.Value);
    }

    void SetSelectedIndex(PlayerSelection playerSelection, int newIndex) {
        if (newIndex < 0) {
            playerSelection.character.Value = characterSet.characters.First();
        } else if (newIndex >= characterSet.characters.Count()) {
            playerSelection.character.Value = characterSet.characters.Last();
        }
    }

    void ListenForSelection(PlayerSelection playerSelection, MenuActions controller) {
        var playerRemoved = playerSelections.ObserveRemove().Where(ev => ev.Value == playerSelection);

        OnButton(controller.left)
            .Where(_ => !playerSelection.confirmed.Value)
            .TakeUntil(playerRemoved)
            .Subscribe(_ => {
                SetSelectedIndex(playerSelection, GetSelectedIndex(playerSelection) - 1);
            })
            .AddTo(this);

        OnButton(controller.right)
            .Where(_ => !playerSelection.confirmed.Value)
            .TakeUntil(playerRemoved)
            .Subscribe(_ => {
                SetSelectedIndex(playerSelection, GetSelectedIndex(playerSelection) + 1);
            }).AddTo(this);

        OnButton(controller.up)
            .Where(_ => !playerSelection.confirmed.Value)
            .TakeUntil(playerRemoved)
            .Subscribe(_ => {
                SetSelectedIndex(playerSelection, GetSelectedIndex(playerSelection) - Columns);
            }).AddTo(this);

        OnButton(controller.down)
            .Where(_ => !playerSelection.confirmed.Value)
            .TakeUntil(playerRemoved)
            .Subscribe(_ => {
                SetSelectedIndex(playerSelection, GetSelectedIndex(playerSelection) + Columns);
            }).AddTo(this);

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
                    // Remove the player...
                    GameManager.Instance.players.Remove(playerSelection.player);
                    playerSelections.Remove(playerSelection);
                    controllerActions.Remove(controller);
                    controller.Destroy();
                    WatchDeviceForJoin(controller.Device);
                }
            });
    }

    void WatchDeviceForJoin(InputDevice device) {
        var playerActions = new MenuActions { Device = device };

        OnButton(playerActions.ok)
            .Take(1)
            .Subscribe(_ => {
                var newPlayer = GameManager.Instance.AddPlayer(device);
                var playerSelection = new PlayerSelection(newPlayer, characterSet.characters.First());
                playerSelections.Add(playerSelection);
                controllerActions.Add(playerActions);
                ListenForSelection(playerSelection, playerActions);
            })
            .AddTo(this);
    }

}