using System;
using System.Collections.Generic;

namespace DarkRift.Server.Plugins.Commands
{
    public class ClientDisconnection
    {
        public static void ClientDisconnect(ClientDisconnectedEventArgs e, List<IClient> authenticatedClients)
        {
            using (DarkRiftWriter w = DarkRiftWriter.Create())
            {
                w.Write(e.Client.ID);
                using (Message disconnectmessage = Message.Create(BasisTags.DisconnectTag, w))
                {
                    int clients = authenticatedClients.Count;
                    for (int index = 0; index < clients; index++)
                    {
                        authenticatedClients[index].SendMessage(disconnectmessage, DeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }
    }
}
