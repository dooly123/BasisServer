
using DarkRift;
public static partial class SerializableDarkRift
{
    public struct LocalAvatarSyncMessage : IDarkRiftSerializable
    {
        public byte[] array;
        public int size;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.ReadBytes(ref array,out size);
        }

        public void Dispose()
        {
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(array);
        }
    }
}
