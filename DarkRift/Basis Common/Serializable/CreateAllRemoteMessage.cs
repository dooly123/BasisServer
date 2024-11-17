using DarkRift;
using System;
using System.Collections.Generic;

public static partial class SerializableDarkRift
{
    public struct CreateAllRemoteMessage : IDarkRiftSerializable
    {
        public ServerReadyMessage[] serverSidePlayer;

        public void Deserialize(DeserializeEvent e)
        {
            // Ensure the reader is not null and has content to read
            if (e.Reader != null && e.Reader.Length > e.Reader.Position)
            {
                Console.WriteLine("Deserialization started.");

                List<ServerReadyMessage> temp = new List<ServerReadyMessage>();
                while (e.Reader.Length != e.Reader.Position)
                {
                    // Ensure that the reader is capable of reading the expected type
                    e.Reader.Read(out ServerReadyMessage serverReadyMessage);
                    temp.Add(serverReadyMessage);
                    Console.WriteLine($"Deserialized ServerReadyMessage at position: {e.Reader.Position}");
                }
                serverSidePlayer = temp.ToArray();
                Console.WriteLine($"Deserialization complete. Total ServerReadyMessages: {serverSidePlayer.Length}");
            }
            else
            {
                // Handle the case where the reader is invalid or empty
                Console.WriteLine("Reader is invalid or empty. No deserialization occurred.");
                serverSidePlayer = new ServerReadyMessage[0];
            }
        }

        public void Serialize(SerializeEvent e)
        {
            // Ensure serverSidePlayer is not null before attempting to serialize
            if (serverSidePlayer != null)
            {
                for (int index = 0; index < serverSidePlayer.Length; index++)
                {
                    serverSidePlayer[index].Serialize(e);
                }

                Console.WriteLine("Serialization complete.");
            }
            else
            {
                // Log if serverSidePlayer is null
                Console.WriteLine("serverSidePlayer array is null. Nothing to serialize.");
            }
        }
    }
}
