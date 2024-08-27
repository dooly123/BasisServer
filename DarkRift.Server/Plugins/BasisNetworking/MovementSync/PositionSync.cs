using System.Collections.Concurrent;
using System.Linq;
namespace DarkRift.Server.Plugins.BasisNetworking.MovementSync
{
    public class PositionSync
    {
        public static void BroadcastPositionUpdate(IClient sender, byte channel, Message message, ConcurrentDictionary<ushort, IClient> authenticatedClients)
        {
            var clientsToNotify = authenticatedClients.Values.Where(client => client != sender);

            foreach (IClient client in clientsToNotify)
            {
                client.SendMessage(message, channel, DeliveryMethod.Sequenced);
            }
        }
    }
}
