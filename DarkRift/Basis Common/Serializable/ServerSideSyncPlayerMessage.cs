using DarkRift;
public static partial class SerializableDarkRift
{
    public struct ServerSideSyncPlayerMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public LocalAvatarSyncMessage avatarSerialization;
        public void Deserialize(DeserializeEvent e)
        {
            playerIdMessage.Deserialize(e);
            avatarSerialization.Deserialize(e);
        }
        public void Serialize(SerializeEvent e)
        {
            playerIdMessage.Serialize(e);
            avatarSerialization.Serialize(e);
        }
    }
}
