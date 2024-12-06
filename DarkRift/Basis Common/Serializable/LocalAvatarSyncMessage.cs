using System.Buffers;
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
                e.Reader.Read(out array);
            }
            else
            {
                e.Reader.ReadBytes(ref array);
            }
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(array);
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(array);
        }
    }
}
