using System.Collections.Concurrent;
using static SerializableDarkRift;

namespace DarkRift.Server.Plugins.BasisNetworking.PlayerDataStore
{
    public class BasisSavedState
    {
        /// <summary>
        /// Stores the last state of each player on the server side.
        /// </summary>
        private readonly ConcurrentDictionary<ushort, ServerSideSyncPlayerMessage> serverSideLastState = new ConcurrentDictionary<ushort, ServerSideSyncPlayerMessage>();

        /// <summary>
        /// Removes a player from the store.
        /// </summary>
        /// <param name="client">The client representing the player to be removed.</param>
        public void RemovePlayer(IClient client)
        {
            serverSideLastState.TryRemove(client.ID, out _);
        }

        /// <summary>
        /// Adds or updates the last data message for a player.
        /// </summary>
        /// <param name="client">The client representing the player.</param>
        /// <param name="message">The last state message of the player.</param>
        public void AddLastData(IClient client, ServerSideSyncPlayerMessage message)
        {
            if (serverSideLastState.ContainsKey(client.ID))
            {
                serverSideLastState[client.ID] = message;
            }
            else
            {
                serverSideLastState.TryAdd(client.ID, message);
            }
        }

        /// <summary>
        /// Retrieves the last data message for a player.
        /// </summary>
        /// <param name="client">The client representing the player.</param>
        /// <returns>The last state message of the player, or null if the player is not found.</returns>
        public bool GetLastData(IClient client,out ServerSideSyncPlayerMessage SSSPM)
        {
            return serverSideLastState.TryGetValue(client.ID, out SSSPM);
        }
    }
}
