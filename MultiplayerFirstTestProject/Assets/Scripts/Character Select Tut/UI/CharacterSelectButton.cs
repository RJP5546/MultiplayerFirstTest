using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject disabledOverlay;
    [SerializeField] private Button button;

    private CharacterSelectDisplay characterSelect;

    public Character Character { get; private set; }
    public bool IsDisabled { get; private set; }

    public void SetCharacter(CharacterSelectDisplay _characterSelect, Character _character)
    {
        iconImage.sprite = _character.Icon;

        this.characterSelect = _characterSelect;

        Character = _character;
    }

    public void SelectCharacter()
    {
        PlayerInformation playerInfo = GetComponentInParent<PlayerInformation>();
        characterSelect.Select(Character, playerInfo.LocalPlayerNumber);
    }

    public void SetDisabled()
    {
        IsDisabled = true;
        disabledOverlay.SetActive(true);
        button.interactable = false;
    }

}
