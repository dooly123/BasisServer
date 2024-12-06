using DarkRift;
public static partial class SerializableDarkRift
{
    public struct CreateSingleRemoteMessage : IDarkRiftSerializable
    {
        public string password;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out password);
        }

        public void Dispose()
        {

        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(password);
        }
    }
}
