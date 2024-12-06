using static SerializableDarkRift;

namespace DarkRift.Basis_Common.Serializable
{
    public struct OwnershipTransferMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;
        public string ownershipID;
        public void Deserialize(DeserializeEvent e)
        {
            playerIdMessage.Deserialize(e);
            e.Reader.Read(out ownershipID);
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
        }

        public void Serialize(SerializeEvent e)
        {
            playerIdMessage.Serialize(e);
            e.Writer.Write(ownershipID);
        }
    }
}
