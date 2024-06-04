using DarkRift;
public static partial class SerializableDarkRift
{
    public struct CreateAllRemoteMessage : IDarkRiftSerializable
    {
        public ushort playerCount;
        public ServerSideSyncPlayerMessage[] serverSidePlayer;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerCount);
            serverSidePlayer = new ServerSideSyncPlayerMessage[playerCount];
            for (int index = 0; index < playerCount; index++)
            {
                serverSidePlayer[index].Deserialize(e);
            }
        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerCount);
            for (int index = 0; index < playerCount; index++)
            {
                serverSidePlayer[index].Serialize(e);
            }
        }
    }
}
