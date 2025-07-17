using System;
using TMPro;
using Unity.Multiplayer.Widgets;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowJoinCode : NetworkBehaviour
{
    const string noCode = "–";

    ISession session;


    [SerializeField]
    TMP_Text m_Text;
    [SerializeField]
    Button m_CopyCodeButton;

    void Start()
    {
        if (m_Text == null)
            m_Text = GetComponentInChildren<TMP_Text>();
        if (m_CopyCodeButton == null)
            m_CopyCodeButton = GetComponentInChildren<Button>();

        m_CopyCodeButton.onClick.AddListener(CopySessionCodeToClipboard);

        session = SessionManagerDA.Instance.ActiveSession;

        if(session != null)
        {
            m_Text.text = session?.Code ?? noCode;
            m_CopyCodeButton.interactable = true;
        }
        else
        {
            m_Text.text = noCode;
            m_CopyCodeButton.interactable = false;
        }

    }

    void CopySessionCodeToClipboard()
    {
        // Deselect the button when clicked.
        EventSystem.current.SetSelectedGameObject(null);

        var code = m_Text.text;

        if (session?.Code == null || string.IsNullOrEmpty(code))
        {
            return;
        }

        // Copy the text to the clipboard.
        GUIUtility.systemCopyBuffer = code;
    }
}
