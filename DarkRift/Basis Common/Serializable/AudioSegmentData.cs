using DarkRift;
public static partial class SerializableDarkRift
{
    public struct AudioSegmentData : IDarkRiftSerializable
    {
        public ushort encodedLength;
        public byte[] buffer;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out encodedLength);
            e.Reader.Read(out buffer);

        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(encodedLength);
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
