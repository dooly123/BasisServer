using DarkRift;
using System.IO;
using System;
public static partial class SerializableDarkRift
{
    public struct AvatarDataMessage : IDarkRiftSerializable
    {
        /// <summary>
        /// this is the avatar that this data is going to
        /// </summary>
        public ushort assignedAvatarPlayer;
        /// <summary>
        /// this is the message index that can be used to give some level of seperation between messages,
        /// i have included this so everyone has a understanding of the message index instead of leaving it up to the end user.
        /// </summary>
        public byte messageIndex;
        /// <summary>
        /// this is the payload
        /// </summary>
        public byte[] payload;
        /// <summary>
        /// if null its everyone else only send to the listed entrys
        /// </summary>
        public ushort[] recipients;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the assignedAvatarPlayer and messageIndex first
            e.Reader.Read(out assignedAvatarPlayer);
            e.Reader.Read(out messageIndex);
            e.Reader.Read(out payload);
            e.Reader.Read(out recipients);
        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(assignedAvatarPlayer);
            e.Writer.Write(messageIndex);
            e.Writer.Write(payload);
            e.Writer.Write(recipients);
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
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(avatarDataMessage);
        }
    }
    public struct AvatarDataMessage_NoRecipients : IDarkRiftSerializable
    {
        public ushort assignedAvatarPlayer;
        public byte messageIndex;
        public byte[] payload;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the assignedAvatarPlayer, messageIndex, and payload
            e.Reader.Read(out assignedAvatarPlayer);
            e.Reader.Read(out messageIndex);
            e.Reader.Read(out payload);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the assignedAvatarPlayer, messageIndex, and payload
            e.Writer.Write(assignedAvatarPlayer);
            e.Writer.Write(messageIndex);
            e.Writer.Write(payload);
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
    }
    public struct AvatarDataMessage_NoRecipients_NoPayload : IDarkRiftSerializable
    {
        public ushort assignedAvatarPlayer;
        public byte messageIndex;

        public void Deserialize(DeserializeEvent e)
        {
            // Read the assignedAvatarPlayer and messageIndex only
            e.Reader.Read(out assignedAvatarPlayer);
            e.Reader.Read(out messageIndex);
        }

        public void Serialize(SerializeEvent e)
        {
            // Write the assignedAvatarPlayer and messageIndex
            e.Writer.Write(assignedAvatarPlayer);
            e.Writer.Write(messageIndex);
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
    }
}
