using DarkRift;
public static partial class SerializableDarkRift
{
    public struct Vector3Message : IDarkRiftSerializable
    {
        public float x;
        public float y;
        public float z;
        public void SetValue(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        public void Deserialize(DeserializeEvent e)
        {
            e.Reader.Read(out x);
            e.Reader.Read(out y);
            e.Reader.Read(out z);
        }
        public void Serialize(SerializeEvent e)
        {
            e.Writer.Write(x);
            e.Writer.Write(y);
            e.Writer.Write(z);
        }
    }
}
