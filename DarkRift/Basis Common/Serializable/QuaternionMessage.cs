using DarkRift;
using System.Numerics;
public static partial class SerializableDarkRift
{
    public struct QuaternionMessage : IDarkRiftSerializable
    {
        public uint compressedRotation;
        public Quaternion rotation;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out compressedRotation);
            Compression.DecompressQuaternion(compressedRotation);
        }

        public void Dispose()
        {

        }

        public void Serialize(SerializeEvent e)
        {
            compressedRotation = Compression.CompressQuaternion(rotation);
            e.Writer.Write(compressedRotation);
        }
    }
}
