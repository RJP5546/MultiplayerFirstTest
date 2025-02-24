using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Multiplayer;
using UnityEngine;

public class CharacterSpawner : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] private CharacterDatabase characterDatabase;
    /*
    public override void OnNetworkSpawn()
    {

        if (!IsServer) { return; }
        Debug.Log($"clientDataSize: {HostManager.Instance.PlayerDataList.Count}");
        
        foreach (var client in HostManager.Instance.PlayerDataList)
        {
            Debug.Log("ServerSpawnChar");
            //load their character
            var character = characterDatabase.GetCharacterById(client.Value.CharacterId);
            if (character != null)
            {
                var spawnPos = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f));
                //set as character instance so it is a network object, and assign a client ID so the server knows which client owns and controls it
                var characterInstance = Instantiate(character.GameplayPrefab, spawnPos, Quaternion.identity);
                characterInstance.SpawnAsPlayerObject(client.Value.ClientId);
            }
        }

    }
    */
}