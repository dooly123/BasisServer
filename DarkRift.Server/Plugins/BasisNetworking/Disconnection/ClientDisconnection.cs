using System.Collections.Concurrent;
namespace DarkRift.Server.Plugins.Commands
{
    public class ClientDisconnection
    {
        public static void ClientDisconnect(ClientDisconnectedEventArgs e, byte channel, ConcurrentDictionary<ushort, IClient> authenticatedClients)
        {
            using (DarkRiftWriter w = DarkRiftWriter.Create())
            {
                w.Write(e.Client.ID);
                using (Message disconnectmessage = Message.Create(BasisTags.DisconnectTag, w))
                {
                    foreach (IClient client in authenticatedClients.Values)
                    {
                        client.SendMessage(disconnectmessage, channel, DeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }
    }
}
