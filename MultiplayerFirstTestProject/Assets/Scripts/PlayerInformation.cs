using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInformation : NetworkBehaviour
{
    public int LocalPlayerNumber;

    public PlayerInput Input;
    public NetworkObject networkObjectComponent;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        Debug.Log("PlayerSpawned");
    }

}
