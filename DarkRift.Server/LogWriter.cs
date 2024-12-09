/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DarkRift.Server
{
    /// <summary>
    ///     Base class for all log writers.
    /// </summary>
    public abstract class LogWriter : PluginBase
    {
        /// <summary>
        ///     Creates a new LogWriter.
        /// </summary>
        /// <param name="logWriterLoadData">The data to start the log writer with.</param>
        public LogWriter(LogWriterLoadData logWriterLoadData)
            : base(logWriterLoadData)
        {

        }

        /// <summary>
        ///     Writes an event to this log writer.
        /// </summary>
        /// <param name="args">The message to log.</param>
        public abstract void WriteEvent(WriteEventArgs args);
    }
}
