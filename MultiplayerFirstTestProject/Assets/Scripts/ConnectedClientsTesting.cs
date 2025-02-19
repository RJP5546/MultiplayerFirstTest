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
        public Character player;

        private void Update()
        {
            if (!shouldRun) { return; }

            /*
            var spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
            //set as character instance so it is a network object, and assign a client ID so the server knows which client owns and controls it
            var characterInstance = Instantiate(player.GameplayPrefab, spawnPos, Quaternion.identity);
            characterInstance.SpawnAsPlayerObject(NetworkManager.Singleton.LocalClientId);
            shouldRun = false;
            */

            UpdateLobbyOptions updateOptions = new UpdateLobbyOptions()
            {
                MaxPlayers = 3
            };

            LobbyService.Instance.UpdateLobbyAsync(HostManager.Instance.lobbyId, updateOptions);
        }
    }

}
