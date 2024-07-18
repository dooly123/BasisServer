using DarkRift;
using System.Collections.Generic;
public static partial class SerializableDarkRift
{
    public struct CreateAllRemoteMessage : IDarkRiftSerializable
    {
        public ServerReadyMessage[] serverSidePlayer;
        public void Deserialize(DeserializeEvent e)
        {
            List<ServerReadyMessage> temp = new List<ServerReadyMessage>();
            while (e.Reader.Length != e.Reader.Position)
            {
                e.Reader.Read(out ServerReadyMessage serverReadyMessage);
                temp.Add(serverReadyMessage);
            }
            serverSidePlayer = temp.ToArray();
        }
        public void Serialize(SerializeEvent e)
        {
            for (int index = 0; index < serverSidePlayer.Length; index++)
            {
                serverSidePlayer[index].Serialize(e);
            }
        }
    }
}
