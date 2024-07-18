using System.Collections.Generic;
using System.Linq;

namespace DarkRift.Server.Plugins.BasisNetworking.MovementSync
{
    public class PositionSync
    {
        public static void BroadcastPositionUpdate(IClient sender,byte channel, Message message, IClient[] authenticatedClients)
        {
            IEnumerable<IClient> clientsExceptSender = authenticatedClients.Where(x => x != sender);

            foreach (IClient client in clientsExceptSender)
            {
                client.SendMessage(message, channel, DeliveryMethod.Sequenced);
            }
        }
    }
}
