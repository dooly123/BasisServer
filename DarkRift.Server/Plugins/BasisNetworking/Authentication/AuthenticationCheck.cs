using System;
using System.Collections.Generic;
using static SerializableDarkRift;

namespace DarkRift.Server.Plugins.Commands
{
    public class AuthenticationCheck
    {
        private const int TimeoutSeconds = 180; // 3 minutes
        private static readonly Dictionary<IClient, System.Timers.Timer> clientTimers = new Dictionary<IClient, System.Timers.Timer>();
        public BasisNetworking basisNetworking;
        public List<IClient> authenticatedClients = new List<IClient>();
        public AuthenticationCheck(BasisNetworking basisNetworking)
        {
            this.basisNetworking = basisNetworking;
        }
        public void AddTimer(IClient client)
        {
            client.MessageReceived += AuthenticationProcess;
            System.Timers.Timer timer = new System.Timers.Timer
            {
                Interval = TimeoutSeconds * 1000,
                AutoReset = false
            };
            timer.Elapsed += (sender, e) => OnTimeoutElapsed(sender, e, client); // Pass client to the event handler
            clientTimers.Add(client, timer);
            timer.Start();
        }
        public void AuthenticationProcess(object sender, MessageReceivedEventArgs e)
        {
            using (Message message = e.GetMessage())
            {
                if (message.Tag == BasisTags.AuthenticationTag)
                {
                    Console.WriteLine("AuthenticationCheck... for " + e.Client.ID);
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
                    e.Client.MessageReceived += basisNetworking.AuthenticationPassed;
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
            if (authenticatedClients.Contains(e.Client))
            {
                Console.WriteLine("Removing " + e.Client.ID);
                authenticatedClients.Remove(e.Client);
            }
        }
        private void OnTimeoutElapsed(object sender, System.Timers.ElapsedEventArgs e, IClient client)
        {
            // Disconnect the client if authentication takes too long
            Console.WriteLine("Authentication timed out. Disconnecting client.");
            System.Timers.Timer timer = (System.Timers.Timer)sender; // Cast sender back to timer
            timer.Stop(); // Stop the timer
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
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                using (Message authenticatedMessage = Message.Create(BasisTags.AuthenticationSucess, newPlayerWriter))
                {
                    client.SendMessage(authenticatedMessage, SendMode.Reliable);
                }
            }
        }

        private void SendAuthenticationFailureMessage(IClient client)
        {
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                using (Message authenticatedMessage = Message.Create(BasisTags.AuthenticationFailure, newPlayerWriter))
                {
                    client.SendMessage(authenticatedMessage, SendMode.Reliable);
                }
            }
        }
        public void SendRemoteSpawnMessage(IClient authclient)
        {
            // Inform all existing clients about the new client connection
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                PlayerIdMessage uShortPlayerId = new PlayerIdMessage
                {
                    playerID = authclient.ID
                };
                LocalAvatarSyncMessage lASM = new LocalAvatarSyncMessage
                {
                    array = new byte[] { },
                };
                ServerSideSyncPlayerMessage remote = new ServerSideSyncPlayerMessage
                {
                    playerIdMessage = uShortPlayerId,
                    avatarSerialization = lASM
                };
                newPlayerWriter.Write(remote);
                using (Message remoteCreate = Message.Create(BasisTags.CreateRemotePlayerTag, newPlayerWriter))
                {
                    foreach (IClient client in authenticatedClients)
                    {
                        Console.WriteLine("Sent Remote Spawn request to " + client.ID);
                        client.SendMessage(remoteCreate, SendMode.Reliable);
                    }
                }
            }

            // Send the full list of clients to the newly connected client
            using (DarkRiftWriter newPlayerWriter = DarkRiftWriter.Create())
            {
                if (authenticatedClients.Count > ushort.MaxValue)
                {
                    Console.Write("authenticatedClients was at then " + ushort.MaxValue);
                    return;
                }
                CreateAllRemoteMessage remoteMessages = new CreateAllRemoteMessage
                {
                    serverSidePlayer = new ServerSideSyncPlayerMessage[] {},
                    playerCount = (ushort)authenticatedClients.Count
                };
                List<ServerSideSyncPlayerMessage> copied = new List<ServerSideSyncPlayerMessage>();
                foreach (IClient client in authenticatedClients)
                {
                    PlayerIdMessage uShortPlayerId = new PlayerIdMessage
                    {
                        playerID = client.ID
                    };
                    LocalAvatarSyncMessage lASM = new LocalAvatarSyncMessage
                    {
                        array = new byte[] { },
                    };
                    ServerSideSyncPlayerMessage sspm = new ServerSideSyncPlayerMessage
                    {
                         playerIdMessage = uShortPlayerId,
                        avatarSerialization = lASM
                    };
                    copied.Add(sspm);
                }
                remoteMessages.serverSidePlayer = copied.ToArray();
                newPlayerWriter.Write(remoteMessages);
                using (Message allClientsMessage = Message.Create(BasisTags.CreateAllRemoteClientsTag, newPlayerWriter))
                {
                    Console.WriteLine("Sending list of clients to " + authclient.ID);
                    authclient.SendMessage(allClientsMessage, SendMode.Reliable);
                }
            }
            if (authenticatedClients.Contains(authclient) == false)
            {
                authenticatedClients.Add(authclient);
            }
            else
            {
                Console.WriteLine("Error user already authenticated");
            }
        }
    }
}
