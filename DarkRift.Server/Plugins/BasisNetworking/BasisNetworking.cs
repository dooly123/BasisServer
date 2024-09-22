using DarkRift.Server.Plugins.BasisNetworking.Content_Sync;
using DarkRift.Server.Plugins.BasisNetworking.MovementSync;
using DarkRift.Server.Plugins.BasisNetworking.Ownership;
using DarkRift.Server.Plugins.BasisNetworking.PlayerDataStore;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static DarkRift.Server.Plugins.BasisNetworking.PlayerDataStore.BasisSavedState;
using static SerializableDarkRift;

namespace DarkRift.Server.Plugins.Commands
{
    /// <summary>
    /// Helper plugin for sending messages using commands.
    /// </summary>
    public class BasisNetworking : Plugin
    {
        public BasisSavedState basisSavedState = new BasisSavedState();
        public OwnershipManagement ownershipManagement = new OwnershipManagement();
        public override bool ThreadSafe => true;
        public override Version Version => new Version(1, 0, 0);
        public override Command[] Commands => new Command[] { };
        internal override bool Hidden => false;
        public static BasisNetworking Instance;

        public static byte EventsChannel = 0;
        public static byte MovementChannel = 1;
        public static byte VoiceChannel = 2;
        public static byte SceneChannel = 3;
        public static byte AvatarChannel = 4;
        /// <summary>
        /// this is clients that have gone past the ReadyStateTag
        /// </summary>
        public static ConcurrentDictionary<ushort, IClient> ReadyClients = new ConcurrentDictionary<ushort, IClient>();
        public BasisNetworking(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
            Instance = this;
            ClientManager.ClientConnected += ClientConnected;
            ClientManager.ClientDisconnected += ClientDisconnected;
        }
        private void ClientConnected(object sender, ClientConnectedEventArgs e)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                using (Message authenticatedMessage = Message.Create(BasisTags.AuthSuccess, writer))
                {
                    e.Client.SendMessage(authenticatedMessage, EventsChannel, DeliveryMethod.ReliableOrdered);
                }
            }
            e.Client.MessageReceived += MessageReceived;
        }
        private void ClientDisconnected(object sender, ClientDisconnectedEventArgs e)
        {
            basisSavedState.RemovePlayer(e.Client);
            ReadyClients.TryRemove(e.Client.ID, out IClient removedclient);
            ClientDisconnection.ClientDisconnect(e, EventsChannel, ReadyClients);
        }
        public void MessageReceived(object sender, MessageReceivedEventArgs e)
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
                    case BasisTags.AvatarGenericMessage:
                     //   Logger.Log("HandleAvatarDataMessage", LogType.Info);
                        BasisNetworkingGeneric.HandleAvatarDataMessage(message, e, ReadyClients);
                        break;
                    case BasisTags.AvatarGenericMessage_NoRecipients:
                        //   Logger.Log("AvatarGenericMessage_NoRecipients", LogType.Info);
                        BasisNetworkingGeneric.HandleAvatarDataMessage_NoRecipients(message, e, ReadyClients);
                        break;

                    case BasisTags.AvatarGenericMessage_NoRecipients_NoPayload:
                        //  Logger.Log("AvatarGenericMessage_NoRecipients_NoPayload", LogType.Info);
                        BasisNetworkingGeneric.HandleAvatarDataMessage_NoRecipients_NoPayload(message, e, ReadyClients);
                        break;

                    case BasisTags.SceneGenericMessage:
                        //   Logger.Log("SceneGenericMessage", LogType.Info);
                        BasisNetworkingGeneric.HandleSceneDataMessage(message, e, ReadyClients);
                        break;

                    case BasisTags.SceneGenericMessage_NoRecipients:
                     //   Logger.Log("SceneGenericMessage_NoRecipients", LogType.Info);
                        BasisNetworkingGeneric.HandleSceneDataMessage_NoRecipients(message, e, ReadyClients);
                        break;

                    case BasisTags.SceneGenericMessage_NoRecipients_NoPayload:
                      //  Logger.Log("SceneGenericMessage_NoRecipients_NoPayload", LogType.Info);
                        BasisNetworkingGeneric.HandleSceneDataMessage_NoRecipients_NoPayload(message, e, ReadyClients);
                        break;

                    case BasisTags.OwnershipInitialize:
                      Logger.Log("OwnershipInitialize", LogType.Info);
                        ownershipManagement.OwnershipInitialize(message, e);
                        break;
                    case BasisTags.OwnershipTransfer:
                        Logger.Log("OwnershipTransfer", LogType.Info);
                        ownershipManagement.OwnershipTransfer(message, e, ReadyClients);
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

                    using (Message AvatarChangeMessage = Message.Create(BasisTags.AvatarChangeMessage, writer))
                    {
                        BroadcastMessageToClients(AvatarChangeMessage, EventsChannel, e.Client, ReadyClients);
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
                SendVoiceMessageToClients(audioSegment, VoiceChannel, e.Client);
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
        private void SendVoiceMessageToClients(AudioSegmentMessage audioSegment, byte channel, IClient sender)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                audioSegment.playerIdMessage.playerID = sender.ID;
                writer.Write(audioSegment);
                using (Message audioSegmentMessage = Message.Create(BasisTags.AudioSegmentTag, writer))
                {
                    BroadcastMessageToClients(audioSegmentMessage, channel, sender, ReadyClients, DeliveryMethod.Sequenced);
                }
            }
        }
        public static void BroadcastMessageToClients(Message message, byte channel, IClient sender, ConcurrentDictionary<ushort, IClient> authenticatedClients, DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced)
        {
            IEnumerable<KeyValuePair<ushort, IClient>> clientsExceptSender = authenticatedClients.Where(client => client.Value.ID != sender.ID);

            foreach (KeyValuePair<ushort, IClient> client in clientsExceptSender)
            {
                client.Value.SendMessage(message, channel, deliveryMethod);
            }
        }
        public static void BroadcastMessageToClients(Message message, byte channel, ConcurrentDictionary<ushort, IClient> authenticatedClients, DeliveryMethod deliveryMethod = DeliveryMethod.Sequenced)
        {
            foreach (KeyValuePair<ushort, IClient> client in authenticatedClients)
            {
                client.Value.SendMessage(message, channel, deliveryMethod);
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
                        PositionSync.BroadcastPositionUpdate(e.Client, MovementChannel, ssspmMessage, ReadyClients);
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
                reader.Read(out ReadyMessage readyMessage);
                if (ReadyClients.ContainsKey(e.Client.ID) == false)
                {
                    ReadyClients.TryAdd(e.Client.ID, e.Client);
                }
                SendRemoteSpawnMessage(e.Client, readyMessage, EventsChannel);
            }
        }
        public void SendRemoteSpawnMessage(IClient authClient, ReadyMessage readyMessage, byte channel)
        {
            ServerReadyMessage serverReadyMessage = LoadInitialState(authClient, readyMessage);
            NotifyExistingClients(serverReadyMessage, channel, authClient);
            SendClientListToNewClient(authClient, BasisNetworking.EventsChannel);
        }
        public ServerReadyMessage LoadInitialState(IClient authClient, ReadyMessage readyMessage)
        {
            ServerReadyMessage serverReadyMessage = new ServerReadyMessage
            {
                localReadyMessage = readyMessage,
                playerIdMessage = new PlayerIdMessage() { playerID = authClient.ID },
            };
            BasisNetworking.Instance.basisSavedState.AddLastData(authClient, readyMessage);
            return serverReadyMessage;
        }
        private void NotifyExistingClients(ServerReadyMessage serverSideSyncPlayerMessage, byte channel, IClient authClient)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                writer.Write(serverSideSyncPlayerMessage);

                using (Message remoteCreate = Message.Create(BasisTags.CreateRemotePlayerTag, writer))
                {
                    var clientsToNotify = ReadyClients.Values.Where(client => client != authClient);

                    foreach (IClient client in clientsToNotify)
                    {
                        Console.WriteLine($"Sent Remote Spawn request to {client.ID}");
                        client.SendMessage(remoteCreate, channel, DeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }
        private void SendClientListToNewClient(IClient authClient, byte channel)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                if (ReadyClients.Count > ushort.MaxValue)
                {
                    Console.WriteLine($"authenticatedClients count exceeds {ushort.MaxValue}");
                    return;
                }

                List<ServerReadyMessage> copied = new List<ServerReadyMessage>();

                var clientsToNotify = ReadyClients.Values.Where(client => client != authClient);

                foreach (IClient client in clientsToNotify)
                {
                    ServerReadyMessage serverReadyMessage = new ServerReadyMessage();

                    if (BasisNetworking.Instance.basisSavedState.GetLastData(client, out StoredData sspm))
                    {
                        //  Console.WriteLine("Created LocalReadyMessage with avatar | " + sspm.LastAvatarChangeState.avatarID);
                        serverReadyMessage.localReadyMessage = new ReadyMessage
                        {
                            localAvatarSyncMessage = sspm.LastAvatarSyncState,
                            clientAvatarChangeMessage = sspm.LastAvatarChangeState,
                            playerMetaDataMessage = sspm.PlayerMetaDataMessage,
                        };
                        serverReadyMessage.playerIdMessage = new PlayerIdMessage() { playerID = client.ID };
                    }
                    else
                    {
                        Console.WriteLine("Unable to get last Data Creating Fake");
                        serverReadyMessage.playerIdMessage = new PlayerIdMessage { playerID = client.ID };
                        serverReadyMessage.localReadyMessage = new ReadyMessage
                        {
                            localAvatarSyncMessage = new LocalAvatarSyncMessage() { array = new byte[] { } },
                            clientAvatarChangeMessage = new ClientAvatarChangeMessage() { avatarID = string.Empty },
                            playerMetaDataMessage = new PlayerMetaDataMessage() { playerDisplayName = "Error", playerUUID = string.Empty },
                        };
                    }

                    copied.Add(serverReadyMessage);
                }

                CreateAllRemoteMessage remoteMessages = new CreateAllRemoteMessage
                {
                    serverSidePlayer = copied.ToArray(),
                };

                writer.Write(remoteMessages);

                using (Message allClientsMessage = Message.Create(BasisTags.CreateRemotePlayersTag, writer))
                {
                    Console.WriteLine($"Sending list of clients to {authClient.ID}");
                    authClient.SendMessage(allClientsMessage, channel, DeliveryMethod.ReliableOrdered);
                }
            }
        }
    }
}
