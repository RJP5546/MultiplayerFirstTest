using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Multiplayer.Widgets;
using Unity.Services.Lobbies;
using System.Linq;
using Unity.Services.Multiplayer;

public class HostManager : NetworkBehaviour
{
    public static HostManager Instance { get; private set; }
    [SerializeField] private string characterSelectSceneName;
    [SerializeField] private string gameplaySceneName;


    [SerializeField] private WidgetConfiguration networkWidgetConfig;
    [SerializeField] private int playersInLobbyCount = 0;
    [SerializeField] private int maxPlayers = 4;
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
            Debug.Log($"In List: ClientID:{player.ClientId}");
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
            response.Pending = false;
            response.CreatePlayerObject = true;
            //player count will be updated when a playerinput object connects to allow counitng of split screen players
        }
        else
        {
            response.Approved = false;
            response.Pending = false;
        }
    }


    private async void OnNetworkReady()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);

        if (LobbyId == "")
        {
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
            //Debug.Log("Current lobby ID: " + LobbyId);
        }
    }

    public void AddClient(ulong _clientId)
    {
        bool isInList = false;
        int indexInList = 0;

        //adds the client data to the list of PlayerDataList, if already in, it will update. If not in it will add. Clients are added as the first local player
        for (int i = 0; i < PlayerDataList.Count; i++)
        {
            //if the player id is already connected, update the info, if they have any local players on record, remove them
            if (PlayerDataList[i].ClientId == _clientId)
            {
                isInList = true;
                indexInList = i;
            }
            else { continue; }
        }

        if (isInList) PlayerDataList[indexInList] = new PlayerData(_clientId);
        else PlayerDataList.Add(new PlayerData(_clientId));

        playersInLobbyCount++;
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

        playersInLobbyCount--;

        //Debug for testing to see who is in the list
        foreach (var player in PlayerDataList)
        {
            Debug.Log($"PlayerData Contains client:{player.ClientId}");
        }
    }

    public bool IsLobbyFull()
    {
        return playersInLobbyCount >= maxPlayers;
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
