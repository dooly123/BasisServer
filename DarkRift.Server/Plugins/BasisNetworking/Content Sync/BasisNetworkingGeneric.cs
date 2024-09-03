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
                HandleSceneServer(sceneDataMessage, Commands.BasisNetworking.SceneChannel, e.Client, clients);
            }
        }
        public static void HandleAvatarDataMessage(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out AvatarDataMessage avatarDataMessage);
                HandleAvatarServer(avatarDataMessage, Commands.BasisNetworking.AvatarChannel, e.Client, clients);
            }
        }
        private static void HandleSceneServer(SceneDataMessage sceneDataMessage, byte channel, IClient sender, ConcurrentDictionary<ushort, IClient> clients)
        {
            ServerSceneDataMessage serverSceneDataMessage = new ServerSceneDataMessage
            {
                SceneDataMessage = sceneDataMessage,
                PlayerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(sceneDataMessage);
                using (Message audioSegmentMessage = Message.Create(BasisTags.SceneGenericMessage, writer))
                {
                    Commands.BasisNetworking.BroadcastMessageToClients(audioSegmentMessage, channel, sender, clients, DeliveryMethod.Sequenced);
                }
            }
        }
        private static void HandleAvatarServer(AvatarDataMessage avatarDataMessage, byte channel, IClient sender, ConcurrentDictionary<ushort, IClient> clients)
        {
            ServerAvatarDataMessage serverSceneDataMessage = new ServerAvatarDataMessage
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
                using (Message audioSegmentMessage = Message.Create(BasisTags.AvatarGenericMessage, writer))
                {
                    Commands.BasisNetworking.BroadcastMessageToClients(audioSegmentMessage, channel, sender, clients, DeliveryMethod.Sequenced);
                }
            }
        }
    }
}
