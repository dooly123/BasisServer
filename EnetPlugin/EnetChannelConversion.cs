using DarkRift;

namespace EnetPlugin
{
    public static class EnetChannelConversion
    {
        public static ENet.PacketFlags ActiveSendMode(SendMode mode)
        {
            switch (mode)
            {
                case SendMode.Instant:
                    return ENet.PacketFlags.Instant | ENet.PacketFlags.NoAllocate;
                case SendMode.Unreliable:
                    return ENet.PacketFlags.None | ENet.PacketFlags.NoAllocate;
                case SendMode.Reliable:
                    return ENet.PacketFlags.Reliable | ENet.PacketFlags.NoAllocate;
                case SendMode.Unsequenced:
                    return ENet.PacketFlags.Unsequenced | ENet.PacketFlags.NoAllocate;
                case SendMode.UnreliableFragmented:
                    return ENet.PacketFlags.UnreliableFragmented | ENet.PacketFlags.NoAllocate;
                default:
                    return ENet.PacketFlags.None | ENet.PacketFlags.NoAllocate;
            }
        }
    }
}
