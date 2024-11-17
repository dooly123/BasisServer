
using System;
using DarkRift;

public static partial class SerializableDarkRift
{
    public struct PlayerMetaDataMessage : IDarkRiftSerializable
    {
        public string playerUUID;
        public string playerDisplayName;
        public void Deserialize(DeserializeEvent e)
        {
            if (e.Reader.Length != e.Reader.Position)
            {
                e.Reader.Read(out playerUUID);
            }
            if (e.Reader.Length != e.Reader.Position)
            {
                e.Reader.Read(out playerDisplayName);
            }
            if (string.IsNullOrEmpty(playerUUID))
            {
                Console.WriteLine("Player UUID was null or empty Deserialize");
            }
            if (string.IsNullOrEmpty(playerDisplayName))
            {
                Console.WriteLine("Player playerDisplayName was null or empty Deserialize");
            }
        }
        public void Serialize(SerializeEvent e)
        {
            if(string.IsNullOrEmpty(playerUUID))
            {
                Console.WriteLine("Player UUID was null or empty Serialize");
            }
            if (string.IsNullOrEmpty(playerDisplayName))
            {
                Console.WriteLine("Player playerDisplayName was null or empty Serialize");
            }
            e.Writer.Write(playerUUID);
            e.Writer.Write(playerDisplayName);
        }
    }
}
