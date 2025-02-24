using System;
using UnityEngine;
using System.Collections.Generic ;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.TextCore.Text;
using UnityEditor.PackageManager;
using Unity.Template.Multiplayer.NGO.Runtime;
using System.Linq;

public class SplitScreenManager : MonoBehaviour
{
    [SerializeField] private PlayerInputManager playerinputManager;
    [SerializeField] private List<PlayerInput> localPlayers;

    private void Awake()
    {
        localPlayers = new List<PlayerInput>();
    }

    void OnEnable()
    {
        playerinputManager.onPlayerJoined += PlayerJoined;
        playerinputManager.onPlayerLeft += PlayerLeft;
    }

    void OnDisable()
    {
        PlayerInputManager.instance.onPlayerJoined -= PlayerJoined;
        PlayerInputManager.instance.onPlayerLeft -= PlayerLeft;
    }

    public void TestPLayerConnect()
    {
        Debug.Log("TestPlayerConnect");
    }

    private void PlayerJoined(PlayerInput playerInput)
    {
        ulong playerInputOwnerClientId = playerInput.GetComponent<NetworkObject>().OwnerClientId;


        //if the player belongs to the local client add them to the local client list
        if (playerInputOwnerClientId == NetworkManager.Singleton.LocalClientId)
        {
            localPlayers.Add(playerInput);
            playerInput.GetComponent<PlayerInformation>().LocalPlayerNumber = localPlayers.Count;
        }
        
        //if the server is listening to them
        if (NetworkManager.Singleton.IsServer)
        {
            HostManager.Instance.AddClientLocalPlayer(playerInputOwnerClientId, playerInput.GetComponent<PlayerInformation>().LocalPlayerNumber);
        }

        //UpdateSplitScreenCameras();
    }

    private void PlayerLeft(PlayerInput playerInput)
    {
        ulong playerInputOwnerClientId = playerInput.GetComponent<NetworkObject>().OwnerClientId;


        //if the player belongs to the local client add them to the local client list
        if (playerInputOwnerClientId == NetworkManager.Singleton.LocalClientId)
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
