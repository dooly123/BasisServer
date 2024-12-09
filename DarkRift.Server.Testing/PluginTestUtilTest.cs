/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at https://mozilla.org/MPL/2.0/.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace DarkRift.Server.Testing
{
    [TestClass]
    public class PluginTestUtilTest
    {

        [TestMethod]
        public void TestRunCommandOnWhenCommandDoesNotExists()
        {
            // GIVEN a mock event handler
            Mock<EventHandler<CommandEventArgs>> mockHandler = new Mock<EventHandler<CommandEventArgs>>();
            // AND a command with a different name
            Command command = new Command("not-this", "xyz", "abc", mockHandler.Object);

            // AND a mock plugin with that command
            Mock<ExtendedPluginBase> mockPlugin = new Mock<ExtendedPluginBase>(new PluginLoadData(null, null, null, null, (Logger)null, null));
            mockPlugin.Setup(p => p.Commands).Returns(new Command[] { command });

            // THEN an exception is thrown
            // TODO DR3 this is a poor choice of exception
            Assert.ThrowsException<InvalidOperationException>(() =>
            {
                // WHEN the command is run on the plugin through the test util

                PluginTestUtil.RunCommandOn(this,"my-command with arguments -and=many -f -l -a -g -s", mockPlugin.Object);
            });
        }
    }
}
