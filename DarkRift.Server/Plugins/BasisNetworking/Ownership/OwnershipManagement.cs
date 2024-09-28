using DarkRift.Basis_Common.Serializable;
using DarkRift.Server.Plugins.Commands;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace DarkRift.Server.Plugins.BasisNetworking.Ownership
{
    public class OwnershipManagement
    {
        // A dictionary for easy lookup by object ID (Object unique string ID -> Ownership ID)
        public ConcurrentDictionary<string, ushort> ownershipByObjectId = new ConcurrentDictionary<string, ushort>();

        public readonly object LockObject = new object();  // For synchronized multi-step operations
        public void OwnershipResponse(Message message, MessageReceivedEventArgs e, ConcurrentDictionary<ushort, IClient> allClients)
        {
            try
            {
                using (DarkRiftReader reader = message.GetReader())
                {
                    reader.Read(out OwnershipTransferMessage ownershipTransferMessage);
                    //if we are not aware of this ownershipID lets only give back to that client that its been assigned to them
                    //the goal here is to make it so ownership understanding has to be requested.
                    //once a ownership has been requested there good for life or when a ownership switch happens.
                    NetworkRequestNewOrExisting(ownershipTransferMessage, out ushort currentOwner);
                    using (DarkRiftWriter writer = DarkRiftWriter.Create())
                    {
                        ownershipTransferMessage.playerIdMessage.playerID = currentOwner;
                        writer.Write(ownershipTransferMessage);
                        Console.WriteLine("OwnershipResponse " + currentOwner + " for " + ownershipTransferMessage.playerIdMessage);

                        using (Message serverOwnershipInitialize = Message.Create(BasisTags.OwnershipResponse, writer))
                        {
                            e.Client.SendMessage(serverOwnershipInitialize, Commands.BasisNetworking.EventsChannel, DeliveryMethod.ReliableSequenced);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error Reqesting ownership: {ex.Message}");
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
                        //all clients need to know about a ownership switch
                        if (SwitchOwnership(ownershipTransferMessage.ownershipID, e.Client.ID))
                        {
                            ownershipTransferMessage.playerIdMessage.playerID = e.Client.ID;
                            writer.Write(ownershipTransferMessage);

                            Console.WriteLine("OwnershipResponse " + ownershipTransferMessage.ownershipID + " for " + ownershipTransferMessage.playerIdMessage);

                            using (Message serverOwnershipTransfer = Message.Create(BasisTags.OwnershipTransfer, writer))
                            {
                                Commands.BasisNetworking.BroadcastMessageToClients(serverOwnershipTransfer, Commands.BasisNetworking.EventsChannel, allClients, DeliveryMethod.ReliableSequenced);
                            }
                        }
                        else
                        {
                            //if we are not aware of this ownershipID lets only give back to that client that its been assigned to them
                            //the goal here is to make it so ownership understanding has to be requested.
                            //once a ownership has been requested there good for life or when a ownership switch happens.
                            NetworkRequestNewOrExisting(ownershipTransferMessage, out ushort currentOwner);
                            writer.Write(ownershipTransferMessage);
                            using (Message serverOwnershipInitialize = Message.Create(BasisTags.OwnershipTransfer, writer))
                            {
                                e.Client.SendMessage(serverOwnershipInitialize, Commands.BasisNetworking.EventsChannel, DeliveryMethod.ReliableSequenced);
                            }
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
        public bool NetworkRequestNewOrExisting(OwnershipTransferMessage ownershipInitializeMessage, out ushort ownershipInfo)
        {
            if (GetOwnershipInformation(ownershipInitializeMessage.ownershipID, out ownershipInfo))
            {
                // Ownership already exists, no need to add
                return false;
            }
            else
            {
                if (!AddOwnership(ownershipInitializeMessage.ownershipID, ownershipInitializeMessage.playerIdMessage.playerID))
                {
                    Console.WriteLine($"Error while adding ownership for: {ownershipInitializeMessage.ownershipID}");
                    return false;
                }
                else
                {
                    ownershipInfo = ownershipInitializeMessage.playerIdMessage.playerID;
                }
            }
            return true;
        }

        /// <summary>
        /// Adds an object with ownership information to the database in a thread-safe manner.
        /// </summary>
        public bool AddOwnership(string objectId, ushort ownerId)
        {
            if (ownershipByObjectId.TryAdd(objectId, ownerId))
            {
                Console.WriteLine($"Object {objectId} added with owner {ownerId}");
                return true;
            }
            else
            {
                Console.WriteLine($"Failed to add Object {objectId} to object ownership lookup.");
                return false;
            }
        }

        /// <summary>
        /// Removes an object and its ownership information from the database in a thread-safe and consistent manner.
        /// </summary>
        public bool RemoveObject(string objectId)
        {
            lock (LockObject)
            {
                if (ownershipByObjectId.TryRemove(objectId, out ushort ownershipInformation))
                {
                    Console.WriteLine($"Object {objectId} owned by {ownershipInformation} removed from database.");
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
                if (ownershipByObjectId.TryGetValue(objectId, out ushort currentOwnerId))
                {
                    // Update ownership only if the current owner matches
                    if (ownershipByObjectId.TryUpdate(objectId, newOwnerId, currentOwnerId))
                    {
                        Console.WriteLine($"Ownership of object {objectId} switched from {currentOwnerId} to {newOwnerId}.");
                        return true;
                    }
                }
                else
                {
                    Console.WriteLine($"Ownership failed to switch ObjectId " + objectId + " is not in dictionary");
                }

                Console.WriteLine($"Object with ID {objectId} does not exist or ownership change failed.");
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
                foreach (var entry in ownershipByObjectId)
                {
                    Console.WriteLine($"Ownership ID: {entry.Key}, Owner ID: {entry.Value}");
                }
            }
        }

        /// <summary>
        /// Removes all ownership of a specific player and notifies all clients.
        /// </summary>
        public void RemovePlayerOwnership(ushort playerId)
        {
            lock (LockObject)
            {
                List<string> objectsToRemove = new List<string>();

                // Collect all object IDs owned by the player
                foreach (KeyValuePair<string, ushort> entry in ownershipByObjectId)
                {
                    if (entry.Value == playerId)
                    {
                        objectsToRemove.Add(entry.Key);
                    }
                }
                foreach (string client in objectsToRemove)
                {
                    ownershipByObjectId.TryRemove(client, out ushort user);
                }
                Console.WriteLine($"Player {playerId}'s ownership removed from {objectsToRemove.Count} objects.");
            }
        }
    }
}
