using DarkRift;
public static partial class SerializableDarkRift
{
    public struct AvatarDataMessage : IDarkRiftSerializable
    {
        public byte MessageIndex;
        public byte[] buffer;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out MessageIndex);
            e.Reader.Read(out buffer);

        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(MessageIndex);
            e.Writer.Write(buffer);
        }
    }
    public struct ServerAvatarDataMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage PlayerIdMessage;
        public AvatarDataMessage SceneDataMessage;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out PlayerIdMessage);
            e.Reader.Read(out SceneDataMessage);

        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(PlayerIdMessage);
            e.Writer.Write(SceneDataMessage);
        }
    }
}
