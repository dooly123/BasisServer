using System;
using System.Collections.Generic;
using System.Linq;

namespace DarkRift.Server.Plugins.BasisNetworking.MovementSync
{
    public class PositionSync
    {
        public static void BroadcastPositionUpdate(IClient sender, Message message, List<IClient> authenticatedClients)
        {
            IEnumerable<IClient> clientsExceptSender = authenticatedClients.Where(x => x != sender);

            foreach (IClient client in clientsExceptSender)
            {
                client.SendMessage(message, SendMode.Unreliable);
            }
        }
    }
}
