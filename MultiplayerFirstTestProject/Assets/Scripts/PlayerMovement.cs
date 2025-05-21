using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : NetworkBehaviour
{
    [SerializeField]
    CharacterController m_CharacterController;

    [SerializeField]
    PlayerInput playerInput;

    [SerializeField]
    Camera playerCamera;

    [SerializeField]
    bool spawnPlayer = false;

    private void Start()
    {
        if (!GetComponent<NetworkObject>().IsSpawned)
        {
            GetComponent<NetworkObject>().Spawn();
        }
    }

    private void Update()
    {
        if (spawnPlayer) { GetComponent<NetworkObject>().Spawn(); spawnPlayer = false; }
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        enabled = IsClient;
        if (!IsOwner)
        {
            enabled = false;
            m_CharacterController.enabled = false;
            playerInput.enabled = false;
            return;
        }

        // player input is only enabled on owning players
        playerInput.enabled = true;

        // see the note inside ServerPlayerMove why this step is also necessary for synchronizing initial player
        // position on owning clients
        m_CharacterController.enabled = true;

        //playerCamera.enabled = true;

    }

    public void OnMove(InputValue value)
    {
        Vector2 inputValue = value.Get<Vector2>();
        gameObject.transform.position += new Vector3 (inputValue.x, inputValue.y, 0);

    }

}
