using UnityEngine;

[CreateAssetMenu]
public class GameCharacter : ScriptableObject {
    public string characterName;

    public GameObject carPrefab;

    public Material carSkin;

    public Sprite image;
}