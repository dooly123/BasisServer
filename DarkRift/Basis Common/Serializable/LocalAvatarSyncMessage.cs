using System;
using DarkRift;
public static partial class SerializableDarkRift
{
    public struct LocalAvatarSyncMessage : IDarkRiftSerializable
    {
        public byte[] array;
        public void Deserialize(DeserializeEvent e)
        {
            array = e.Reader.ReadRaw(412);
            if (array == null || array.Length == 0)
            {
                Console.WriteLine($"Array was empty or null for {nameof(LocalAvatarSyncMessage)}");
            }
        }
        public void Serialize(SerializeEvent e)
        {
            if (array == null || array.Length == 0)
            {
                Console.WriteLine($"Array was empty or null for {nameof(LocalAvatarSyncMessage)}");
            }
            e.Writer.WriteRaw(array, 0, 412);
        }
    }
}
