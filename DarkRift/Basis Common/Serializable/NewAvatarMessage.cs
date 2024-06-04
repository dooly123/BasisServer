using DarkRift;
public static partial class SerializableDarkRift
{
    public struct NewAvatarMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage uShortPlayerId;
        public string avatarID;
        public void Deserialize(DeserializeEvent e)
        {
            uShortPlayerId.Deserialize(e);
            e.Reader.Read(out avatarID);
        }
        public void Serialize(SerializeEvent e)
        {
            uShortPlayerId.Serialize(e);
            e.Writer.Write(avatarID);
        }
    }
}
