using DarkRift;

public static partial class SerializableDarkRift
{
    public struct SceneDataMessage : IDarkRiftSerializable
    {
        public ushort messageIndex;

        public uint payloadSize;
        public ushort recipientsSize;

        public byte[] payload;

        /// <summary>
        /// If null, it's for everyone. Otherwise, send only to the listed entries.
        /// </summary>
        public ushort[] recipients;

        public void Deserialize(DeserializeEvent e)
        {
            // Read messageIndex
            e.Reader.Read(out messageIndex);

            e.Reader.Read(out recipientsSize);
            e.Reader.Read(out payloadSize);

            recipients = new ushort[recipientsSize];
            payload = new byte[payloadSize];

            for (int index = 0; index < recipients.Length; index++)
            {
                e.Reader.Read(out recipients[index]);
            }
            for (int index = 0; index < payload.Length; index++)
            {
                e.Reader.Read(out payload[index]);
            }
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the messageIndex and buffer
            e.Writer.Write(messageIndex);

            recipientsSize = (ushort)recipients.Length;
            payloadSize = (uint)payload.Length;

            e.Writer.Write(recipientsSize);
            e.Writer.Write(payloadSize);

            for (int index = 0; index < recipients.Length; index++)
            {
                ushort recipient = recipients[index];
                e.Writer.Write(recipient);
            }
            for (int index = 0; index < payload.Length; index++)
            {
                byte payloadid = payload[index];
                e.Writer.Write(payloadid);
            }
        }
    }

    public struct ServerSceneDataMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public SceneDataMessage sceneDataMessage;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the playerIdMessage
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out sceneDataMessage);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the playerIdMessage and sceneDataMessage
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(sceneDataMessage);
        }
    }
    public struct SceneDataMessage_NoRecipients : IDarkRiftSerializable
    {
        public ushort messageIndex;
        public byte[] payload;

        public void Deserialize(DeserializeEvent e)
        {
            // Read messageIndex and payload
            e.Reader.Read(out messageIndex);
            e.Reader.Read(out payload);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the messageIndex and payload
            e.Writer.Write(messageIndex);
            e.Writer.Write(payload);
        }
    }

    public struct ServerSceneDataMessage_NoRecipients : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public SceneDataMessage_NoRecipients sceneDataMessage;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the playerIdMessage and sceneDataMessage
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out sceneDataMessage);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the playerIdMessage and sceneDataMessage
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(sceneDataMessage);
        }
    }
    public struct SceneDataMessage_NoRecipients_NoPayload : IDarkRiftSerializable
    {
        public ushort messageIndex;

        public void Deserialize(DeserializeEvent e)
        {
            // Read only messageIndex
            e.Reader.Read(out messageIndex);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the messageIndex
            e.Writer.Write(messageIndex);
        }
    }

    public struct ServerSceneDataMessage_NoRecipients_NoPayload : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public SceneDataMessage_NoRecipients_NoPayload sceneDataMessage;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the playerIdMessage and sceneDataMessage
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out sceneDataMessage);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the playerIdMessage and sceneDataMessage
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(sceneDataMessage);
        }
    }
}
