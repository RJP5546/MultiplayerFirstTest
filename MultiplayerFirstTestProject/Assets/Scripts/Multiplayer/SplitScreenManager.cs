using System;
using UnityEngine;
using System.Collections.Generic ;
using UnityEngine.InputSystem;
using Unity.Netcode;
using UnityEngine.TextCore.Text;
using UnityEditor.PackageManager;
using Unity.Template.Multiplayer.NGO.Runtime;


public class SplitScreenManager : NetworkBehaviour
{
    [SerializeField] private PlayerInputManager playerinputManager;
    [SerializeField] private List<GameObject> localPlayers;

    private void Awake()
    {
        localPlayers = new List<GameObject>();
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

    private void PlayerJoined(PlayerInput playerInput)
    {
        localPlayers.Add(playerInput.gameObject);
        UpdateSplitScreenCameras();
    }

    private void PlayerLeft(PlayerInput playerInput)
    {
        localPlayers.Remove(playerInput.gameObject);
        UpdateSplitScreenCameras();
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
