using System;
using System.Buffers;
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
        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(buffer);
        }
    }
}
