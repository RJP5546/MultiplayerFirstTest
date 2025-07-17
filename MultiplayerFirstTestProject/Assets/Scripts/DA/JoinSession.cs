using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class JoinSession : MonoBehaviour
{
    [SerializeField] TMP_InputField playerName;
    [SerializeField] TMP_InputField lobbyCode;
    [SerializeField] Button createButton;

    private void OnEnable()
    {
        lobbyCode.onValueChanged.AddListener(value =>
        {
            createButton.interactable = !string.IsNullOrEmpty(value) && SessionManagerDA.Instance.ActiveSession == null;
        });
    }

    public void EnterSessionByCode()
    {
        if (lobbyCode.text != null)
        {
            SessionManagerDA.Instance.JoinSessionByCode(playerName.text, lobbyCode.text);
        }

    }


}
