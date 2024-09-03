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
        public byte loadMode;
        // Downloading,//attempts to download from a url, make sure a hash also exists.
        //    BuiltIn,//loads as a addressable in unity.
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out avatarID);
            e.Reader.Read(out loadMode);
        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(avatarID);
            e.Writer.Write(loadMode);
        }
    }
}
