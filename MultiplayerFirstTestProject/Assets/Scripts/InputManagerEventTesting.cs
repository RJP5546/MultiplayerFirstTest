using Unity.Burst;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManagerEventTesting : NetworkBehaviour
{
    private PlayerInputManager inputManager;

    private bool isListeningToInputManager = false;

    void OnEnable()
    {
        if (inputManager != null)
        {
            PlayerInputManager.instance.onPlayerJoined += PlayerJoined;
            isListeningToInputManager = true;
        }
        else return;
    }

    void OnDisable()
    {
        if (inputManager != null)
        {
            PlayerInputManager.instance.onPlayerJoined -= PlayerJoined;
            isListeningToInputManager = false;
        }
        else return;
    }

    private void Start()
    {
        if (inputManager == null)
        {
            inputManager = PlayerInputManager.instance;
            if (!isListeningToInputManager)
            {
                PlayerInputManager.instance.onPlayerJoined += PlayerJoined;
                isListeningToInputManager = true;
            }
        }
    }

    private void PlayerJoined(PlayerInput playerInput)
    {
        
        //Debug.Log($"Player Joined object id {playerInput.GetComponent<NetworkObject>().NetworkObjectId}");
        PlayerJoinedRpc(playerInput.GetComponent<NetworkObject>().OwnerClientId, RpcTarget.Single(playerInput.GetComponent<NetworkObject>().OwnerClientId, RpcTargetUse.Temp));

    }

    [Rpc(SendTo.SpecifiedInParams)]
    public void PlayerJoinedRpc(ulong rpcTarget, RpcParams rpcParams)
    {
        Debug.Log($"Player joined ");
        //add the code here for updating player list, maybe attach input for spawning characters later??
    }

}
