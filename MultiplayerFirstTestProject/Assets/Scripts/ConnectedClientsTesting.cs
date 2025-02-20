using Unity.Netcode;
using Unity.Services.Lobbies;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.TextCore.Text;
namespace Unity.Multiplayer.Widgets
{
    public class ConnectedClientsTesting : NetworkBehaviour
    {
        public bool shouldRun;
        public int maxPlayers;

        private void Update()
        {
            if (!shouldRun) { return; }

            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions()
            {
                MaxPlayers = maxPlayers
            };

            LobbyService.Instance.UpdateLobbyAsync(HostManager.Instance.LobbyId, updateOptions);

            shouldRun = false;
        }
    }

}
