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
                reader.Read(out SceneDataMessage SceneDataMessage);
                HandleSceneServer(SceneDataMessage, Commands.BasisNetworking.SceneChannel, e.Client, clients);
            }
        }
        public static void HandleAvatarDataMessage(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> clients)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out AvatarDataMessage AvatarDataMessage);
                HandleAvatarServer(AvatarDataMessage, Commands.BasisNetworking.AvatarChannel, e.Client, clients);
            }
        }
        private static void HandleSceneServer(SceneDataMessage SceneDataMessage, byte channel, IClient sender, ConcurrentDictionary<ushort, IClient> clients)
        {
            ServerSceneDataMessage ServerSceneDataMessage = new ServerSceneDataMessage
            {
                SceneDataMessage = SceneDataMessage,
                PlayerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(SceneDataMessage);
                using (Message audioSegmentMessage = Message.Create(BasisTags.SceneGenericMessage, writer))
                {
                    Commands.BasisNetworking.BroadcastMessageToClients(audioSegmentMessage, channel, sender, clients, DeliveryMethod.Sequenced);
                }
            }
        }
        private static void HandleAvatarServer(AvatarDataMessage SceneDataMessage, byte channel, IClient sender, ConcurrentDictionary<ushort, IClient> clients)
        {
            ServerAvatarDataMessage ServerSceneDataMessage = new ServerAvatarDataMessage
            {
                SceneDataMessage = SceneDataMessage,
                PlayerIdMessage = new PlayerIdMessage
                {
                    playerID = sender.ID
                }
            };
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(SceneDataMessage);
                using (Message audioSegmentMessage = Message.Create(BasisTags.AvatarGenericMessage, writer))
                {
                    Commands.BasisNetworking.BroadcastMessageToClients(audioSegmentMessage, channel, sender, clients, DeliveryMethod.Sequenced);
                }
            }
        }
    }
}
