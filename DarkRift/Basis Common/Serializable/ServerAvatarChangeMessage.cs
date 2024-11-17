using System;
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

            // Check if byteLength is greater than 0, otherwise we leave byteArray as null
            if (byteLength > 0)
            {
                byteArray = new byte[byteLength];

                // Read each byte manually into the array
                for (int index = 0; index < byteLength; index++)
                {
                    byteArray[index] = e.Reader.ReadByte();
                }
            }
            else
            {
                byteArray = null;  // No data, set byteArray to null
                Console.WriteLine("Missing Payload!");
            }
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the load mode
            e.Writer.Write(loadMode);

            // Check if byteArray is null or has zero length
            if (byteArray == null || byteArray.Length == 0)
            {
                byteLength = 0; // Set byteLength to 0 if there's no data
                Console.WriteLine("Missing Payload!");
            }
            else
            {
                byteLength = (ushort)byteArray.Length;
            }

            // Write byte length
            e.Writer.Write(byteLength);

            // If byteArray is not null or empty, write each byte manually
            if (byteArray != null && byteArray.Length > 0)
            {
                for (int index = 0; index < byteLength; index++)
                {
                    e.Writer.Write(byteArray[index]);
                }
            }
            else
            {
                Console.WriteLine("Missing Payload!");
            }
        }
    }
}
