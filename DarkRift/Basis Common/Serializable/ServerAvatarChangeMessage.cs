using DarkRift;
public static partial class SerializableDarkRift
{
    public struct ServerAvatarChangeMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage uShortPlayerId;
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        public void Deserialize(DeserializeEvent e)
        {
            uShortPlayerId.Deserialize(e);
            e.Reader.Read(out clientAvatarChangeMessage);
        }
        public void Serialize(SerializeEvent e)
        {
            uShortPlayerId.Serialize(e);
            e.Writer.Write(clientAvatarChangeMessage);
        }
    }
    public struct ClientAvatarChangeMessage : IDarkRiftSerializable
    {
        public string avatarID;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out avatarID);
        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(avatarID);
        }
    }
}
