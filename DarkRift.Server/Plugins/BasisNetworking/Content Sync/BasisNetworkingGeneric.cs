using DarkRift.Server.Plugins.Commands;
using System.Collections.Concurrent;
using static SerializableDarkRift;

namespace DarkRift.Server.Plugins.BasisNetworking.Content_Sync
{
    public static class BasisNetworkingGeneric
    {
        // Original SceneDataMessage handler
        public static void HandleSceneDataMessage_Recipients_Payload(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out SceneDataMessage sceneDataMessage);
                HandleSceneServer_Recipients_Payload(sceneDataMessage, Commands.BasisNetworking.SceneChannel, e.SendMode, e.Client, clients);
            }
        }
        private static void HandleSceneServer_Recipients_Payload(SceneDataMessage sceneDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
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
                writer.Write(serverSceneDataMessage);

                using (Message message = Message.Create(BasisTags.SceneGenericMessage, writer))
                {
                    if (serverSceneDataMessage.sceneDataMessage.recipients != null && serverSceneDataMessage.sceneDataMessage.recipients.Length > 0)
                    {
                        var targetedClients = new ConcurrentDictionary<ushort, IClient>();

                        foreach (ushort recipientId in serverSceneDataMessage.sceneDataMessage.recipients)
                        {
                            if (allClients.TryGetValue(recipientId, out IClient client))
                            {
                                targetedClients.TryAdd(client.ID, client);
                            }
                        }

                        if (targetedClients.Count > 0)
                        {
                            Commands.BasisNetworking.BroadcastMessageToClients(message, channel, targetedClients, method);
                        }
                    }
                    else
                    {
                        Commands.BasisNetworking.BroadcastMessageToClients(message, channel, sender, allClients, method);
                    }
                }
            }
        }

        // Original AvatarDataMessage handler
        public static void HandleAvatarDataMessage_Recipients_Payload(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out AvatarDataMessage avatarDataMessage);
                HandleAvatarServer_Recipients_Payload(avatarDataMessage, Commands.BasisNetworking.AvatarChannel, e.SendMode, e.Client, clients);
            }
        }

        private static void HandleAvatarServer_Recipients_Payload(AvatarDataMessage avatarDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
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
                writer.Write(serverAvatarDataMessage);

                using (Message message = Message.Create(BasisTags.AvatarGenericMessage, writer))
                {
                    if (avatarDataMessage.recipients != null && avatarDataMessage.recipients.Length > 0)
                    {
                        var targetedClients = new ConcurrentDictionary<ushort, IClient>();

                        int recipientsLength = avatarDataMessage.recipients.Length;
                        for (int index = 0; index < recipientsLength; index++)
                        {
                            if (allClients.TryGetValue(avatarDataMessage.recipients[index], out IClient client))
                            {
                                targetedClients.TryAdd(client.ID, client);
                            }
                        }

                        if (targetedClients.Count > 0)
                        {
                            Commands.BasisNetworking.BroadcastMessageToClients(message, channel, targetedClients, method);
                        }
                    }
                    else
                    {
                        Commands.BasisNetworking.BroadcastMessageToClients(message, channel, sender, allClients, method);
                    }
                }
            }
        }

        // New handler for SceneDataMessage without recipients
        public static void HandleSceneDataMessage_NoRecipients(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out SceneDataMessage_NoRecipients sceneDataMessage);
                HandleSceneServer_NoRecipients(sceneDataMessage, Commands.BasisNetworking.SceneChannel, e.SendMode, e.Client, clients);
            }
        }

        // New handler for AvatarDataMessage without recipients
        public static void HandleAvatarDataMessage_NoRecipients(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out AvatarDataMessage_NoRecipients avatarDataMessage);
                HandleAvatarServer_NoRecipients(avatarDataMessage, Commands.BasisNetworking.AvatarChannel, e.SendMode, e.Client, clients);
            }
        }

        // New handler for SceneDataMessage without recipients and payload
        public static void HandleSceneDataMessage_NoRecipients_NoPayload(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out SceneDataMessage_NoRecipients_NoPayload sceneDataMessage);
                HandleSceneServer_NoRecipients_NoPayload(sceneDataMessage, Commands.BasisNetworking.SceneChannel, e.SendMode, e.Client, clients);
            }
        }

        // New handler for AvatarDataMessage without recipients and payload
        public static void HandleAvatarDataMessage_NoRecipients_NoPayload(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out AvatarDataMessage_NoRecipients_NoPayload avatarDataMessage);
                HandleAvatarServer_NoRecipients_NoPayload(avatarDataMessage, Commands.BasisNetworking.AvatarChannel, e.SendMode, e.Client, clients);
            }
        }

        // Server logic for SceneDataMessage without recipients
        private static void HandleSceneServer_NoRecipients(SceneDataMessage_NoRecipients sceneDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
        {
            ServerSceneDataMessage_NoRecipients serverSceneDataMessage = new ServerSceneDataMessage_NoRecipients
            {
                sceneDataMessage = sceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(serverSceneDataMessage);

                using (Message message = Message.Create(BasisTags.SceneGenericMessage_NoRecipients, writer))
                {
                    Commands.BasisNetworking.BroadcastMessageToClients(message, channel, sender, allClients, method);
                }
            }
        }

        // Server logic for AvatarDataMessage without recipients
        private static void HandleAvatarServer_NoRecipients(AvatarDataMessage_NoRecipients avatarDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
        {
            ServerAvatarDataMessage_NoRecipients serverAvatarDataMessage = new ServerAvatarDataMessage_NoRecipients
            {
                avatarDataMessage = avatarDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(serverAvatarDataMessage);

                using (Message message = Message.Create(BasisTags.AvatarGenericMessage_NoRecipients, writer))
                {
                    Commands.BasisNetworking.BroadcastMessageToClients(message, channel, sender, allClients, method);
                }
            }
        }

        // Server logic for SceneDataMessage without recipients and payload
        private static void HandleSceneServer_NoRecipients_NoPayload(SceneDataMessage_NoRecipients_NoPayload sceneDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
        {
            ServerSceneDataMessage_NoRecipients_NoPayload serverSceneDataMessage = new ServerSceneDataMessage_NoRecipients_NoPayload
            {
                sceneDataMessage = sceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(serverSceneDataMessage);

                using (Message message = Message.Create(BasisTags.SceneGenericMessage_NoRecipients_NoPayload, writer))
                {
                    Commands.BasisNetworking.BroadcastMessageToClients(message, channel, sender, allClients, method);
                }
            }
        }

        // Server logic for AvatarDataMessage without recipients and payload
        private static void HandleAvatarServer_NoRecipients_NoPayload(AvatarDataMessage_NoRecipients_NoPayload avatarDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
        {
            
            ServerAvatarDataMessage_NoRecipients_NoPayload serverAvatarDataMessage = new ServerAvatarDataMessage_NoRecipients_NoPayload
            {
                avatarDataMessage = avatarDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(serverAvatarDataMessage);

                using (Message message = Message.Create(BasisTags.AvatarGenericMessage_NoRecipients_NoPayload, writer))
                {
                    Commands.BasisNetworking.BroadcastMessageToClients(message, channel, sender, allClients, method);
                }
            }
        }
        // New handler for SceneDataMessage with Recipients but no Payload
        public static void HandleSceneDataMessage_Recipients_NoPayload(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out SceneDataMessage_Recipients_NoPayload sceneDataMessage);
                HandleSceneServer_Recipients_NoPayload(sceneDataMessage, Commands.BasisNetworking.SceneChannel, e.SendMode, e.Client, clients);
            }
        }
        // New handler for AvatarDataMessage with Recipients but no Payload
        public static void HandleAvatarDataMessage_Recipients_NoPayload(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out AvatarDataMessage_Recipients_NoPayload avatarDataMessage);
                HandleAvatarServer_Recipients_NoPayload(avatarDataMessage, Commands.BasisNetworking.AvatarChannel, e.SendMode, e.Client, clients);
            }
        }

        // Server logic for SceneDataMessage with Recipients but no Payload
        private static void HandleSceneServer_Recipients_NoPayload(SceneDataMessage_Recipients_NoPayload sceneDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
        {
            ServerSceneDataMessage_Recipients_NoPayload serverSceneDataMessage = new ServerSceneDataMessage_Recipients_NoPayload
            {
                sceneDataMessage = sceneDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(serverSceneDataMessage);

                using (Message message = Message.Create(BasisTags.SceneGenericMessage_Recipients_NoPayload, writer))
                {
                    var targetedClients = new ConcurrentDictionary<ushort, IClient>();

                    foreach (ushort recipientId in sceneDataMessage.recipients)
                    {
                        if (allClients.TryGetValue(recipientId, out IClient client))
                        {
                            targetedClients.TryAdd(client.ID, client);
                        }
                    }

                    if (targetedClients.Count > 0)
                    {
                        Commands.BasisNetworking.BroadcastMessageToClients(message, channel, targetedClients, method);
                    }
                }
            }
        }

        // Server logic for AvatarDataMessage with Recipients but no Payload
        private static void HandleAvatarServer_Recipients_NoPayload(AvatarDataMessage_Recipients_NoPayload avatarDataMessage, byte channel, DeliveryMethod method, IClient sender, ConcurrentDictionary<ushort, IClient> allClients)
        {
            ServerAvatarDataMessage_Recipients_NoPayload serverAvatarDataMessage = new ServerAvatarDataMessage_Recipients_NoPayload
            {
                avatarDataMessage = avatarDataMessage,
                playerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };

            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(serverAvatarDataMessage);

                using (Message message = Message.Create(BasisTags.AvatarGenericMessage_Recipients_NoPayload, writer))
                {
                    var targetedClients = new ConcurrentDictionary<ushort, IClient>();

                    foreach (ushort recipientId in avatarDataMessage.recipients)
                    {
                        if (allClients.TryGetValue(recipientId, out IClient client))
                        {
                            targetedClients.TryAdd(client.ID, client);
                        }
                    }

                    if (targetedClients.Count > 0)
                    {
                        Commands.BasisNetworking.BroadcastMessageToClients(message, channel, targetedClients, method);
                    }
                }
            }
        }
    }
}
