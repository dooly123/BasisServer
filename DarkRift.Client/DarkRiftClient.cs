/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using DarkRift.Server.Plugins.Commands;

namespace DarkRift.Client
{
    /// <summary>
    ///     The client for DarkRift connections.
    /// </summary>
    public class DarkRiftClient : IDisposable
    {
        /// <summary>
        ///     Event fired when a message is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        ///     Event fired when the client becomes disconnected.
        /// </summary>
        public event EventHandler<DisconnectedEventArgs> Disconnected;

        /// <summary>
        ///     The ID the client has been assigned.
        /// </summary>
        public ushort ID { get; private set; }

        /// <summary>
        ///     The state of the connection.
        /// </summary>
        public ConnectionState ConnectionState => Connection?.ConnectionState ?? ConnectionState.Disconnected;

        /// <summary>
        ///     The endpoints of the connection.
        /// </summary>
        public IEnumerable<IPEndPoint> RemoteEndPoints => Connection?.RemoteEndPoints ?? new IPEndPoint[0];


        /// <summary>
        ///     Delegate type for handling the completion of an asynchronous connect.
        /// </summary>
        /// <param name="e">The exception that occured, if any.</param>
        public delegate void ConnectCompleteHandler(Exception e);

        /// <summary>
        ///     The connection to the remote server.
        /// </summary>
        public NetworkClientConnection Connection { get; private set; }

        /// <summary>
        ///     Mutex that is triggered once the connection is completely setup.
        /// </summary>
        private readonly ManualResetEvent setupMutex = new ManualResetEvent(false);

        /// <summary>
        ///     The recommended cache settings for clients.
        /// </summary>
        public static ClientObjectCacheSettings DefaultCacheSettings => new ClientObjectCacheSettings {
            MaxWriters = 2,
            MaxReaders = 2,
            MaxMessages = 4,
            MaxMessageBuffers = 4,
            MaxSocketAsyncEventArgs = 32,
            MaxActionDispatcherTasks = 16,
            MaxAutoRecyclingArrays = 4,

            ExtraSmallMemoryBlockSize = 16,
            MaxExtraSmallMemoryBlocks = 2,
            SmallMemoryBlockSize = 64,
            MaxSmallMemoryBlocks = 2,
            MediumMemoryBlockSize = 256,
            MaxMediumMemoryBlocks = 2,
            LargeMemoryBlockSize = 1024,
            MaxLargeMemoryBlocks = 2,
            ExtraLargeMemoryBlockSize = 4096,
            MaxExtraLargeMemoryBlocks = 2,

            MaxMessageReceivedEventArgs = 4
    };

        /// <summary>
        ///     Creates a new DarkRiftClient object with default cache settings.
        /// </summary>
        public DarkRiftClient()
            : this (DefaultCacheSettings)
        {

        }

        /// <summary>
        ///     Creates a new DarkRiftClient object with specified cache settings.
        /// </summary>
        /// <param name="objectCacheSettings">The settings for the object cache.</param>
        public DarkRiftClient(ClientObjectCacheSettings objectCacheSettings)
        {
            ObjectCache.Initialize(objectCacheSettings);
            ClientObjectCache.Initialize(objectCacheSettings);
        }
        public void ConnectInBackground(NetworkClientConnection connection, string ip, int port, byte[] array, ConnectCompleteHandler callback = null)
        {
            new Thread(
                delegate ()
                {
                    try
                    {
                        Connect(connection, ip,port,array);
                    }
                    catch (Exception e)
                    {
                        callback?.Invoke(e);
                        return;
                    }

                    callback?.Invoke(null);
                }
            ).Start();
        }
        /// <summary>
        ///     Connects the client using the given connection.
        /// </summary>
        /// <param name="connection">The connection to use to connect to the server.</param>
        private void Connect(NetworkClientConnection connection, string ip, int port, byte[] array)
        {
            setupMutex.Reset();

            if (this.Connection != null)
            {
                this.Connection.Dispose();
            }

            this.Connection = connection;
            connection.MessageReceived = MessageReceivedHandler;
            connection.Disconnected = DisconnectedHandler;

            connection.Connect(ip, port, array);

            //On timeout disconnect
            if (!setupMutex.WaitOne(10000))
            {
                Connection.Disconnect("Timeout!");
            }
        }
        /// <summary>
        ///     Sends a message to the server.
        /// </summary>
        /// <param name="message">The message to send.</param>
        /// <param name="sendMode">How the message should be sent.</param>
        /// <param name="channel">the channel we are using.</param>
        /// <returns>Whether the send was successful.</returns>
        public bool SendMessage(Message message,byte channel, DeliveryMethod sendMode)
        {
            return Connection.SendMessage(message.ToBuffer(), channel, sendMode);
        }

        /// <summary>
        ///     Gets the endpoint with the given name.
        /// </summary>
        /// <param name="name">The name of the endpoint.</param>
        /// <returns>The end point.</returns>
        public IPEndPoint GetRemoteEndPoint(string name)
        {
            return Connection.GetRemoteEndPoint(name);
        }

        /// <summary>
        ///     Disconnects this client from the server.
        /// </summary>
        /// <returns>Whether the disconnect was successful.</returns>
        public bool Disconnect(string DisconnectionReason)
        {
            if (Connection == null)
                return false;

            if (!Connection.Disconnect(DisconnectionReason))
                return false;

            Disconnected?.Invoke(this, new DisconnectedEventArgs(true, SocketError.Disconnecting, null));

            return true;
        }

        /// <summary>
        ///     Callback for when data is received.
        /// </summary>
        /// <param name="buffer">The data recevied.</param>
        /// <param name="channel"></param>
        /// <param name="sendMode">The SendMode used to send the data.</param>
        private void MessageReceivedHandler(MessageBuffer buffer, byte channel, DeliveryMethod sendMode)
        {
            using (Message message = Message.Create(buffer, true))
            {
                switch (message.Tag)
                {
                    case BasisTags.Configure:
                        using (DarkRiftReader reader = message.GetReader())
                        {
                            ID = reader.ReadUInt16();
                        }
                        break;
                    case BasisTags.Identify:
                        using (DarkRiftReader reader = message.GetReader())
                        {
                            ID = reader.ReadUInt16();

                            setupMutex.Set();
                        }
                        break;
                    default:
                        HandleMessage(message, sendMode);
                        break;
                }
            }
        }

        /// <summary>
        ///     Handles a message received.
        /// </summary>
        /// <param name="message">The message that was received.</param>
        /// <param name="sendMode">The send mode the message was received with.</param>
        private void HandleMessage(Message message, DeliveryMethod sendMode)
        {
            //Invoke for message received event
            using (MessageReceivedEventArgs args = MessageReceivedEventArgs.Create(message, sendMode))
                MessageReceived?.Invoke(this, args);
        }

        /// <summary>
        ///     Called when this connection becomes disconnected.
        /// </summary>
        /// <param name="error">The error that caused the disconnection.</param>
        /// <param name="exception">The exception that caused the disconnection.</param>
        private void DisconnectedHandler(SocketError error, Exception exception)
        {
            Disconnected?.Invoke(this, new DisconnectedEventArgs(false, error, exception));
        }

        private volatile bool disposed = false;
        
        /// <summary>
        ///     Disposes of the DarkRift client object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     Handles disposing of the DarkRift client object.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !disposed)
            {
                disposed = true;

                if (Connection != null)
                    Connection.Dispose();

                setupMutex.Close();
            }
        }
    }
}
