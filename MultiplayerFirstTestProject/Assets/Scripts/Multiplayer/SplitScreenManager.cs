using UnityEngine;
using System.Collections.Generic ;
using UnityEngine.InputSystem;
using Unity.Netcode;
using System.Collections;

public class SplitScreenManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerinputManager;
    [SerializeField] private List<PlayerInput> localPlayers;
    [SerializeField] private NetworkObject localPlayerObject;
    private ulong localClientID;

    private void Awake()
    {
        localPlayers = new List<PlayerInput>();
        playerinputManager = PlayerInputManager.instance;
        
        //if the server, log the host player into the system, prevents it from being skipped
        if (NetworkManager.Singleton.IsServer)
        {
            //Add host player
            localPlayerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();
            PlayerJoined(localPlayerObject.GetComponent<PlayerInput>());
            Debug.Log("Add host to playerList");
        }
        localClientID = NetworkManager.Singleton.LocalClientId;
    }


    void OnEnable()
    {
        playerinputManager.onPlayerJoined += PlayerJoined;
        playerinputManager.onPlayerLeft += PlayerLeft;
    }

    void OnDisable()
    {
        playerinputManager.onPlayerJoined -= PlayerJoined;
        playerinputManager.onPlayerLeft -= PlayerLeft;
    }

    private void PlayerJoined(PlayerInput playerInput)
    {
        //check if the lobby is full
        if (HostManager.Instance.IsLobbyFull())
        {
            Debug.Log("Lobby full, split screen joining disabled");
            playerinputManager.DisableJoining();
            Destroy(playerInput.gameObject);
            return;
        }

        //spawn the network object
        //if (NetworkManager.Singleton.IsClient && !playerInput.GetComponent<NetworkObject>().IsSpawned) { HostManager.Instance.SpawnClientLocalPlayerNetworkObject(playerInput.GetComponent<NetworkObject>(), localClientID); }

        StartCoroutine(AddPlayerToServerPlayerList(playerInput));       
    }

    private IEnumerator AddPlayerToServerPlayerList(PlayerInput playerInput)
    {
        Debug.Log("PlayerJoinedRun");
        while (!playerInput.GetComponent<NetworkObject>().IsSpawned)
        {
            yield return null;
        }
        //Debug.Log("WhileLoopDone");
        //once the player has spawned on the network
        ulong playerInputOwnerClientId = playerInput.GetComponent<NetworkObject>().OwnerClientId;
        //Debug.Log($"deos player client id match client id: {playerInputOwnerClientId == localClientID}");

        //if the player belongs to the local client add them to the local client list
        if (playerInputOwnerClientId == localClientID)
        {
            localPlayers.Add(playerInput);
            playerInput.GetComponent<PlayerInformation>().LocalPlayerNumber = localPlayers.Count;
        }

        //If the server, update the network player list
        if (NetworkManager.Singleton.IsServer)
        {
            //HostManager.Instance.AddClientLocalPlayer(playerInputOwnerClientId, playerInput.GetComponent<PlayerInformation>().LocalPlayerNumber);
        }

        //UpdateSplitScreenCameras();
    }

    private void PlayerLeft(PlayerInput playerInput)
    {
        ulong playerInputOwnerClientId = playerInput.GetComponent<NetworkObject>().OwnerClientId;


        //if the player belongs to the local client add them to the local client list
        if (playerInputOwnerClientId == localClientID)
        {
            localPlayers.Remove(playerInput);
        }

        //add server remove player option
        
        //UpdateSplitScreenCameras();
    }

    private void UpdateSplitScreenCameras()
    {
        switch (localPlayers.Count)
        {
            case 1:
                localPlayers[0].GetComponentInChildren<Camera>().rect = new Rect(0, 0, 1, 1);
                return;
            case 2:
                localPlayers[0].GetComponentInChildren<Camera>().rect = new Rect(0, 0.5f, 1, 0.5f);
                localPlayers[1].GetComponentInChildren<Camera>().rect = new Rect(0, 0, 1, 0.5f);
                return;
        }
    }
}
