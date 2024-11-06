using DarkRift;
public static partial class SerializableDarkRift
{
    public struct LocalAvatarSyncMessage : IDarkRiftSerializable
    {
        public byte[] array;
        public void Deserialize(DeserializeEvent e)
        {
            if (array == null || array.Length == 0)
            {
                array = e.Reader.ReadBytes();
            }
            else
            {
                e.Reader.ReadBytes(ref array);
            }
        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(array);
        }
    }
}
