using System;
using Unity.Netcode;

[Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public ulong ClientId;
    public int LocalPlayerNumber;
    public int CharacterId;

    public PlayerData(ulong clientId, int localPlayerNumber, int characterId = -1)
    {
        ClientId = clientId;
        LocalPlayerNumber = localPlayerNumber;
        CharacterId = characterId;
    }
    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref ClientId);
        serializer.SerializeValue(ref LocalPlayerNumber);
        serializer.SerializeValue(ref CharacterId);
    }

    public bool Equals(PlayerData other)
    {
        return ClientId == other.ClientId &&
            LocalPlayerNumber == other.LocalPlayerNumber &&
            CharacterId == other.CharacterId;
    }


}