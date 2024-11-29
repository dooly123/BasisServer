namespace Basis.Scripts.Networking.Compression
{
    public struct Vector3
    {
        public float x;
        public float y;
        public float z;
        public Vector3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }
        // Subtraction operator for convenience
        public static Vector3 operator -(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        }

        // Squared magnitude (squared length of a vector)
        public float SquaredMagnitude()
        {
            return x * x + y * y + z * z;
        }

    }
}
