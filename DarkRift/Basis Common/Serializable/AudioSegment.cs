using DarkRift;
public static partial class SerializableDarkRift
{
    public struct AudioSegment : IDarkRiftSerializable
    {
        public PlayerIdMessage playerIdMessage;

        public AudioSilentSegmentData silentData;

        public AudioSegmentData audioSegmentData;
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
                e.Reader.Read(out silentData);
                wasSilentData = true;
            }
            else
            {
                e.Reader.Read(out audioSegmentData);
                wasSilentData = false;
            }
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
}
