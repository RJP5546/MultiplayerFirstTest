using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Multiplayer.Widgets;
using Unity.Services.Lobbies;
using System.Linq;

public class HostManager : NetworkBehaviour
{
    public static HostManager Instance { get; private set; }
    [SerializeField] private string characterSelectSceneName;
    [SerializeField] private string gameplaySceneName;


    [SerializeField] private WidgetConfiguration networkWidgetConfig;
    [SerializeField] private int playersInLobbyCount;
    [SerializeField] private int maxPlayers = 2;
    public NetworkList<PlayerData> PlayerDataList { get; private set; }

    public string LobbyId;
    private bool hasGameStarted;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(gameObject);
        networkWidgetConfig.MaxPlayers = maxPlayers;
    }

    public void StartHostListeners()
    {
        //run on start host being clicked on main menu UI
        ResetPlayerData();

        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

        PlayerDataList.OnListChanged += PlayerDataListChanged;

        //every time someone tries to join this server, run this method
        NetworkManager.Singleton.OnClientConnectedCallback += AddClient;
        NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
    }

    private void PlayerDataListChanged(NetworkListEvent<PlayerData> changeEvent)
    {
        foreach (var player in PlayerDataList)
        {
            Debug.Log($"In List: ClientID:{player.ClientId} LocalPlayerNum:{player.LocalPlayerNumber}");
        }
    }

    public void ResetPlayerData()
    {
        //resets the dictionary of client data
        PlayerDataList = new NetworkList<PlayerData>();
    }

    public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {

        if (playersInLobbyCount < maxPlayers)
        {
            response.Approved = true;
            response.CreatePlayerObject = false;
            response.Pending = false;
            response.CreatePlayerObject = true;
            playersInLobbyCount += 1;
        }
        else
        {
            response.Approved = false;
            response.Pending = false;
        }

    }


    public void AddClient(ulong _clientId)
    {
        Debug.Log($"Client {_clientId} connected");
        /*
        Debug.Log("Add Client Called");

        //if the player is the first to join, reset and add their data
        if (PlayerDataList.Count == 0)
        {
            PlayerDataList.Add(new PlayerDataList(_clientId, 0));
            Debug.Log($"PlayerDataList Contains client:{PlayerDataList[0].ClientId} LocalPlayer num: {PlayerDataList[0].LocalPlayerNumber}");
            return;
        }
        Debug.Log("PlayerDataList is not null");
        //adds the client data to the list of PlayerDataList, if already in, it will update. If not in it will add. Clients are added as the first local player
        for (int i =0; i < PlayerDataList.Count; i++)
        {
            //if the player id is already connected, update the info, if they have any local players on record, remove them
            if (PlayerDataList[i].ClientId == _clientId && PlayerDataList[i].LocalPlayerNumber == 0)
            {
                PlayerDataList[i] = new PlayerDataList(_clientId, 0);
            }
            else if (PlayerDataList[i].ClientId == _clientId && PlayerDataList[i].LocalPlayerNumber != 0)
            {
                PlayerDataList.Remove(PlayerDataList[i]);
            }
            else { PlayerDataList.Add(new PlayerDataList(_clientId, 0)); }
        }

        //Debug for testing to see who is in the list
        playerDataDebugInfo = PlayerDataList.ToArray();
        foreach (var player in playerDataDebugInfo)
        {
            Debug.Log($"PlayerDataList Contains client:{player.ClientId} LocalPlayer num: {player.LocalPlayerNumber}");
        }
        */
    }

    public void AddClientLocalPlayer(ulong _clientId, int _localPlayerNumber)
    {
        if (IsClient) { Debug.Log("Client called addClientPLayer"); }
        if(IsServer) { Debug.Log("Server called addClientPLayer"); }

        bool isInList = false;
        int indexInList = 0;

        //adds the client data to the list of PlayerDataList, if already in, it will update. If not in it will add. Clients are added as the first local player
        for (int i = 0; i < PlayerDataList.Count; i++)
        {
            //if the player id is already connected, update the info, if they have any local players on record, remove them
            if (PlayerDataList[i].ClientId == _clientId && PlayerDataList[i].LocalPlayerNumber == _localPlayerNumber)
            {
                isInList = true;
                indexInList = i;
            }
            else { continue; }
        }

        if (isInList) PlayerDataList[indexInList] = new PlayerData(_clientId, _localPlayerNumber);
        else PlayerDataList.Add(new PlayerData(_clientId, _localPlayerNumber));

        //Debug for testing to see who is in the list
        foreach (var player in PlayerDataList)
        {
            Debug.Log($"PlayerData Contains client:{player.ClientId} LocalPlayer num: {player.LocalPlayerNumber}");
        }
    }


    private async void OnNetworkReady()
   {
       NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
       NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);
        
        try
        {
            var joinedLobbies = await LobbyService.Instance.GetJoinedLobbiesAsync();
            LobbyId = joinedLobbies[0];
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
        Debug.Log("Current lobby ID: " + LobbyId);
    }

    private void OnClientDisconnect(ulong _clientID)
   {
        //removes a client and all its players from the player list
        foreach (PlayerData playerData in PlayerDataList)
        {
            if (playerData.ClientId == _clientID)
            {
                PlayerDataList.Remove(playerData);
                Debug.Log($"Removed ClientId: {_clientID}");
            }
        }

        //Debug for testing to see who is in the list
        foreach (var player in PlayerDataList)
        {
            Debug.Log($"PlayerData Contains client:{player.ClientId} LocalPlayer num: {player.LocalPlayerNumber}");
        }
    }



    //Tell the server manager which client is which player
    public void SetCharacter(ulong _clientID, int _characterId)
    {
        /*
        if (PlayerDataList.TryGetValue(_clientID, out PlayerDataList data))
        {
            data.CharacterId = _characterId;
        }
        */
    }

    //start the game
    public void StartGame()
    {
        hasGameStarted = true;
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
}
