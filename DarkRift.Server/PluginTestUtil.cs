/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System.Linq;

namespace DarkRift.Server
{
    /// <summary>
    ///     Utility for testing plugins.
    /// </summary>
    public static class PluginTestUtil
    {
        /// <summary>
        ///     Runs a command on the given plugin.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="command">The command to invoke. Plugin names will be ignored</param>
        /// <param name="plugin">The plugin to invoke the command on.</param>
        public static void RunCommandOn(object sender,string command, ExtendedPluginBase plugin)
        {
            string commandName = CommandEngine.GetCommandName(command).ToLower();
            Command commandObj = plugin.Commands.Single((x) => x.Name.ToLower() == commandName);

            commandObj.Handler.Invoke(sender, CommandEngine.BuildCommandEventArgs(command, commandObj));
        }

        // TODO builder for plugin spawn data?
    }
}
