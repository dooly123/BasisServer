using System.Collections.Concurrent;
using System.Threading;
using static SerializableDarkRift;
public class BasisServerReductionSystem
{
    // Default interval in milliseconds for the timer
    public static int MillisecondDefaultInterval = 100;
    public static ConcurrentDictionary<ushort, SyncedToPlayerPulse> PlayerSync = new ConcurrentDictionary<ushort, SyncedToPlayerPulse>();
    public static void AddOrUpdatePlayer(ushort playerID, ServerSideSyncPlayerMessage playerToUpdate)
    {
        if (PlayerSync.TryGetValue(playerID, out SyncedToPlayerPulse playerData))
        {
            // Update the player's message
            playerData.SupplyNewData(playerToUpdate.playerIdMessage.playerID, playerToUpdate);
        }
        else
        {
            SyncedToPlayerPulse syncedToPlayerPulse = new SyncedToPlayerPulse
            {
                playerID = playerID,
                queuedPlayerMessages = new ConcurrentDictionary<ushort, ServerSideReducablePlayer>()
            };
            if (PlayerSync.TryAdd(playerID, syncedToPlayerPulse))
            {
                playerData.SupplyNewData(playerToUpdate.playerIdMessage.playerID, playerToUpdate);
            }
        }
    }
    public static void RemovePlayer(ushort playerID)
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
        public ushort playerID;

        /// <summary>
        /// Dictionary to hold queued messages for each player.
        /// Key: Player ID, Value: Server-side player data
        /// </summary>
        public ConcurrentDictionary<ushort, ServerSideReducablePlayer> queuedPlayerMessages = new ConcurrentDictionary<ushort, ServerSideReducablePlayer>();

        /// <summary>
        /// Supply new data to a specific player.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <param name="serverSideSyncPlayerMessage">The message to be synced</param>
        public void SupplyNewData(ushort playerID, ServerSideSyncPlayerMessage serverSideSyncPlayerMessage)
        {
            if (queuedPlayerMessages.TryGetValue(playerID, out ServerSideReducablePlayer playerData))
            {
                // Update the player's message
                playerData.serverSideSyncPlayerMessage = serverSideSyncPlayerMessage;
                playerData.newDataExists = true;
                queuedPlayerMessages[playerID] = playerData;
            }
            else
            {
                // If the player doesn't exist, add them with default settings
                AddPlayer(playerID, serverSideSyncPlayerMessage);
            }
        }

        /// <summary>
        /// Adds a new player to the queue with a default timer and settings.
        /// </summary>
        /// <param name="playerID">The ID of the player</param>
        /// <param name="serverSideSyncPlayerMessage">The initial message to be sent</param>
        public void AddPlayer(ushort playerID, ServerSideSyncPlayerMessage serverSideSyncPlayerMessage)
        {
            ServerSideReducablePlayer newPlayer = new ServerSideReducablePlayer
            {
                serverSideSyncPlayerMessage = serverSideSyncPlayerMessage,
                syncRateMultiplier = 1.0f, // Default to full sync rate
                newDataExists = true,
                timer = new Timer(SendPlayerData, playerID, MillisecondDefaultInterval, MillisecondDefaultInterval)
            };

            queuedPlayerMessages[playerID] = newPlayer;
        }

        /// <summary>
        /// Removes a player from the queue and disposes of their timer.
        /// </summary>
        /// <param name="playerID">The ID of the player to remove</param>
        public void RemovePlayer(ushort playerID)
        {
            if (queuedPlayerMessages.TryRemove(playerID, out ServerSideReducablePlayer playerData))
            {
                // Dispose of the timer to free resources
                playerData.timer.Dispose();
            }
        }

        /// <summary>
        /// Callback function to send player data at regular intervals.
        /// </summary>
        /// <param name="state">The player ID (passed from the timer)</param>
        private void SendPlayerData(object state)
        {
            if (state is ushort playerID && queuedPlayerMessages.TryGetValue(playerID, out ServerSideReducablePlayer playerData))
            {
                if (playerData.newDataExists)
                {
                    // Send out data
                    //   Console.WriteLine($"Sending data to player {playerID}: {playerData.serverSideSyncPlayerMessage.Data}");

                    // Adjust the timer interval based on the sync rate multiplier
                    int adjustedInterval = (int)(MillisecondDefaultInterval * playerData.syncRateMultiplier);
                    playerData.timer.Change(adjustedInterval, adjustedInterval);
                    playerData.newDataExists = false;
                }
            }
        }
    }
    /// <summary>
    /// Structure representing a player's server-side data that can be reduced.
    /// </summary>
    public struct ServerSideReducablePlayer
    {
        public System.Threading.Timer timer;//create a new timer
        public float syncRateMultiplier; //0 to 1
        public bool newDataExists;
        public ServerSideSyncPlayerMessage serverSideSyncPlayerMessage;
    }
}
