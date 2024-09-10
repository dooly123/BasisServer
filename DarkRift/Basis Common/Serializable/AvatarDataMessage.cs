using DarkRift;
using System.Collections.Generic;
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
            e.Reader.Read(out assignedAvatarPlayer);
            e.Reader.Read(out messageIndex);
            e.Reader.Read(out payload);
            if (e.Reader.Length != e.Reader.Position)
            {
                List<ushort> list = new List<ushort>();
                while (e.Reader.Length != e.Reader.Position)
                {
                    e.Reader.Read(out ushort recipient);
                    list.Add(recipient);
                }
                recipients = list.ToArray();
            }
            else
            {
                recipients = null;
            }
        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(assignedAvatarPlayer);
            e.Writer.Write(messageIndex);
            e.Writer.Write(payload);
            if (recipients != null)
            {
                e.Writer.Write(recipients);
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
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerIdMessage);
            e.Writer.Write(avatarDataMessage);
        }
    }
}
