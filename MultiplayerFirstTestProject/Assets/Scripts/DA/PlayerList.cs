using System;
using System.Collections.Generic;
using Unity.Services.Multiplayer;
using UnityEngine;

public class PlayerList : MonoBehaviour
{
    
    public ISession Session { get; set; }

    /// <summary>
    /// If enabled the host will be able to kick players via the UI.
    /// </summary>
    [Header("Settings")]
    public bool HostCanKickPlayers = true;

    /// <summary>
    /// If enabled players can be muted via the UI.
    /// </summary>
    public bool PlayersCanBeMuted = true;

    /// <summary>
    /// The item that will be instantiated for each player in the session.
    /// </summary>
    [Header("References")]
    [Tooltip("The GameObject that will be instantiated for each player in the session.")]
    public GameObject ListItem;

    /// <summary>
    /// The root transform the Player List Item's will be instantiated under.
    /// </summary>
    [Tooltip("The parent transform all ListItems will be instantiated under.")]
    public Transform ContentRoot;

    Dictionary<string, PlayerListItem> m_PlayerListItems = new();

    List<PlayerListItem> m_CachedPlayerListItems = new();

    private void OnEnable()
    {
        Session = SessionManagerDA.Instance.ActiveSession;
        Session.PlayerJoined += OnPlayerJoinedSession;
        UpdatePlayerList();
    }

    private void OnDisable()
    {
        Session.PlayerJoined -= OnPlayerJoinedSession;
        DisableAllPlayerListItems();
    }

    public void OnSessionLeft()
    {
        Session = null;
        DisableAllPlayerListItems();
    }

    public void OnSessionJoined()
    {
        UpdatePlayerList();
    }

    public void OnPlayerJoinedSession(string playerID)
    {
        UpdatePlayerList();
    }

    public void OnPlayerLeftSession(string playerId)
    {
        if (m_PlayerListItems.TryGetValue(playerId, out var playerListItem))
        {
            playerListItem.Reset();
            playerListItem.gameObject.SetActive(false);
            m_CachedPlayerListItems.Add(playerListItem);
            m_PlayerListItems.Remove(playerId);
        }
    }

    void UpdatePlayerList()
    {
        if (Session == null)
            return;

        foreach (var player in Session.Players)
        {
            var playerId = player.Id;
            Debug.Log($"PlayerId: {playerId}");

            if (m_PlayerListItems.ContainsKey(playerId))
                continue;

            var playerListItem = GetPlayerListItem(playerId);
            playerListItem.gameObject.SetActive(true);

            var playerName = "Unknown";
            if (player.Properties.TryGetValue("playerName", out var playerNameProperty))
                playerName = playerNameProperty.Value;

            var configuration = new PlayerListItem.Configuration
            {
                HostCanKickPlayers = HostCanKickPlayers,
                PlayersCanBeMuted = true
                //PlayersCanBeMuted = (WidgetConfiguration?.EnableVoiceChat ?? false) && PlayersCanBeMuted,
            };

            playerListItem.Init(playerName, playerId, configuration);
        }
    }

    PlayerListItem GetPlayerListItem(string playerId)
    {

        if (m_PlayerListItems.TryGetValue(playerId, out var playerListItem))
            return playerListItem;

        playerListItem = Instantiate(ListItem, ContentRoot).GetComponent<PlayerListItem>();

        m_PlayerListItems.Add(playerId, playerListItem);
        return playerListItem;
    }

    void DisableAllPlayerListItems()
    {
        foreach (var playerListItem in m_PlayerListItems.Values)
        {
            Destroy(playerListItem.gameObject);
        }

        m_PlayerListItems.Clear();
    }

    /*
    public void OnPlayerAddedToChat(IChatParticipant participant)
    {
        if (!PlayersCanBeMuted)
            return;

        if (!m_PlayerListItems.TryGetValue(participant.Id, out var listItem))
            return;

        listItem.ChatParticipant = participant;

        listItem.SetMuteButtonInteractable(WidgetConfiguration.EnableVoiceChat);
        listItem.SetVoiceIndicatorEnabled(WidgetConfiguration.EnableVoiceChat);
    }
    */
    
}

