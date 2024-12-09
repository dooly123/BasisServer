/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using DarkRift.Server.Metrics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Base class for plugins with additional privileges.
    /// </summary>
    public abstract class ExtendedPluginBase : PluginBase
    {
        /// <summary>
        ///     Is this plugin able to handle multithreaded events?
        /// </summary>
        /// <remarks>
        ///     Enabling this option allows DarkRift to send messages to your plugin from multiple threads simultaneously, 
        ///     greatly increasing performance. Do not enable this unless you are confident that you understand 
        ///     multithreading else you will find yourself with a variety of unfriendly problems to fix!
        /// </remarks>
        public abstract bool ThreadSafe { get; }

        /// <summary>
        ///     The commands the plugin has.
        /// </summary>
        /// <remarks>
        ///     This is an array of commands that can be executed by this plugin and will be searched through when the 
        ///     command is executed. Changes to this array will be reflected instantly by the command system.
        /// </remarks>
        public virtual Command[] Commands => new Command[0];

        /// <summary>
        /// The server's metrics manager.
        /// </summary>
        public IMetricsManager MetricsManager { get; }

        /// <summary>
        ///     Metrics collector for the plugin.
        /// </summary>
        protected MetricsCollector MetricsCollector { get; }

        /// <summary>
        ///     Constructor taking extended load data.
        /// </summary>
        /// <param name="pluginLoadData">The load data for the plugins.</param>
        public ExtendedPluginBase(ExtendedPluginBaseLoadData pluginLoadData) : base(pluginLoadData)
        {

            MetricsManager = pluginLoadData.MetricsManager;
            MetricsCollector= pluginLoadData.MetricsCollector;
        }

        /// <summary>
        ///     Method that will be called when the server and all plugins have loaded.
        /// </summary>
        /// <param name="args">The details of the load.</param>
        protected internal virtual void Loaded(LoadedEventArgs args)
        { }

        /// <summary>
        ///     Method that will be called when the plugin is installed.
        /// </summary>
        /// <param name="args">The details of the installation.</param>
        protected internal virtual void Install(InstallEventArgs args)
        { }

        /// <summary>
        ///     Method that will be called when the plugin is upgraded.
        /// </summary>
        /// <param name="args">The details of the upgrade.</param>
        protected internal virtual void Upgrade(UpgradeEventArgs args)
        { }
    }
}
