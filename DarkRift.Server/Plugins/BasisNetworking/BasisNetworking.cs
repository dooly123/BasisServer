using DarkRift.Server.Plugins.BasisNetworking.MovementSync;
using System;
using System.Collections.Generic;
using System.Linq;
using static SerializableDarkRift;
namespace DarkRift.Server.Plugins.Commands
{
    /// <summary>
    ///     Helper plugin for sending messages using commands.
    /// </summary>
    public class BasisNetworking : Plugin
    {
        public AuthenticationCheck check;
        public const string AuthenticationCode = "Default";
        public override bool ThreadSafe => true;
        public override Version Version => new Version(1, 0, 0);
        public override Command[] Commands => new Command[]{ };
        internal override bool Hidden => false;
        public BasisNetworking(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            check = new AuthenticationCheck(this);
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }
        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            ClientDisconnection.ClientDisconnect(e, check.authenticatedClients);
            check.Disconnection(sender, e);
        }
        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            check.AddTimer(e.Client);
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
                        Ready(message, e);
                        break;
                    case BasisTags.AudioSegmentTag:
                        HandleVoice(message, e);
                        break;
                    default:
                        Logger.Log("Message was recieved but no function exists " + message.Tag, LogType.Error);
                        // Handle unrecognized message tags
                        break;
                }
            }
        }
        private void HandleVoice(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                AudioSegment audioSegment = new AudioSegment();
                if (reader.Length == reader.Position)
                {
                    HandleSilentVoice(reader,ref audioSegment);
                }
                else
                {
                    HandleRegularVoice(reader, ref audioSegment);
                }

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    audioSegment.playerIdMessage.playerID = e.Client.ID;
                    writer.Write(audioSegment);
                    using (Message audioSegmentMessage = Message.Create(BasisTags.AudioSegmentTag, writer))
                    {
                        BroadcastAudioUpdate(e.Client, audioSegmentMessage, check.authenticatedClients);
                    }
                }
            }
        }
        public void HandleSilentVoice(DarkRiftReader reader,ref AudioSegment audioSegment)
        {
            audioSegment.wasSilentData = true;
            reader.Read(out AudioSilentSegmentData audioSilentSegmentData);
            audioSegment.silentData = audioSilentSegmentData;
        }
        public void HandleRegularVoice(DarkRiftReader reader,ref AudioSegment audioSegment)
        {
            audioSegment.wasSilentData = false;
            reader.Read(out AudioSegmentData audioSegmentData);
            audioSegment.audioSegmentData = audioSegmentData;
        }
        public static void BroadcastAudioUpdate(IClient sender, Message message, List<IClient> authenticatedClients)
        {
            IEnumerable<IClient> clientsExceptSender = authenticatedClients.Where(x => x != sender);

            foreach (IClient client in clientsExceptSender)
            {
                client.SendMessage(message, SendMode.Unreliable);
            }
        }
        private void HandleAvatarMovement(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out LocalAvatarSyncMessage local);

                using (DarkRiftWriter writer = DarkRiftWriter.Create())
                {
                    ServerSideSyncPlayerMessage ssspm = new ServerSideSyncPlayerMessage();
                    PlayerIdMessage playerIdMessage = new PlayerIdMessage
                    {
                        playerID = e.Client.ID
                    };
                    ssspm.playerIdMessage = playerIdMessage;
                    ssspm.avatarSerialization = local;

                    writer.Write(ssspm);
                    using (Message ssspmmessage = Message.Create(BasisTags.AvatarMuscleUpdateTag, writer))
                    {
                        PositionSync.BroadcastPositionUpdate(e.Client, ssspmmessage, check.authenticatedClients);
                    }
                }
            }
        }
        private void Ready(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                check.SendRemoteSpawnMessage(e.Client);
            }
        }
    }
}
