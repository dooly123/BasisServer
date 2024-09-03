using DarkRift;
public static partial class SerializableDarkRift
{
    public struct ReadyMessage : IDarkRiftSerializable
    {
        public LocalAvatarSyncMessage localAvatarSyncMessage;
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        public PlayerMetaDataMessage playerMetaDataMessage;
        public void Deserialize(DeserializeEvent e)
        {
            localAvatarSyncMessage.Deserialize(e);
            clientAvatarChangeMessage.Deserialize(e);
            playerMetaDataMessage.Deserialize(e);
        }
        public void Serialize(SerializeEvent e)
        {
            localAvatarSyncMessage.Serialize(e);
            clientAvatarChangeMessage.Serialize(e);
            playerMetaDataMessage.Serialize(e);
        }
    }
    public struct ServerReadyMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public ReadyMessage localReadyMessage;
        public void Deserialize(DeserializeEvent e)
        {
            playerIdMessage.Deserialize(e);
             localReadyMessage.Deserialize(e);
        }
        public void Serialize(SerializeEvent e)
        {
            playerIdMessage.Serialize(e);
            localReadyMessage.Serialize(e);
        }
    }
}
