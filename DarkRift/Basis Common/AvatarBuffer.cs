using Basis.Scripts.Networking.Compression;

namespace DarkRift.Basis_Common
{
    [System.Serializable]
    public struct AvatarBuffer
    {
        public quaternion rotation;
        public float3 Scale;
        public float3 Position;
        public float[] Muscles;
        public double timestamp;
    }
}
