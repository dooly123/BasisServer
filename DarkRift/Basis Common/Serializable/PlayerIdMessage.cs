using DarkRift;
public static partial class SerializableDarkRift
{
    public struct PlayerIdMessage : IDarkRiftSerializable
    {
        public ushort playerID;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerID);
        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerID);
        }
    }
}
