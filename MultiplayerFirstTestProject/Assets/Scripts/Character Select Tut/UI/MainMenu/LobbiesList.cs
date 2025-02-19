using System.Collections;
using System.Collections.Generic;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class LobbiesList : MonoBehaviour
{
    [SerializeField] private LobbyItem lobbyItemPrefab;
    [SerializeField] private Transform lobbyItemParent;

    private bool isRefreshing;
    private bool isJoining;
    private void OnEnable()
    {
        RefreshList();
    }

    public async void RefreshList()
    {
        //prevents too many calls causing rate limiting
        if (isRefreshing) { return; }
        isRefreshing = true;

        //Query the lobby service to return the lobbies we want to see
        try
        {
            var options = new QueryLobbiesOptions();
            options.Count = 25; // the amount of lobbies to return
            options.Filters = new List<QueryFilter>()
            {
                new QueryFilter(
                    //Filters lobbies by avalible player slots GT(greater than) 0
                    field: QueryFilter.FieldOptions.AvailableSlots,
                    op: QueryFilter.OpOptions.GT,
                    value: "0"),
                new QueryFilter(
                    //Filters lobbies by if they are locked EQ(equal to) 0 (locked)
                    field: QueryFilter.FieldOptions.IsLocked,
                    op: QueryFilter.OpOptions.EQ,
                    value: "0")
            };

            var lobbies = await LobbyService.Instance.QueryLobbiesAsync(options);

            //clear out the old lobby list prefabs
            foreach (Transform child in lobbyItemParent)
            {
                Destroy(child.gameObject);
            }
            //create the new ones
            foreach (Lobby lobby in lobbies.Results)
            {
                var lobbyInstance = Instantiate(lobbyItemPrefab, lobbyItemParent);
                lobbyInstance.Initialise(this, lobby);
            }

        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
            isRefreshing = false;
            throw;
        }

        isRefreshing = false;
    }

    public async void JoinAsync(Lobby _lobby)
    {
        if(isJoining) { return; }

        isJoining = true;

        try
        {
            var joiningLobby = await LobbyService.Instance.JoinLobbyByIdAsync(_lobby.Id);
            string joinCode = joiningLobby.Data["JoinCode"].Value;
            Debug.Log($"joing lobby id: {_lobby.Id}");
            await ClientManager.Instance.StartClient(joinCode);
        }
        catch(LobbyServiceException e)
        {
            Debug.Log(e);
            isJoining=false;
            throw;
        }
        isJoining = false;
    }
}
