// Copyright (c) The University of Dundee 2018-2023
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.Tests.CommandExecution;

internal sealed class TestExecuteCommandClearUserSettings : CommandCliTests
{
    [Test]
    public void Test_ClearUserSettings()
    {
        var invoker = GetInvoker();
        var activator = GetActivator();

        UserSettings.Wait5SecondsAfterStartupUI = false;

        invoker.ExecuteCommand(typeof(ExecuteCommandSetUserSetting), new CommandLineObjectPicker(new[] { nameof(UserSettings.Wait5SecondsAfterStartupUI), "true" }, activator));

        Assert.IsTrue(UserSettings.Wait5SecondsAfterStartupUI);
        invoker.ExecuteCommand(typeof(ExecuteCommandSetUserSetting), new CommandLineObjectPicker(new[] { nameof(UserSettings.Wait5SecondsAfterStartupUI), "false" }, activator));
        Assert.IsFalse(UserSettings.Wait5SecondsAfterStartupUI);
        invoker.ExecuteCommand(typeof(ExecuteCommandClearUserSettings), new CommandLineObjectPicker(System.Array.Empty<string>(), activator));

        Assert.IsTrue(UserSettings.Wait5SecondsAfterStartupUI);
    }
}