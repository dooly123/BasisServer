using DarkRift;

public static partial class SerializableDarkRift
{
    public struct SceneDataMessage : IDarkRiftSerializable
    {
        public ushort messageIndex;
        public byte[] payload;
        /// <summary>
        /// If null, it's for everyone. Otherwise, send only to the listed entries.
        /// </summary>
        public ushort[] recipients;

        public void Deserialize(DeserializeEvent e)
        {
            // Read messageIndex
            e.Reader.Read(out messageIndex);
            e.Reader.Read(out recipients);
            e.Reader.Read(out payload);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the messageIndex and buffer
            e.Writer.Write(messageIndex);
            e.Writer.Write(recipients);
            e.Writer.Write(payload);
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
