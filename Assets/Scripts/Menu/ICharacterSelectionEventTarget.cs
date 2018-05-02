using UnityEngine.EventSystems;

public interface ICharacterSelectionEventTarget : IEventSystemHandler {

    void OnPlayerCharacterChanged(Player player, GameCharacter current);

    void OnPlayerCharacterConfirmed(Player player, GameCharacter current);

    void OnPlayerCharacterUnconfirmed(Player player, GameCharacter current);

    void OnPlayerRemoved(Player player);

}