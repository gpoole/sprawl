using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CharacterSelectTile : MonoBehaviour {

    public GameCharacter character;

    public Image image;

    public Text characterName;

    void Start() {
        UpdateUI();
    }

    void OnValidate() {
        UpdateUI();
    }

    void UpdateUI() {
        if (character != null) {
            if (image != null) {
                image.sprite = character.image;
            }

            if (characterName != null) {
                characterName.text = character.characterName;
            }
        }
    }

}