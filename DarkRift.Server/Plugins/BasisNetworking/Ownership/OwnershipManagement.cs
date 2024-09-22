using DarkRift.Basis_Common.Serializable;
using DarkRift.Server.Plugins.Commands;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace DarkRift.Server.Plugins.BasisNetworking.Ownership
{
    public class OwnershipManagement
    {
        // ConcurrentDictionary to store object ownership information (ObjectID -> PlayerID)
        public ConcurrentDictionary<ushort, ushort> ownershipDatabase = new ConcurrentDictionary<ushort, ushort>();

        // A dictionary for easy lookup by object ID (Object unique string ID -> Ownership ID)
        public ConcurrentDictionary<string, ushort> ownershipByObjectId = new ConcurrentDictionary<string, ushort>();

        public readonly object LockObject = new object();  // For synchronized multi-step operations
        private int currentOwnershipIndex = 0;

        /// <summary>
        /// Generates the next available index for networkIdentifier in a thread-safe manner.
        /// </summary>
        /// <returns></returns>
        public ushort NextAvailableIndex()
        {
            return (ushort)Interlocked.Increment(ref currentOwnershipIndex);
        }

        /// <summary>
        /// Handles the request for initialized ownership for a client with proper error handling.
        /// </summary>
        public void OwnershipInitialize(Message message, MessageReceivedEventArgs e)
        {
            try
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    reader.Read(out OwnershipInitializeMessage ownershipInitializeMessage);
                    NetworkRequestNewOrExisting(ownershipInitializeMessage, out ushort currentOwner);

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        OwnershipTransferMessage ownershipTransferMessage = new OwnershipTransferMessage
                        {
                            playerIdMessage = new SerializableDarkRift.PlayerIdMessage
                            {
                                playerID = currentOwner
                            },
                            ownershipID = ownershipInitializeMessage.uniqueOwnerLink
                        };

                        writer.Write(ownershipTransferMessage);

                        using (Message serverOwnershipInitialize = Message.Create(BasisTags.OwnershipInitialize, writer))
                        {
                            e.Client.SendMessage(serverOwnershipInitialize, Commands.BasisNetworking.EventsChannel, DeliveryMethod.ReliableSequenced);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing ownership: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles the ownership transfer for all clients with proper error handling.
        /// </summary>
        public void OwnershipTransfer(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> allClients)
        {
            try
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    reader.Read(out OwnershipTransferMessage ownershipTransferMessage);

                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        SwitchOwnership(ownershipTransferMessage.ownershipID, e.Client.ID);
                        writer.Write(ownershipTransferMessage);

                        using (Message serverOwnershipTransfer = Message.Create(BasisTags.OwnershipTransfer, writer))
                        {
                            Commands.BasisNetworking.BroadcastMessageToClients(serverOwnershipTransfer, Commands.BasisNetworking.EventsChannel, allClients, DeliveryMethod.ReliableSequenced);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error transferring ownership: {ex.Message}");
            }
        }

        /// <summary>
        /// Requests either new or existing ownership with thread safety and rollback.
        /// </summary>
        public void NetworkRequestNewOrExisting(OwnershipInitializeMessage ownershipInitializeMessage, out ushort ownershipInfo)
        {
            if (GetOwnershipInformation(ownershipInitializeMessage.uniqueOwnerLink, out ownershipInfo))
            {
                // Ownership already exists, no need to add
            }
            else
            {
                if (!AddOwnership(ownershipInitializeMessage.uniqueOwnerLink, ownershipInitializeMessage.playerIdMessage.playerID, out ownershipInfo))
                {
                    Console.WriteLine($"Error while adding ownership for: {ownershipInitializeMessage.uniqueOwnerLink}");
                }
            }
        }

        /// <summary>
        /// Adds an object with ownership information to the database in a thread-safe manner.
        /// </summary>
        public bool AddOwnership(string objectId, ushort ownerId, out ushort ownershipInformation)
        {
            ownershipInformation = NextAvailableIndex();
            bool result = ownershipDatabase.TryAdd(ownershipInformation, ownerId);

            if (result)
            {
                if (ownershipByObjectId.TryAdd(objectId, ownershipInformation))
                {
                    Console.WriteLine($"Object {objectId} added with owner {ownerId}");
                }
                else
                {
                    // Rollback if `ownershipByObjectId` fails
                    ownershipDatabase.TryRemove(ownershipInformation, out _);
                    Console.WriteLine($"Failed to add Object {objectId} to object ownership lookup.");
                    result = false;
                }
            }
            else
            {
                Console.WriteLine($"Failed to add Object {objectId}.");
            }
            return result;
        }

        /// <summary>
        /// Removes an object and its ownership information from the database in a thread-safe and consistent manner.
        /// </summary>
        public bool RemoveObject(string objectId)
        {
            lock (LockObject)
            {
                if (ownershipByObjectId.TryRemove(objectId, out ushort ownershipInformation) &&
                    ownershipDatabase.TryRemove(ownershipInformation, out ushort ownerId))
                {
                    Console.WriteLine($"Object {objectId} owned by {ownerId} removed from database.");
                    return true;
                }
                else
                {
                    Console.WriteLine($"Failed to remove object with ID {objectId}.");
                    return false;
                }
            }
        }

        /// <summary>
        /// Switches the ownership of an object in a thread-safe manner.
        /// </summary>
        public bool SwitchOwnership(string objectId, ushort newOwnerId)
        {
            lock (LockObject)
            {
                if (ownershipByObjectId.TryGetValue(objectId, out ushort ownershipInformation))
                {
                    ownershipDatabase[ownershipInformation] = newOwnerId;
                    Console.WriteLine($"Ownership of object {objectId} switched to {newOwnerId}.");
                    return true;
                }

                Console.WriteLine($"Object with ID {objectId} does not exist.");
                return false;
            }
        }

        /// <summary>
        /// Checks if an object exists in the database.
        /// </summary>
        public bool DoesObjectExistInDatabase(string objectId)
        {
            return ownershipByObjectId.ContainsKey(objectId); // Thread-safe lookup without extra locking
        }

        /// <summary>
        /// Retrieves ownership information for a specific object ID in a thread-safe manner.
        /// </summary>
        public bool GetOwnershipInformation(string objectId, out ushort ownershipInfo)
        {
            if (ownershipByObjectId.TryGetValue(objectId, out ownershipInfo))
            {
                return true;
            }

            ownershipInfo = 0;
            return false;
        }

        /// <summary>
        /// Prints current ownership database for debugging purposes with thread safety.
        /// </summary>
        public void PrintOwnershipDatabase()
        {
            Console.WriteLine("Current Ownership Database:");

            lock (LockObject)
            {
                foreach (var entry in ownershipDatabase)
                {
                    Console.WriteLine($"Ownership ID: {entry.Key}, Owner ID: {entry.Value}");
                }
            }
        }
    }
}
