using DarkRift;
public static partial class SerializableDarkRift
{
    public struct AudioSegmentDataMessage : IDarkRiftSerializable
    {
        public byte[] buffer;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out buffer);

        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(buffer);
        }
    }
    public struct AudioSilentSegmentDataMessage : IDarkRiftSerializable
    {
        public void Deserialize(DeserializeEvent e)
        {
        }
        public void Serialize(SerializeEvent e)
        {
        }
    }
}
