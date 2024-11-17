using DarkRift;
public static partial class SerializableDarkRift
{
    public struct ReadyMessage : IDarkRiftSerializable
    {
        public PlayerMetaDataMessage playerMetaDataMessage;
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        public LocalAvatarSyncMessage localAvatarSyncMessage;
        public void Deserialize(DeserializeEvent e)
        {
            playerMetaDataMessage.Deserialize(e);
            clientAvatarChangeMessage.Deserialize(e);
            localAvatarSyncMessage.Deserialize(e);
        }
        public void Serialize(SerializeEvent e)
        {
            playerMetaDataMessage.Serialize(e);
            clientAvatarChangeMessage.Serialize(e);
            localAvatarSyncMessage.Serialize(e);
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
