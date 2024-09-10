using DarkRift.Server.Plugins.Commands;
using System.Collections.Concurrent;
using static SerializableDarkRift;
namespace DarkRift.Server.Plugins.BasisNetworking.Content_Sync
{
    public static class BasisNetworkingGeneric
    {
        public static void HandleSceneDataMessage(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out SceneDataMessage sceneDataMessage);
                HandleSceneServer(sceneDataMessage, Commands.BasisNetworking.SceneChannel, e.SendMode, e.Client, clients);
            }
        }
        public static void HandleAvatarDataMessage(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out AvatarDataMessage avatarDataMessage);
                HandleAvatarServer(avatarDataMessage, Commands.BasisNetworking.AvatarChannel, e.SendMode, e.Client, clients);
            }
        }
        private static void HandleSceneServer(SceneDataMessage sceneDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
        {
            ServerSceneDataMessage serverSceneDataMessage = new ServerSceneDataMessage
            {
                sceneDataMessage = sceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(sceneDataMessage);

                using (Message message = Message.Create(BasisTags.SceneGenericMessage, writer))
                {
                    if (sceneDataMessage.recipients != null && sceneDataMessage.recipients.Length > 0)
                    {
                        // Filter out clients whose ID matches the recipient list
                        var targetedClients = new ConcurrentDictionary<ushort, IClient>();

                        foreach (ushort recipientId in sceneDataMessage.recipients)
                        {
                            if (allClients.TryGetValue(recipientId, out IClient client))
                            {
                                targetedClients.TryAdd(client.ID, client);
                            }
                        }

                        // Broadcast only to targeted clients
                        if (targetedClients.Count > 0)
                        {
                            Commands.BasisNetworking.BroadcastMessageToClients(message, channel, targetedClients, method);
                        }
                    }
                    else
                    {
                        // If no recipients, broadcast to all clients except the sender
                        Commands.BasisNetworking.BroadcastMessageToClients(message, channel, sender, allClients, method);
                    }
                }
            }
        }
        private static void HandleAvatarServer(AvatarDataMessage avatarDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
        {
            ServerAvatarDataMessage serverAvatarDataMessage = new ServerAvatarDataMessage
            {
                avatarDataMessage = avatarDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(avatarDataMessage);

                using (Message message = Message.Create(BasisTags.AvatarGenericMessage, writer))
                {
                    if (avatarDataMessage.recipients != null && avatarDataMessage.recipients.Length > 0)
                    {
                        // Filter out clients whose ID matches the recipient list
                        var targetedClients = new ConcurrentDictionary<ushort, IClient>();

                        foreach (ushort recipientId in avatarDataMessage.recipients)
                        {
                            if (allClients.TryGetValue(recipientId, out IClient client))
                            {
                                targetedClients.TryAdd(client.ID, client);
                            }
                        }

                        // Broadcast only to targeted clients
                        if (targetedClients.Count > 0)
                        {
                            Commands.BasisNetworking.BroadcastMessageToClients(message, channel, targetedClients, method);
                        }
                    }
                    else
                    {
                        // If no recipients, broadcast to all clients except the sender
                        Commands.BasisNetworking.BroadcastMessageToClients(message, channel, sender, allClients, method);
                    }
                }
            }
        }
    }
}
