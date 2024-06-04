using DarkRift;
public static partial class SerializableDarkRift
{
    public struct LocalAvatarSyncMessage : IDarkRiftSerializable
    {
        public byte[] array;
        public void Deserialize(DeserializeEvent e)
        {
            array = e.Reader.ReadBytes();
        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(array);
        }
    }
}
