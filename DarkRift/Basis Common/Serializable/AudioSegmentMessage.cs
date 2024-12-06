using DarkRift;
public static partial class SerializableDarkRift
{
    public struct AudioSegmentMessage : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;

        public AudioSilentSegmentDataMessage silentData;

        public AudioSegmentDataMessage audioSegmentData;
        /// <summary>
        /// the goal here is to reuse this message but drop the AudioSegmentData when its not.
        /// this forces the queue to remain correct.
        /// </summary>
        public bool wasSilentData;
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out playerIdMessage);
            if (e.Reader.Length == e.Reader.Position)
            {
                wasSilentData = true;
                e.Reader.Read(out silentData);
            }
            else
            {
                wasSilentData = false;
                audioSegmentData.Deserialize(e.Reader.deserializeEventSingleton);
            }
        }

        public void Dispose()
        {
            playerIdMessage.Dispose();
            silentData.Dispose();
            audioSegmentData.Dispose();
        }

        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(playerIdMessage);
            if (wasSilentData)
            {
                e.Writer.Write(silentData);
            }
            else
            {
                e.Writer.Write(audioSegmentData);
            }
        }
    }

    public struct VoiceReceiversMessage : IDarkRiftSerializable
    {
        public ushort[] users;

        public void Deserialize(DeserializeEvent e)
        {
            // Calculate the number of ushorts based on the remaining bytes
            int remainingBytes = (int)(e.Reader.Length - e.Reader.Position);
            int ushortCount = remainingBytes / sizeof(ushort);

            // Initialize the array with the calculated size
            users = new ushort[ushortCount];

            // Read each ushort value into the array
            for (int index = 0; index < ushortCount; index++)
            {
                users[index] = e.Reader.ReadUInt16();
            }
        }

        public void Dispose()
        {
        }

        public void Serialize(SerializeEvent e)
        {
            foreach (ushort v in users)
            {
                e.Writer.Write(v);
            }
        }
    }
}
