using System.Buffers;
using DarkRift;
public static partial class SerializableDarkRift
{
    public struct ServerAvatarChangeMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage uShortPlayerId;
        public ClientAvatarChangeMessage clientAvatarChangeMessage;
        public void Deserialize(DeserializeEvent e)
        {
            uShortPlayerId.Deserialize(e);
            e.Reader.Read(out clientAvatarChangeMessage);
        }

        public void Dispose()
        {
            uShortPlayerId.Dispose();
            clientAvatarChangeMessage.Dispose();
        }

        public void Serialize(SerializeEvent e)
        {
            uShortPlayerId.Serialize(e);
            e.Writer.Write(clientAvatarChangeMessage);
        }
    }
    public struct ClientAvatarChangeMessage : IDarkRiftSerializable
    {
        // Downloading - attempts to download from a URL, make sure a hash also exists.
        // BuiltIn - loads as an addressable in Unity.
        public byte loadMode;
        public ushort byteLength;
        public byte[] byteArray;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the load mode
            loadMode = e.Reader.ReadByte();

            // Read the byte length
            byteLength = e.Reader.ReadUInt16();

            // Initialize the byte array with the specified length
            byteArray = new byte[byteLength];

            // Read each byte manually into the array
            for (int index = 0; index < byteLength; index++)
            {
                byteArray[index] = e.Reader.ReadByte();
            }
        }

        public void Dispose()
        {
            ArrayPool<byte>.Shared.Return(byteArray);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the load mode
            e.Writer.Write(loadMode);

            // Update and write the byte length
            byteLength = (ushort)byteArray.Length;
            e.Writer.Write(byteLength);

            // Write each byte manually from the array
            for (int index = 0; index < byteLength; index++)
            {
                e.Writer.Write(byteArray[index]);
            }
        }
    }
}
