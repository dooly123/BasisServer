using System;
using System.Collections.Generic;
using System.Timers;
using static SerializableDarkRift;

namespace DarkRift.Server.Plugins.Commands
{
    public class AuthenticationCheck
    {
        private const int TimeoutSeconds = 180; // 3 minutes
        private static readonly Dictionary<IClient, System.Timers.Timer> clientTimers = new Dictionary<IClient, System.Timers.Timer>();

        public BasisNetworking BasisNetworking { get; }
        public List<IClient> AuthenticatedClients { get; } = new List<IClient>();

        public AuthenticationCheck(BasisNetworking basisNetworking)
        {
            BasisNetworking = basisNetworking;
        }

        public void AddTimer(IClient client)
        {
            client.MessageReceived += AuthenticationProcess;

            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = TimeoutSeconds * 1000,
                AutoReset = false
            };

            timer.Elapsed += (sender, e) => OnTimeoutElapsed(sender, e, client);
            clientTimers.Add(client, timer);
            timer.Start();
        }

        private void AuthenticationProcess(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                if (message.Tag == BasisTags.AuthTag)
                {
                    Console.WriteLine($"AuthenticationCheck... for {e.Client.ID}");
                    AttemptAuthentication(message, e);
                }
            }
        }

        private void AttemptAuthentication(Message message, MessageReceivedEventArgs e)
        {
            using (DarkRiftReader reader = message.GetReader())
            {
                reader.Read(out AuthenticationToServerMessage auth);

                if (auth.password == BasisNetworking.AuthenticationCode)
                {
                    SendAuthenticationSuccessMessage(e.Client);
                    e.Client.MessageReceived -= AuthenticationProcess;
                    e.Client.MessageReceived += BasisNetworking.AuthenticationPassed;
                    Console.WriteLine("Authentication Passed");
                }
                else
                {
                    SendAuthenticationFailureMessage(e.Client);
                    e.Client.MessageReceived -= AuthenticationProcess;
                    e.Client.Disconnect();
                    Console.WriteLine("Authentication Failed");
                }

                StopTimer(e.Client);
            }
        }

        public void Disconnection(object sender, ClientDisconnectedEventArgs e)
        {
            if (AuthenticatedClients.Contains(e.Client))
            {
                Console.WriteLine($"Removing {e.Client.ID}");
                AuthenticatedClients.Remove(e.Client);
            }
        }

        private void OnTimeoutElapsed(object sender, ElapsedEventArgs e, IClient client)
        {
            Console.WriteLine("Authentication timed out. Disconnecting client.");

            System.Timers.Timer timer = (System.Timers.Timer)sender;
            timer.Stop();
            client.Disconnect();
        }

        private void StopTimer(IClient client)
        {
            if (clientTimers.TryGetValue(client, out System.Timers.Timer timer))
            {
                timer.Stop();
                clientTimers.Remove(client);
            }
        }

        private void SendAuthenticationSuccessMessage(IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                using (Message authenticatedMessage = Message.Create(BasisTags.AuthSuccess, writer))
                {
                    client.SendMessage(authenticatedMessage, DeliveryMethod.ReliableOrdered);
                }
            }
        }

        private void SendAuthenticationFailureMessage(IClient client)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                using (Message authenticatedMessage = Message.Create(BasisTags.AuthFailure, writer))
                {
                    client.SendMessage(authenticatedMessage, DeliveryMethod.ReliableOrdered);
                }
            }
        }

        public void SendRemoteSpawnMessage(IClient authClient, LocalAvatarSyncMessage localAvatarSyncMessage)
        {
            LoadInitalState(authClient, localAvatarSyncMessage);
            NotifyExistingClients(authClient);
            SendClientListToNewClient(authClient);

            if (!AuthenticatedClients.Contains(authClient))
            {
                AuthenticatedClients.Add(authClient);
            }
            else
            {
                Console.WriteLine("Error: user already authenticated");
            }
        }
        public void LoadInitalState(IClient authClient, LocalAvatarSyncMessage localAvatarSyncMessage)
        {
            ServerSideSyncPlayerMessage serverSideSyncPlayerMessage = new ServerSideSyncPlayerMessage
            {
                playerIdMessage = new PlayerIdMessage { playerID = authClient.ID },
                avatarSerialization = localAvatarSyncMessage
            };
            BasisNetworking.Instance.basisSavedState.AddLastData(authClient, serverSideSyncPlayerMessage);
        }

        private void NotifyExistingClients(IClient authClient)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                PlayerIdMessage uShortPlayerId = new PlayerIdMessage { playerID = authClient.ID };
                ServerSideSyncPlayerMessage serverSideSyncPlayerMessage = new ServerSideSyncPlayerMessage();

                if (BasisNetworking.Instance.basisSavedState.GetLastData(authClient, out serverSideSyncPlayerMessage))
                {
                    serverSideSyncPlayerMessage.playerIdMessage = uShortPlayerId;
                }
                else
                {
                    serverSideSyncPlayerMessage.playerIdMessage = uShortPlayerId;
                    serverSideSyncPlayerMessage.avatarSerialization = new LocalAvatarSyncMessage { array = new byte[] { } };
                }

                writer.Write(serverSideSyncPlayerMessage);

                using (Message remoteCreate = Message.Create(BasisTags.CreateRemotePlayerTag, writer))
                {
                    foreach (IClient client in AuthenticatedClients)
                    {
                        Console.WriteLine($"Sent Remote Spawn request to {client.ID}");
                        client.SendMessage(remoteCreate, DeliveryMethod.ReliableOrdered);
                    }
                }
            }
        }

        private void SendClientListToNewClient(IClient authClient)
        {
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                if (AuthenticatedClients.Count > ushort.MaxValue)
                {
                    Console.WriteLine($"authenticatedClients count exceeds {ushort.MaxValue}");
                    return;
                }

                List<ServerSideSyncPlayerMessage> copied = new List<ServerSideSyncPlayerMessage>();

                foreach (IClient client in AuthenticatedClients)
                {
                    ServerSideSyncPlayerMessage sspm = new ServerSideSyncPlayerMessage();

                    if (!BasisNetworking.Instance.basisSavedState.GetLastData(authClient, out sspm))
                    {
                        sspm.playerIdMessage = new PlayerIdMessage { playerID = client.ID };
                        sspm.avatarSerialization = new LocalAvatarSyncMessage { array = new byte[] { } };
                      Console.WriteLine("UnAble to get last Data Creating Fake");
                    }

                    copied.Add(sspm);
                }

                CreateAllRemoteMessage remoteMessages = new CreateAllRemoteMessage
                {
                    serverSidePlayer = copied.ToArray(),
                    playerCount = (ushort)AuthenticatedClients.Count
                };

                writer.Write(remoteMessages);

                using (Message allClientsMessage = Message.Create(BasisTags.CreateRemotePlayersTag, writer))
                {
                    Console.WriteLine($"Sending list of clients to {authClient.ID}");
                    authClient.SendMessage(allClientsMessage, DeliveryMethod.ReliableOrdered);
                }
            }
        }
    }
}
