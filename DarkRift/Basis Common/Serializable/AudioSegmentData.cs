using DarkRift;
public static partial class SerializableDarkRift
{
    public struct AudioSegmentData : IDarkRiftSerializable
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
    public struct AudioSilentSegmentData : IDarkRiftSerializable
    {
        public void Deserialize(DeserializeEvent e)
        {
        }
        public void Serialize(SerializeEvent e)
        {
        }
    }
}
