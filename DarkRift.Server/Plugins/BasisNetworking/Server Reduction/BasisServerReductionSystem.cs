﻿using System;
using System.Collections.Concurrent;
using Basis.Scripts.Networking.Compression;
using DarkRift;
using DarkRift.Server;
using DarkRift.Server.Plugins.Commands;
using static SerializableDarkRift;
public class BasisServerReductionSystem
{
    // Default interval in milliseconds for the timer
    public static int MillisecondDefaultInterval = 33;
    public static float BaseMultiplier = 1f; // Starting multiplier.
  public static float IncreaseRate = 0.0075f; // Rate of increase per unit distance.
    public static ConcurrentDictionary<IClient, SyncedToPlayerPulse> PlayerSync = new ConcurrentDictionary<IClient, SyncedToPlayerPulse>();
    /// <summary>
    /// add the new client
    /// then update all existing clients arrays
    /// </summary>
    /// <param name="playerID"></param>
    /// <param name="playerToUpdate"></param>
    /// <param name="serverSideSyncPlayer"></param>
    public static void AddOrUpdatePlayer(IClient playerID, ServerSideSyncPlayerMessage playerToUpdate, IClient serverSideSyncPlayer)
    {
        //stage 1 lets update whoever send us this datas last player information
        if (PlayerSync.TryGetValue(serverSideSyncPlayer, out SyncedToPlayerPulse playerData))
        {
            playerData.lastPlayerInformation = playerToUpdate;
        }
        //ok now we can try to schedule sending out this data!
        if (PlayerSync.TryGetValue(playerID, out playerData))
        {
            // Update the player's message
            playerData.SupplyNewData(playerID, playerToUpdate, serverSideSyncPlayer);
        }
        else
        {
            //first time request create said data!
            playerData = new SyncedToPlayerPulse
            {
                playerID = playerID,
                queuedPlayerMessages = new ConcurrentDictionary<IClient, ServerSideReducablePlayer>(),
                lastPlayerInformation = playerToUpdate,
            };
            //ok now we can try to schedule sending out this data!
            if (PlayerSync.TryAdd(playerID, playerData))
            {  // Update the player's message
                playerData.SupplyNewData(playerID, playerToUpdate, serverSideSyncPlayer);
            }
        }
    }
    public static void RemovePlayer(IClient playerID)
    {
        if (PlayerSync.TryRemove(playerID, out SyncedToPlayerPulse pulse))
        {
            foreach (ServerSideReducablePlayer player in pulse.queuedPlayerMessages.Values)
            {
                player.timer.Dispose();
            }
        }
        foreach (SyncedToPlayerPulse player in PlayerSync.Values)
        {
            if (player.queuedPlayerMessages.TryRemove(playerID, out ServerSideReducablePlayer reduceablePlayer))
            {
                reduceablePlayer.timer.Dispose();
            }
        }
    }
    /// <summary>
    /// Structure to synchronize data with a specific player.
    /// </summary>
    public class SyncedToPlayerPulse
    {
        // The player ID to which the data is being sent
        public IClient playerID;
        public ServerSideSyncPlayerMessage lastPlayerInformation;
        /// <summary>
        /// Dictionary to hold queued messages for each player.
        /// Key: Player ID, Value: Server-side player data
        /// </summary>
        public ConcurrentDictionary<IClient, ServerSideReducablePlayer> queuedPlayerMessages = new ConcurrentDictionary<IClient, ServerSideReducablePlayer>();

        /// <summary>
        /// Supply new data to a specific player.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <param name="serverSideSyncPlayerMessage">The message to be synced</param>
        /// <param name="serverSidePlayer"></param>
        public void SupplyNewData(IClient playerID, ServerSideSyncPlayerMessage serverSideSyncPlayerMessage, IClient serverSidePlayer)
        {
            if (queuedPlayerMessages.TryGetValue(serverSidePlayer, out ServerSideReducablePlayer playerData))
            {
                // Update the player's message
                playerData.serverSideSyncPlayerMessage = serverSideSyncPlayerMessage;
                playerData.newDataExists = true;
                queuedPlayerMessages[serverSidePlayer] = playerData;
            }
            else
            {
                // If the player doesn't exist, add them with default settings
                AddPlayer(playerID, serverSideSyncPlayerMessage, serverSidePlayer);
            }
        }

        /// <summary>
        /// Adds a new player to the queue with a default timer and settings.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <param name="serverSideSyncPlayerMessage">The initial message to be sent</param>
        /// <param name="serverSidePlayer"></param>
        public void AddPlayer(IClient playerID, ServerSideSyncPlayerMessage serverSideSyncPlayerMessage, IClient serverSidePlayer)
        {
            ClientPayload clientPayload = new ClientPayload
            {
                localClient = playerID,
                dataCameFromThisUser = serverSidePlayer
            };
            ServerSideReducablePlayer newPlayer = new ServerSideReducablePlayer
            {
                serverSideSyncPlayerMessage = serverSideSyncPlayerMessage,
                newDataExists = true,
                timer = new System.Threading.Timer(SendPlayerData, clientPayload, MillisecondDefaultInterval, MillisecondDefaultInterval)
            };

            queuedPlayerMessages[serverSidePlayer] = newPlayer;
        }

        /// <summary>
        /// Removes a player from the queue and disposes of their timer.
        /// </summary>
        /// <param name="playerID">The ID of the player to remove</param>
        public void RemovePlayer(IClient playerID)
        {
            if (queuedPlayerMessages.TryRemove(playerID, out ServerSideReducablePlayer playerData))
            {
                // Dispose of the timer to free resources
                playerData.timer.Dispose();
            }
        }
        public struct ClientPayload
        {
            public IClient localClient;
            public IClient dataCameFromThisUser;
        }
        /// <summary>
        /// Callback function to send player data at regular intervals.
        /// </summary>
        /// <param name="state">The player ID (passed from the timer)</param>
        private void SendPlayerData(object state)
        {
            if (state is ClientPayload playerID && queuedPlayerMessages.TryGetValue(playerID.dataCameFromThisUser, out ServerSideReducablePlayer playerData))
            {
                if (playerData.newDataExists)
                {
                    if (PlayerSync.TryGetValue(playerID.localClient, out SyncedToPlayerPulse pulse))
                    {
                        Vector3 from = BasisBitPackerExtensions.DecompressAndProcessAvatar(pulse.lastPlayerInformation);
                        Vector3 to = BasisBitPackerExtensions.DecompressAndProcessAvatar(playerData.serverSideSyncPlayerMessage);
                        // Calculate the distance between the two points
                        float activeDistance = Distance(from, to);
                        // Adjust the timer interval based on the new syncRateMultiplier
                        int adjustedInterval = (int)(MillisecondDefaultInterval * (BaseMultiplier + (activeDistance * IncreaseRate)));
                      //  Console.WriteLine("Adjusted Interval is" + adjustedInterval);
                        playerData.timer.Change(adjustedInterval, adjustedInterval);
                    }
                    else
                    {
                        Console.WriteLine("Unable to find Pulse for LocalClient Wont Do Interval Adjust");
                    }
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        if (playerData.serverSideSyncPlayerMessage.avatarSerialization.array == null || playerData.serverSideSyncPlayerMessage.avatarSerialization.array.Length == 0)
                        {
                            Console.WriteLine("Unable to send out Avatar Data Was null or Empty!");
                        }
                        else
                        {
                            writer.Write(playerData.serverSideSyncPlayerMessage);

                            using (Message message = Message.Create(BasisTags.AvatarMuscleUpdateTag, writer))
                            {
                                playerID.localClient.SendMessage(message, BasisNetworking.MovementChannel, DeliveryMethod.Sequenced);
                            }
                        }
                    }
                    playerData.newDataExists = false;
                }
            }
        }
    }
    public static float Distance(Vector3 pointA, Vector3 pointB)
    {
        Vector3 difference = pointB - pointA;
        return difference.SquaredMagnitude();
    }

    /// <summary>
    /// Structure representing a player's server-side data that can be reduced.
    /// </summary>
    public struct ServerSideReducablePlayer
    {
        public System.Threading.Timer timer;//create a new timer
        public bool newDataExists;
        public ServerSideSyncPlayerMessage serverSideSyncPlayerMessage;
    }
}
