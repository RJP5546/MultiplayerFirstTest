using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }
    [SerializeField] private string characterSelectSceneName;
    [SerializeField] private TMP_InputField lobbyJoinCodeField;

    public Dictionary<ulong, PlayerData> ClientData { get; private set; }

    ISession activeSession;

    ISession ActiveSession
    {
        get => activeSession;
        set
        {
            activeSession = value;
            Debug.Log($"Active session: {activeSession}");
        }
    }

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

    const string playerNamePropertyKey = "playerName";

    async void Start()
    {
        try
        {
            await UnityServices.InitializeAsync(); // Initialize Unity Gaming Services SDKs.
            await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Anonymously authenticate the player
            Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerId}");
            Debug.Log($"PlayerName: {AuthenticationService.Instance.PlayerName}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties()
    {
        // Custom game-specific properties that apply to an individual player, ie: name, role, skill level, etc.
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        return new Dictionary<string, PlayerProperty> { { playerNamePropertyKey, playerNameProperty } };
    }

    async void StartSessionAsHost()
    {
        var playerProperties = await GetPlayerProperties();

        var options = new SessionOptions
        {
            MaxPlayers = 2,
            IsLocked = false,
            IsPrivate = false,
            PlayerProperties = playerProperties
        }.WithRelayNetwork(); // or WithDistributedAuthorityNetwork() to use Distributed Authority instead of Relay

        ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
        Debug.Log($"Session {ActiveSession.Id} created! Join code: {ActiveSession.Code}");
        //NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);
    }

    async void JoinSessionById(string sessionId)
    {
        var playerProperties = await GetPlayerProperties();

        var joinSessionOptions = new JoinSessionOptions
        {
            PlayerProperties = playerProperties
        };

        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId, joinSessionOptions);
        Debug.Log($"Session {ActiveSession.Id} joined!");
    }

    async void JoinSessionByCode(string sessionCode)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(sessionCode);
        Debug.Log($"Session {ActiveSession.Id} joined!");
    }

    async void KickPlayer(string playerId)
    {
        if (!ActiveSession.IsHost) return;
        await ActiveSession.AsHost().RemovePlayerAsync(playerId);
    }

    async Task<IList<ISessionInfo>> QuerySessions()
    {
        var sessionQueryOptions = new QuerySessionsOptions();
        QuerySessionsResults results = await MultiplayerService.Instance.QuerySessionsAsync(sessionQueryOptions);
        return results.Sessions;
    }

    async void LeaveSession()
    {
        if (ActiveSession != null)
        {
            try
            {
                await ActiveSession.LeaveAsync();
            }
            catch
            {
                // Ignored as we are exiting the game
            }
            finally
            {
                ActiveSession = null;
            }
        }
    }

    public void StartAsHost()
    {
        StartSessionAsHost();
    }

    public void JoinViaCode()
    {
        JoinSessionByCode(lobbyJoinCodeField.text);
    }

    public void JoinViaId()
    {
        JoinSessionById(lobbyJoinCodeField.text);
    }

    public void LoadNextScene()
    {
        NetworkManager.Singleton.SceneManager.LoadScene(characterSelectSceneName, LoadSceneMode.Single);
    }

    public ISession ReturnActiveSession()
    {
        return ActiveSession;
    }
}