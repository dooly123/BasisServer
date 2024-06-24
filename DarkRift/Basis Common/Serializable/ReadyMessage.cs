using DarkRift;
public static partial class SerializableDarkRift
{
    public struct ReadyMessage : IDarkRiftSerializable
    {
        public LocalAvatarSyncMessage localAvatarSyncMessage;
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        public void Deserialize(DeserializeEvent e)
        {
            localAvatarSyncMessage.Deserialize(e);
            clientAvatarChangeMessage.Deserialize(e);
        }
        public void Serialize(SerializeEvent e)
        {
            localAvatarSyncMessage.Serialize(e);
            clientAvatarChangeMessage.Serialize(e);
        }
    }
    public struct ServerReadyMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public ReadyMessage LocalReadyMessage;
        public void Deserialize(DeserializeEvent e)
        {
            playerIdMessage.Deserialize(e);
             LocalReadyMessage.Deserialize(e);
        }
        public void Serialize(SerializeEvent e)
        {
            playerIdMessage.Serialize(e);
            LocalReadyMessage.Serialize(e);
        }
    }
}
