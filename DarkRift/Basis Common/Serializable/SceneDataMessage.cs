using DarkRift;
using System.Collections.Generic;
public static partial class SerializableDarkRift
{
    public struct SceneDataMessage : IDarkRiftSerializable
    {
        public ushort messageIndex;
        public byte[] buffer;
        /// <summary>
        /// if null its everyone else only send to the listed entrys
        /// </summary>
        public ushort[] recipients;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out messageIndex);
            e.Reader.Read(out buffer);
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
            e.Writer.Write(messageIndex);
            e.Writer.Write(buffer);
            if (recipients != null)
            {
                e.Writer.Write(recipients);
            }
        }
    }
    public struct ServerSceneDataMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public SceneDataMessage sceneDataMessage;
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
    }
}
