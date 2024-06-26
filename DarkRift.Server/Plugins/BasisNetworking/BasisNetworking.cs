using DarkRift.Server.Plugins.BasisNetworking.MovementSync;
using DarkRift.Server.Plugins.BasisNetworking.PlayerDataStore;
using System;
using System.Collections.Generic;
using System.Linq;
using static SerializableDarkRift;

namespace DarkRift.Server.Plugins.Commands
{
    /// <summary>
    /// Helper plugin for sending messages using commands.
    /// </summary>
    public class BasisNetworking : Plugin
    {
        public AuthenticationCheck check;
        public const string AuthenticationCode = "Default";
        public BasisSavedState basisSavedState = new BasisSavedState();
        public override bool ThreadSafe => true;
        public override Version Version => new Version(1, 0, 0);
        public override Command[] Commands => new Command[] { };
        internal override bool Hidden => false;
        public static BasisNetworking Instance;
        public BasisNetworking(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            Instance = this;
            check = new AuthenticationCheck(this);
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }

        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            check.AddTimer(e.Client);
        }

        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            basisSavedState.RemovePlayer(e.Client);
            ClientDisconnection.ClientDisconnect(e, check.AuthenticatedClients);
            check.Disconnection(sender, e);
        }

        public void AuthenticationPassed(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                switch (message.Tag)
                {
                    case BasisTags.AvatarMuscleUpdateTag:
                        HandleAvatarMovement(message, e);
                        break;
                    case BasisTags.ReadyStateTag:
                        HandleReadyState(message, e);
                        break;
                    case BasisTags.AudioSegmentTag:
                        HandleVoiceMessage(message, e);
                        break;
                    case BasisTags.AvatarChangeMessage:
                        SendAvatarMessageToClients(message, e);
                        break;
                    default:
                        Logger.Log($"Message was received but no handler exists for tag {message.Tag}", LogType.Error);
                        break;
                }
            }
        }
        private void SendAvatarMessageToClients(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out ClientAvatarChangeMessage clientAvatarChangeMessage);
                ServerAvatarChangeMessage serverAvatarChangeMessage = new ServerAvatarChangeMessage
                {
                    clientAvatarChangeMessage = clientAvatarChangeMessage,
                    uShortPlayerId = new PlayerIdMessage
                    {
                        playerID = e.Client.ID
                    }
                };
                basisSavedState.AddLastData(e.Client, clientAvatarChangeMessage);
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(serverAvatarChangeMessage);

                    using (Message audioSegmentMessage = Message.Create(BasisTags.AvatarChangeMessage, writer))
                    {
                        BroadcastMessageToClients(audioSegmentMessage, e.Client, check.AuthenticatedClients);
                    }
                }
            }
        }
        private void HandleVoiceMessage(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                AudioSegmentMessage audioSegment = new AudioSegmentMessage();

                if (reader.Length == reader.Position)
                {
                    HandleSilentVoice(reader, ref audioSegment);
                }
                else
                {
                    HandleRegularVoice(reader, ref audioSegment);
                }

                SendVoiceMessageToClients(audioSegment, e.Client);
            }
        }

        private void HandleSilentVoice(DarkRiftReader reader, ref AudioSegmentMessage audioSegment)
        {
            audioSegment.wasSilentData = true;
            reader.Read(out AudioSilentSegmentDataMessage audioSilentSegmentData);
            audioSegment.silentData = audioSilentSegmentData;
        }

        private void HandleRegularVoice(DarkRiftReader reader, ref AudioSegmentMessage audioSegment)
        {
            audioSegment.wasSilentData = false;
            reader.Read(out audioSegment.audioSegmentData);
        }

        private void SendVoiceMessageToClients(AudioSegmentMessage audioSegment, IClient sender)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                audioSegment.playerIdMessage.playerID = sender.ID;
                writer.Write(audioSegment);

                using (Message audioSegmentMessage = Message.Create(BasisTags.AudioSegmentTag, writer))
                {
                    BroadcastMessageToClients(audioSegmentMessage, sender, check.AuthenticatedClients, DeliveryMethod.Sequenced);
                }
            }
        }

        private void BroadcastMessageToClients(Message message, IClient sender, List<IClient> authenticatedClients, DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced)
        {
            IEnumerable<IClient> clientsExceptSender = authenticatedClients.Where(client => client != sender);

            foreach (IClient client in clientsExceptSender)
            {
                client.SendMessage(message, deliveryMethod);
            }
        }

        private void HandleAvatarMovement(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out LocalAvatarSyncMessage local);
                basisSavedState.AddLastData(e.Client, local);
                ServerSideSyncPlayerMessage ssspm = CreateServerSideSyncPlayerMessage(local, e.Client.ID);
                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    writer.Write(ssspm);
                    using (Message ssspmMessage = Message.Create(BasisTags.AvatarMuscleUpdateTag, writer))
                    {
                        PositionSync.BroadcastPositionUpdate(e.Client, ssspmMessage, check.AuthenticatedClients);
                    }
                }
            }
        }
        private ServerSideSyncPlayerMessage CreateServerSideSyncPlayerMessage(LocalAvatarSyncMessage local, ushort clientId)
        {
            return new ServerSideSyncPlayerMessage
            {
                playerIdMessage = new PlayerIdMessage { playerID = clientId },
                avatarSerialization = local
            };
        }

        private void HandleReadyState(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out ReadyMessage ReadyMessage);
                check.SendRemoteSpawnMessage(e.Client, ReadyMessage);
            }
        }
    }
}
