﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DarkRift.Server.Plugins.Commands
{
    internal class MockedNetworkServerConnection : NetworkServerConnection
    {
        public override ConnectionState ConnectionState => ConnectionState.Connected;

        public override IEnumerable<IPEndPoint> RemoteEndPoints { get; }

        /// <summary>
        ///     Whether the data in the message should be output in hex on receive.
        /// </summary>
        private readonly bool outputData;

        /// <summary>
        ///     The client command plugin that owns us.
        /// </summary>
        private ClientCommand clientCommand;

        public MockedNetworkServerConnection(ClientCommand clientCommand, IPAddress ip, ushort port, bool outputData)
        {
            this.clientCommand = clientCommand;
            this.RemoteEndPoints = new IPEndPoint[] { new IPEndPoint(ip, port) };
            this.outputData = outputData;
        }

        public override bool Disconnect(string reason)
        {
            Console.WriteLine(reason);
            clientCommand.HandleDisconnection(this);
            return true;
        }

        public override IPEndPoint GetRemoteEndPoint(string name)
        {
            return RemoteEndPoints.First();
        }

        public override void StartListening()
        {
            
        }

        public override bool SendMessageReceiver(MessageBuffer message,byte channel, DeliveryMethod sendMode)
        {
            clientCommand.HandleSend(this, sendMode, channel, message, outputData);
            message.Dispose();
            return true;
        }
    }
}
