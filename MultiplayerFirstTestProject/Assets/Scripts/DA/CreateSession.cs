using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CreateSession : MonoBehaviour
{
    [SerializeField] TMP_InputField playerName;
    [SerializeField] TMP_InputField lobbyName;
    [SerializeField] Button createButton;
    [SerializeField] Toggle isPrivate;

    private void OnEnable()
    {
        lobbyName.onValueChanged.AddListener(value =>
        {
            createButton.interactable = !string.IsNullOrEmpty(value) && SessionManagerDA.Instance.ActiveSession == null;
        });
    }

    public void EnterSession()
    {
        if (lobbyName.text != null)
        {
            SessionManagerDA.Instance.CreateNewSession(playerName.text,lobbyName.text, isPrivate);
        }
        
    }

}
