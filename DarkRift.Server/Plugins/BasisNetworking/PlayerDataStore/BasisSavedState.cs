using System;
using System.Collections.Concurrent;
using static SerializableDarkRift;

namespace DarkRift.Server.Plugins.BasisNetworking.PlayerDataStore
{
    public class BasisSavedState
    {
        /// <summary>
        /// Stores the last state of each player on the server side.
        /// </summary>
        private readonly ConcurrentDictionary<ushort, StoredData> serverSideLastState = new ConcurrentDictionary<ushort, StoredData>();

        /// <summary>
        /// Removes a player from the store.
        /// </summary>
        /// <param name="client">The client representing the player to be removed.</param>
        public void RemovePlayer(IClient client)
        {
            serverSideLastState.TryRemove(client.ID, out _);
        }

        /// <summary>
        /// Adds or updates the last avatar sync message for a player.
        /// </summary>
        /// <param name="client">The client representing the player.</param>
        /// <param name="avatarSyncMessage">The last avatar sync message of the player.</param>
        public void AddLastData(IClient client, LocalAvatarSyncMessage avatarSyncMessage)
        {
            serverSideLastState.AddOrUpdate(client.ID, new StoredData { LastAvatarSyncState = avatarSyncMessage },
                (key, oldValue) =>
                {
                    oldValue.LastAvatarSyncState = avatarSyncMessage;
                    return oldValue;
                });
        }
        public void AddLastData(IClient client, LocalAvatarSyncMessage avatarSyncMessage, ClientAvatarChangeMessage avatarChangeMessage)
        {
            serverSideLastState.AddOrUpdate(client.ID, new StoredData { LastAvatarSyncState = avatarSyncMessage, LastAvatarChangeState = avatarChangeMessage },
                (key, oldValue) =>
                {
                    oldValue.LastAvatarSyncState = avatarSyncMessage;
                    oldValue.LastAvatarChangeState = avatarChangeMessage;
                    return oldValue;
                });
        }
        public void AddLastData(IClient client, ReadyMessage readyMessage)
        {
            serverSideLastState.AddOrUpdate(client.ID, new StoredData { LastAvatarSyncState = readyMessage.localAvatarSyncMessage, LastAvatarChangeState = readyMessage.clientAvatarChangeMessage, PlayerMetaDataMessage = readyMessage.playerMetaDataMessage },
                (key, oldValue) =>
                {
                    oldValue.LastAvatarSyncState = readyMessage.localAvatarSyncMessage;
                    oldValue.LastAvatarChangeState = readyMessage.clientAvatarChangeMessage;
                    return oldValue;
                });
            Console.WriteLine("added " + client.ID + " With AvatarID " + readyMessage.clientAvatarChangeMessage.avatarID);
        }
        /// <summary>
        /// Adds or updates the last avatar change message for a player.
        /// </summary>
        /// <param name="client">The client representing the player.</param>
        /// <param name="avatarChangeMessage">The last avatar change message of the player.</param>
        public void AddLastData(IClient client, ClientAvatarChangeMessage avatarChangeMessage)
        {
            serverSideLastState.AddOrUpdate(client.ID, new StoredData { LastAvatarChangeState = avatarChangeMessage },
                (key, oldValue) =>
                {
                    oldValue.LastAvatarChangeState = avatarChangeMessage;
                    return oldValue;
                });
        }

        /// <summary>
        /// Retrieves the last data message for a player.
        /// </summary>
        /// <param name="client">The client representing the player.</param>
        /// <param name="storedData">The last stored data of the player.</param>
        /// <returns>True if the player is found, otherwise false.</returns>
        public bool GetLastData(IClient client, out StoredData storedData)
        {
            return serverSideLastState.TryGetValue(client.ID, out storedData);
        }

        public struct StoredData
        {
            public LocalAvatarSyncMessage LastAvatarSyncState; // pos 1 & 2, rot, scale, muscles
            public ClientAvatarChangeMessage LastAvatarChangeState; // last avatar state
            public PlayerMetaDataMessage PlayerMetaDataMessage;
        }
    }
}
