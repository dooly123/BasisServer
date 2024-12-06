
using DarkRift;

public static partial class SerializableDarkRift
{
    public struct PlayerMetaDataMessage : IDarkRiftSerializable
    {
        public string playerUUID;
        public string playerDisplayName;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerUUID);
            e.Reader.Read(out playerDisplayName);
        }

        public void Dispose()
        {
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerUUID);
            e.Writer.Write(playerDisplayName);
        }
    }
}
