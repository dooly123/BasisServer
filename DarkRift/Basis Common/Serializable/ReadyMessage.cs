using DarkRift;
public static partial class SerializableDarkRift
{
    public struct ReadyMessage : IDarkRiftSerializable
    {
        public LocalAvatarSyncMessage LocalAvatarSyncMessage;
        public void Deserialize(DeserializeEvent e)
        {
            LocalAvatarSyncMessage.Deserialize(e);
        }
        public void Serialize(SerializeEvent e)
        {
            LocalAvatarSyncMessage.Serialize(e);
        }
    }
}
