using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInformation : NetworkBehaviour
{
    public int LocalPlayerNumber;

    public PlayerInput Input;
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        Input.enabled = true;
    }
}
