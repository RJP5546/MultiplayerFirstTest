using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Multiplayer;
using Unity.Multiplayer.Widgets;
using System.Linq;
using Unity.Services.Lobbies;
using System.Collections;
using System.Threading.Tasks;
using Unity.Services.Lobbies.Models;

public class HostManager : NetworkBehaviour
{
    public static HostManager Instance { get; private set; }
    [SerializeField] private string characterSelectSceneName;
    [SerializeField] private string gameplaySceneName;


    [SerializeField] private WidgetConfiguration networkWidgetConfig;
    [SerializeField] private int playersInLobbyCount;
    private int maxPlayers = 2;

    public Dictionary<ulong, ClientData> ClientData { get; private set; }

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

    public void AddClientData(ulong clientId)
    {
        if (ClientData == null) { ResetClientData(); }
        //adds the client data to the dictionary of client data, if already in, it will update. If not in it will add
        ClientData[clientId] = new ClientData(clientId);
        Debug.Log($"ClientIDLenght: {ClientData.Count}");
        Debug.Log($"ClientID: {clientId}");

    }

   public void StartHostListeners()
   {
        ResetClientData();

        //NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;

        //every time someone tries to join this server, run this method
        NetworkManager.Singleton.OnClientConnectedCallback += AddClientData;
       NetworkManager.Singleton.OnServerStarted += OnNetworkReady;
    }
    public void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        

        if (playersInLobbyCount < maxPlayers)
        {
            response.Approved = true;
            response.CreatePlayerObject = false;
            response.Pending = false;
            playersInLobbyCount += 1;
        }
        else
        {
            response.Approved = false;
            response.Pending = false;
        }

        Debug.Log($"connection approval response: {response.Approved} for Id: {request.ClientNetworkId}");
    }

    /*
   public void StartHost()
   {
       //start listening for connection approvals
       NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
       NetworkManager.Singleton.OnServerStarted += OnNetworkReady;

       //resets the dictionary of client data
       ClientData = new Dictionary<ulong, ClientData>();

       NetworkManager.Singleton.StartHost();
   }

   private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest _request, NetworkManager.ConnectionApprovalResponse _response)
   {
       //if the limit on players have joined or the game has started, deny approval
       if (ClientData.Count >= 4 || hasGameStarted)
       {
           _response.Approved = false;
           return;
       }

       _response.Approved = true;
       _response.CreatePlayerObject = false;
       _response.Pending = false;

       //adds the client data to the dictionary of client data, if already in, it will update. If not in it will add
       ClientData[_request.ClientNetworkId] = new ClientData(_request.ClientNetworkId);

       Debug.Log($"Added ClientId: {_request.ClientNetworkId}");
   }
*/

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
       if (ClientData.ContainsKey(_clientID))
       {
           if (ClientData.Remove(_clientID))
           {
               Debug.Log($"Removed ClientId: {_clientID}");
           }
       }
   }

    public void ResetClientData()
    {
        //resets the dictionary of client data
        ClientData = new Dictionary<ulong, ClientData>();
    }

    //Tell the server manager which client is which player
    public void SetCharacter(ulong _clientID, int _characterId)
    {
        
        if (ClientData.TryGetValue(_clientID, out ClientData data))
        {
            data.characterId = _characterId;
        }
    }

    //start the game
    public void StartGame()
    {
        hasGameStarted = true;
        NetworkManager.Singleton.SceneManager.LoadScene(gameplaySceneName, LoadSceneMode.Single);
    }
}
