using System;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionMarker : MonoBehaviour {

    public Player player;

    public BoolReactiveProperty confirmed;

    void Start() {
        var canvasGroup = GetComponent<CanvasGroup>();
        GetComponentInChildren<Text>().text = String.Format("P{0}", player.number);
        confirmed.Subscribe(value => {
            if (value) {
                canvasGroup.alpha = 1.0f;
            } else {
                canvasGroup.alpha = 0.5f;
            }
        }).AddTo(this);
    }

}