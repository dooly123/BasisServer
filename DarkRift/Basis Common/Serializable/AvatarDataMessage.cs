using DarkRift;
using System.Buffers;
using System;
using System.IO;
public static partial class SerializableDarkRift
{
    public struct AvatarDataMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;

        public uint payloadSize;
        public ushort recipientsSize;


        public byte[] payload;
        public ushort[] recipients;
        public void Deserialize(DeserializeEvent e)
        {
            try
            {
                // Read the playerIdMessage and messageIndex first
                e.Reader.Read(out playerIdMessage);
                e.Reader.Read(out messageIndex);
                e.Reader.Read(out recipientsSize);
                e.Reader.Read(out payloadSize);

                // Validate if the reader has enough data to read the expected sizes
                if (e.Reader.Length - e.Reader.Position < recipientsSize * sizeof(ushort) + payloadSize * sizeof(byte))
                {
                    throw new EndOfStreamException("Insufficient data in stream for recipients and payload.");
                }

                // Allocate the arrays
                recipients = new ushort[recipientsSize];
                payload = new byte[payloadSize];

                // Read the recipients
                for (int index = 0; index < recipientsSize; index++)
                {
                    e.Reader.Read(out recipients[index]);
                }

                // Read the payload
                for (int index = 0; index < payloadSize; index++)
                {
                    e.Reader.Read(out payload[index]);
                }
            }
            catch (EndOfStreamException ex)
            {
                // Log or handle stream read error
                throw new EndOfStreamException("Error deserializing AvatarDataMessage: " + ex.Message, ex);
            }
        }

        public void Dispose()
        {
            ArrayPool<ushort>.Shared.Return(recipients);
            ArrayPool<byte>.Shared.Return(payload);
            playerIdMessage.Dispose();
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the playerIdMessage and messageIndex first
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(messageIndex);

            // Prepare sizes
            recipientsSize = (ushort)(recipients?.Length ?? 0);
            payloadSize = (uint)(payload?.Length ?? 0);

            // Write sizes
            e.Writer.Write(recipientsSize);
            e.Writer.Write(payloadSize);

            // Write the recipients
            if (recipients != null)
            {
                for (int index = 0; index < recipientsSize; index++)
                {
                    e.Writer.Write(recipients[index]);
                }
            }

            // Write the payload
            if (payload != null)
            {
                for (int index = 0; index < payloadSize; index++)
                {
                    e.Writer.Write(payload[index]);
                }
            }
        }
    }
    public struct ServerAvatarDataMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public AvatarDataMessage avatarDataMessage;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out avatarDataMessage);

        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarDataMessage.Dispose();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(avatarDataMessage);
        }
    }
    public struct AvatarDataMessage_NoRecipients : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;
        public byte[] payload;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the assignedAvatarPlayer, messageIndex, and payload
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out messageIndex);
            e.Reader.Read(out payload);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the assignedAvatarPlayer, messageIndex, and payload
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(messageIndex);
            e.Writer.Write(payload);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
            ArrayPool<byte>.Shared.Return(payload);
        }
    }

    public struct ServerAvatarDataMessage_NoRecipients : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public AvatarDataMessage_NoRecipients avatarDataMessage;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the playerIdMessage and avatarDataMessage
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out avatarDataMessage);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the playerIdMessage and avatarDataMessage
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(avatarDataMessage);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarDataMessage.Dispose();
        }

    }
    public struct AvatarDataMessage_NoRecipients_NoPayload : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the assignedAvatarPlayer and messageIndex only
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out messageIndex);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the assignedAvatarPlayer and messageIndex
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(messageIndex);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
        }

    }

    public struct ServerAvatarDataMessage_NoRecipients_NoPayload : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public AvatarDataMessage_NoRecipients_NoPayload avatarDataMessage;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the playerIdMessage and avatarDataMessage
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out avatarDataMessage);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the playerIdMessage and avatarDataMessage
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(avatarDataMessage);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarDataMessage.Dispose();
        }

    }
    public struct SceneDataMessage_Recipients_NoPayload : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;
        public ushort recipientsSize;
        public ushort[] recipients;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out messageIndex);
            e.Reader.Read(out recipientsSize);
            recipients = new ushort[recipientsSize];
            for (int index = 0; index < recipientsSize; index++)
            {
                e.Reader.Read(out recipients[index]);
            }
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(messageIndex);

            recipientsSize = (ushort)recipients.Length;
            e.Writer.Write(recipientsSize);

            for (int index = 0; index < recipientsSize; index++)
            {
                ushort recipient = recipients[index];
                e.Writer.Write(recipient);
            }
        }
        public void Dispose()
        {
            ArrayPool<ushort>.Shared.Return(recipients);
            playerIdMessage.Dispose();
        }

    }

    public struct ServerSceneDataMessage_Recipients_NoPayload : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public SceneDataMessage_Recipients_NoPayload sceneDataMessage;

        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out sceneDataMessage);
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(sceneDataMessage);
        }
        public void Dispose()
        {
            playerIdMessage.Dispose();
            sceneDataMessage.Dispose();
        }

    }

    public struct AvatarDataMessage_Recipients_NoPayload : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public byte messageIndex;
        public ushort recipientsSize;
        public ushort[] recipients;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out messageIndex);

            e.Reader.Read(out recipientsSize);

            recipients = new ushort[recipientsSize];

            for (int index = 0; index < recipients.Length; index++)
            {
                e.Reader.Read(out recipients[index]);
            }
        }
        public void Dispose()
        {
            ArrayPool<ushort>.Shared.Return(recipients);
            playerIdMessage.Dispose();
        }


        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(messageIndex);

            recipientsSize = (ushort)recipients.Length;

            e.Writer.Write(recipientsSize);

            for (int index = 0; index < recipients.Length; index++)
            {
                e.Writer.Write(recipients[index]);
            }
        }
    }

    public struct ServerAvatarDataMessage_Recipients_NoPayload : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public AvatarDataMessage_Recipients_NoPayload avatarDataMessage;
        public void Dispose()
        {
            playerIdMessage.Dispose();
            avatarDataMessage.Dispose();
        }

        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerIdMessage);
            e.Reader.Read(out avatarDataMessage);
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(avatarDataMessage);
        }
    }
}
