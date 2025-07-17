using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Multiplayer.Widgets;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SessionManagerDA : Singleton<SessionManagerDA>
{
    public ISession ActiveSession;

    ISession activeSession
    {
        get => ActiveSession;
        set
        {
            ActiveSession = value;
            Debug.Log($"Active session: {ActiveSession}");
        }
    }

    private NetworkManager networkManagerObject;

    const string playerNamePropertyKey = "playerName";

    public string sceneToLoadName;

    private void OnSessionOwnerPromoted(ulong sessionOwnerPromoted)
    {
        if (networkManagerObject.LocalClient.IsSessionOwner)
        {
            Debug.Log($"Client-{networkManagerObject.LocalClientId} is the session owner!");
            networkManagerObject.SceneManager.LoadScene(sceneToLoadName, LoadSceneMode.Single);
        }
    }

    private void OnClientConnectedCallback(ulong clientId)
    {
        if (networkManagerObject.LocalClientId == clientId)
        {
            Debug.Log($"Client-{clientId} is connected and can spawn {nameof(NetworkObject)}s.");
        }
    }

    async void Start()
    {
        networkManagerObject = NetworkManager.Singleton;
        networkManagerObject.OnClientConnectedCallback += OnClientConnectedCallback;
        networkManagerObject.OnSessionOwnerPromoted += OnSessionOwnerPromoted;
        await UnityServices.InitializeAsync();// Initialize Unity Gaming Services SDKs.
    }

    async Task<Dictionary<string, PlayerProperty>> GetPlayerProperties()
    {
        // Custom game-specific properties that apply to an individual player, ie: name, role, skill level, etc.
        var playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
        var playerNameProperty = new PlayerProperty(playerName, VisibilityPropertyOptions.Member);
        return new Dictionary<string, PlayerProperty> { { playerNamePropertyKey, playerNameProperty } };
    }

    public async void CreateNewSession(string _profileName, string _sessionName, bool _isPrivate)
    {
        try
        {
            AuthenticationService.Instance.SwitchProfile(_profileName);

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Anonymously authenticate the player
                await AuthenticationService.Instance.UpdatePlayerNameAsync(_profileName);
                await VivoxService.Instance.InitializeAsync();
                Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerName}");

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            var playerProperties = await GetPlayerProperties();

            var options = new SessionOptions()
            {
                Name = _sessionName,
                MaxPlayers = 2,
                IsLocked = false,
                IsPrivate = _isPrivate,
                PlayerProperties = playerProperties
            }.WithDistributedAuthorityNetwork();

            ActiveSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            
            if (ActiveSession.IsHost)
            {
                networkManagerObject.SceneManager.LoadScene(sceneToLoadName, LoadSceneMode.Additive);
            }
            

        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }

    public async void JoinSessionByCode(string _profileName, string _sessionCode)
    {
        try
        {
            AuthenticationService.Instance.SwitchProfile(_profileName);

            try
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync(); // Anonymously authenticate the player
                await AuthenticationService.Instance.UpdatePlayerNameAsync(_profileName);
                Debug.Log($"Sign in anonymously succeeded! PlayerID: {AuthenticationService.Instance.PlayerName}");

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            var playerProperties = await GetPlayerProperties();

            var joinOptions = new JoinSessionOptions()
            {
                PlayerProperties = playerProperties
            };

            ActiveSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(_sessionCode, joinOptions);

        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }

    }


    async void JoinSessionById(string sessionId)
    {
        ActiveSession = await MultiplayerService.Instance.JoinSessionByIdAsync(sessionId);
        Debug.Log($"Session {activeSession.Id} joined!");
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
        if (activeSession != null)
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
}
