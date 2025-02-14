using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HostManager : MonoBehaviour
{
    public static HostManager Instance { get; private set; }
    [SerializeField] private string characterSelectSceneName;
    [SerializeField] private string gameplaySceneName;
    public Dictionary<ulong, ClientData> ClientData { get; private set; }


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
    }
/*
    public void StartServer()
    {
        //every time someone tries to join this server, run this method
        NetworkManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkManager.Singleton.OnServerStarted += OnNetworkReady;

        //resets the dictionary of client data
        ClientData = new Dictionary<ulong, ClientData>();

        NetworkManager.Singleton.StartServer();
    }
*/
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

    private void OnNetworkReady()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnect;
        NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);
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
