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
        public void AddLastData(IClient client, LocalAvatarSyncMessage avatarSyncMessage)
        {
            serverSideLastState.AddOrUpdate(client.ID,
                new StoredData { lastAvatarSyncState = avatarSyncMessage },
                (key, oldValue) =>
                {
                    oldValue.lastAvatarSyncState = avatarSyncMessage;
                    return oldValue;
                });
        }

        public void AddLastData(IClient client, LocalAvatarSyncMessage avatarSyncMessage, ClientAvatarChangeMessage avatarChangeMessage)
        {
            serverSideLastState.AddOrUpdate(client.ID,
                new StoredData { lastAvatarSyncState = avatarSyncMessage, lastAvatarChangeState = avatarChangeMessage },
                (key, oldValue) =>
                {
                    oldValue.lastAvatarSyncState = avatarSyncMessage;
                    oldValue.lastAvatarChangeState = avatarChangeMessage;
                    return oldValue;
                });
        }

        public void AddLastData(IClient client, ReadyMessage readyMessage)
        {
            serverSideLastState.AddOrUpdate(client.ID,
                new StoredData
                {
                    lastAvatarSyncState = readyMessage.localAvatarSyncMessage,
                    lastAvatarChangeState = readyMessage.clientAvatarChangeMessage,
                    playerMetaDataMessage = readyMessage.playerMetaDataMessage
                },
                (key, oldValue) =>
                {
                    oldValue.lastAvatarSyncState = readyMessage.localAvatarSyncMessage;
                    oldValue.lastAvatarChangeState = readyMessage.clientAvatarChangeMessage;
                    oldValue.playerMetaDataMessage = readyMessage.playerMetaDataMessage;
                    return oldValue;
                });
            Console.WriteLine("Added " + client.ID + " with AvatarID " + readyMessage.clientAvatarChangeMessage.byteArray);
        }

        public void AddLastData(IClient client, VoiceReceiversMessage voiceReceiversMessage)
        {
            serverSideLastState.AddOrUpdate(client.ID,new StoredData { voiceReceiversMessage = voiceReceiversMessage },
                (key, oldValue) =>
                {
                    oldValue.voiceReceiversMessage = voiceReceiversMessage;
                    return oldValue;
                });
          //  Console.WriteLine("Voice from " + client.ID + " has updated who they're talking to");
        }

        public void AddLastData(IClient client, ClientAvatarChangeMessage avatarChangeMessage)
        {
            serverSideLastState.AddOrUpdate(client.ID,
                new StoredData { lastAvatarChangeState = avatarChangeMessage },
                (key, oldValue) =>
                {
                    oldValue.lastAvatarChangeState = avatarChangeMessage;
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
            public LocalAvatarSyncMessage lastAvatarSyncState; // pos 1 & 2, rot, scale, muscles
            public ClientAvatarChangeMessage lastAvatarChangeState; // last avatar state
            public PlayerMetaDataMessage playerMetaDataMessage;
            public VoiceReceiversMessage voiceReceiversMessage;
        }
    }
}
