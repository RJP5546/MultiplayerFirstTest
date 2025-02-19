using System;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterSelectDisplay : NetworkBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private Transform charactersHolder;
    [SerializeField] private CharacterSelectButton selectButtonPrefab;
    [SerializeField] private PlayerCard[] playerCards;
    [SerializeField] private GameObject characterInfoPanel;
    [SerializeField] private TMP_Text characterNameText;
    [SerializeField] private Transform introSpawnPoint;
    [SerializeField] private Button lockInButton;
    [SerializeField] private TMP_Text joinCodeText;

    private GameObject introInstance;
    private List<CharacterSelectButton> characterButtons = new List<CharacterSelectButton>();

    [SerializeField] private NetworkList<CharacterSelectState> players;


    private void Awake()
    {
        players = new NetworkList<CharacterSelectState>();
    }

    public override void OnNetworkSpawn()
    {
        if(IsClient)
        {
            Character[] allCharacters = characterDatabase.GetAllCharacters();

            foreach(var character in allCharacters)
            {
                var selectButtonInstance = Instantiate(selectButtonPrefab, charactersHolder);
                //assigns the character to the button
                selectButtonInstance.SetCharacter(this,character);
                //add the character button to a list of all character select buttons
                characterButtons.Add(selectButtonInstance);
            }

            players.OnListChanged += HandlePlayersStateChanged;
        }

        if (IsServer)
        {
            //Events help ensure anyone who connects to the session is added
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += HandleClientDisconnect;

            //Makes sure anyone who is already connected is added
            foreach(NetworkClient client in NetworkManager.Singleton.ConnectedClientsList)
            {
                HandleClientConnected(client.ClientId);
            }
        }

        if (IsHost)
        {
            joinCodeText.text = HostManager.Instance.JoinCode;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {
            players.OnListChanged -= HandlePlayersStateChanged;
        }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= HandleClientDisconnect;
        }
    }

    private void HandleClientConnected(ulong clientID)
    {
        players.Add(new CharacterSelectState(clientID));
        Debug.Log($"Connected client ID: {clientID}");
    }

    private void HandleClientDisconnect(ulong clientID)
    {
        //finds the index of the client ID and removes it from the list
        for(int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId == clientID)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    public void Select(Character character)
    {
        //check if the player is locked in, or already has that character selected. If so, return and do nothing
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].ClientId != NetworkManager.Singleton.LocalClientId) { continue; }

            if (players[i].IsLockedIn) { return; }

            if (players[i].CharacterId == character.Id) { return; }
        }


        characterNameText.text = character.DisplayName;
        characterInfoPanel.SetActive(true);

        if(introInstance != null)
        {
            //if there is already an introCharacterInstance destroy it
            Destroy(introInstance);
        }

        //set the new introInstance to the selected characters intro prefab
        introInstance = Instantiate(character.IntroPrefab, introSpawnPoint);

        //tells the server what character you want to select
        SelectServerRpc(character.Id);
    }

    //lets us parameters without anything owning the object
    [ServerRpc(RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0;i < players.Count;i++)
        {
            //find the client in the list of players
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }
            //if invalid character ID, return
            if (!characterDatabase.IsValidCharacterId(characterId)) { return; }

            //server update player info
            players[i] = new CharacterSelectState(players[i].ClientId, characterId, players[i].IsLockedIn);
        }
    }

    public void LockIn()
    {
        LockInServerRpc();
    }

    //lets us parameters without anything owning the object
    [ServerRpc(RequireOwnership = false)]
    private void LockInServerRpc(ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            //find the client in the list of players
            if (players[i].ClientId != serverRpcParams.Receive.SenderClientId) { continue; }
            //if invalid character ID, return
            if (!characterDatabase.IsValidCharacterId(players[i].CharacterId)) { return; }

            //server update player info
            players[i] = new CharacterSelectState(players[i].ClientId, players[i].CharacterId, true);
        }

        foreach (var player in players)
        {
            
            if (!player.IsLockedIn) { return; }
        }

        //if everyone is locked in, set their characters
        foreach(var player in players)
        {
            HostManager.Instance.SetCharacter(player.ClientId, player.CharacterId);
        }

        //If everyone is locked in, start the gameplay scene
        HostManager.Instance.StartGame();

    }


    private void HandlePlayersStateChanged(NetworkListEvent<CharacterSelectState> changeEvent)
    {
        //if a player joins, leaves, changes character, etc
        for (int i = 0; i < playerCards.Length; i++) 
        {
            if(players.Count > i)
            {
                //updates the cards for number of players in the game
                playerCards[i].UpdateDisplay(players[i]);
            }
            else
            {
                //disables the cards if there arent enough players
                playerCards[i].DisableDisplay();
            }
        }

        //enable or disable lockIn Button
        foreach (var player in players)
        {
            if (player.ClientId != NetworkManager.Singleton.LocalClientId) { continue; }

            if (player.IsLockedIn)
            {
                lockInButton.interactable = false;
                break;
            }

            lockInButton.interactable = true;

            break;
        }
    }

    public void CopySessionCodeToClipboard()
    {
        // Deselect the button when clicked.
        EventSystem.current.SetSelectedGameObject(null);

        var code = joinCodeText.text;

        if (HostManager.Instance.JoinCode == null || string.IsNullOrEmpty(code))
        {
            return;
        }

        // Copy the text to the clipboard.
        GUIUtility.systemCopyBuffer = code;
    }
}
