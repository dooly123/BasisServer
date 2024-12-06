using DarkRift;
public static partial class SerializableDarkRift
{
    public struct ServerSideSyncPlayerMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public LocalAvatarSyncMessage avatarSerialization;
        public byte interval;
        public void Deserialize(DeserializeEvent e)
        {
            playerIdMessage.Deserialize(e);
            avatarSerialization.Deserialize(e);
            e.Reader.Read(out interval);
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarSerialization.Dispose();
        }

        public void Serialize(SerializeEvent e)
        {
            playerIdMessage.Serialize(e);
            avatarSerialization.Serialize(e);
            e.Writer.Write(interval);
        }
    }
}
