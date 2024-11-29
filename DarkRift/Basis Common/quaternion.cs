namespace Basis.Scripts.Networking.Compression
{
    public struct quaternion
    {
        public Vector4 value;
        public quaternion(float x, float y, float z, float w) : this()
        {
            this.value.x = x;
            this.value.y = y;
            this.value.z = z;
            this.value.w = w;
        }
    }
}
