using DarkRift;
public static partial class SerializableDarkRift
{
    public struct AvatarDataMessage : IDarkRiftSerializable
    {
        public byte messageIndex;
        public byte[] buffer;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out messageIndex);
            e.Reader.Read(out buffer);

        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(messageIndex);
            e.Writer.Write(buffer);
        }
    }
    public struct ServerAvatarDataMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public AvatarDataMessage avatarDataMessage;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out avatarDataMessage);

        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(avatarDataMessage);
        }
    }
}
