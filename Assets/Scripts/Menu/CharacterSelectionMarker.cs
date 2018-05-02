using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectionMarker : MonoBehaviour {

    public Player player;

    void Start() {
        GetComponentInChildren<Text>().text = String.Format("P{0}", player.number);
    }

}