using TMPro;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbyItem : MonoBehaviour
{
    [SerializeField] private TMP_Text lobbyNameText;
    [SerializeField] private TMP_Text lobbyPlayersText;

    private LobbiesList lobbyList;
    private Lobby lobby;

    public void Initialise(LobbiesList _lobbiesList, Lobby _lobby)
    {
        this.lobbyList = _lobbiesList;
        this.lobby = _lobby;

        lobbyNameText.text = lobby.Name;
        lobbyPlayersText.text = ($"{lobby.Players.Count}/{lobby.MaxPlayers}");
    }

    private void Join()
    {
        lobbyList.JoinAsync(lobby);
    }
}
