﻿/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;

namespace DarkRift.Server.Plugins.Commands
{
    /// <summary>
    ///     Helper plugin for pretending to receive messages using commands.
    /// </summary>
    internal class MockCommand : Plugin
    {
        public override bool ThreadSafe => true;

        public override Version Version => new Version(1, 0, 0);

        public override Command[] Commands => new Command[]
        {
            new Command("mock", "Mocks message receiving for testing plugins.", "mock <client> <sendMode> <tag> <data> <data> <data> ...", CommandHandler)
        };

        internal override bool Hidden => true;

        public MockCommand(PluginLoadData pluginLoadData) : base(pluginLoadData)
        {
        }

        private void CommandHandler(object sender, CommandEventArgs e)
        {
            if (e.Arguments.Length < 3)
                throw new CommandSyntaxException($"Expected 3 arguments but found {e.Arguments.Length}.");
            
            using (DarkRiftWriter writer = DarkRiftWriter.Create())
            {
                ushort clientID;
                try
                {
                    clientID = ushort.Parse(e.Arguments[0]);
                }
                catch (FormatException)
                {
                    throw new CommandSyntaxException($"Unable to parse the client ID. Expected a number but got '{e.Arguments[0]}'.");
                }

                DeliveryMethod sendMode;
                switch (e.Arguments[1].ToLower())
                {
                    case "unreliable":
                    case "u":
                        sendMode = DeliveryMethod.Unreliable;
                        break;

                    case "reliable":
                    case "r":
                        sendMode = DeliveryMethod.ReliableOrdered;
                        break;

                    default:
                        throw new CommandSyntaxException($"Expected 'unreliable' or 'reliable' but got '{e.Arguments[1]}'.");
                }

                ushort tag;
                try
                {
                    tag = ushort.Parse(e.Arguments[2]);
                }
                catch (FormatException)
                {
                    throw new CommandSyntaxException($"Unable to parse the tag. Expected a number but got '{e.Arguments[2]}'.");
                }

                try
                {
                    IEnumerable<byte> bytes =
                        e.Arguments
                            .Skip(3)
                            .Select((a) => byte.Parse(a));

                    foreach (byte b in bytes)
                        writer.Write(b);
                }
                catch (FormatException)
                {
                    throw new CommandSyntaxException("An argument was unable to be parsed to a number.");
                }
                byte channel = 0;
                using (Message message = Message.Create(tag, writer))
                {
                    try
                    {
                        ((Client)ClientManager[clientID]).HandleIncomingMessage(message, channel, sendMode);
                    }
                    catch (KeyNotFoundException)
                    {
                        Logger.Error("No client with id " + clientID);
                    }
                }
            }
        }
    }
}
