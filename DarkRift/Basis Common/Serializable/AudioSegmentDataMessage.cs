using DarkRift;
public static partial class SerializableDarkRift
{
    public struct AudioSegmentDataMessage : IDarkRiftSerializable
    {
        public byte[] buffer;
        public int size;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.ReadBytes(ref buffer,out size);
        }
        public void Dispose()
        {
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(buffer);
        }
    }
}
