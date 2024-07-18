namespace DarkRift.Server.Plugins.Commands
{
    public class ClientDisconnection
    {
        public static void ClientDisconnect(ClientDisconnectedEventArgs e,byte channel, IClient[] authenticatedClients)
        {
            using (DarkRiftWriter w = DarkRiftWriter.Create())
            {
                w.Write(e.Client.ID);
                using (Message disconnectmessage = Message.Create(BasisTags.DisconnectTag, w))
                {
                    int clients = authenticatedClients.Length;
                    for (int index = 0; index < clients; index++)
                    {
                        authenticatedClients[index].SendMessage(disconnectmessage,channel, DeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }
    }
}
