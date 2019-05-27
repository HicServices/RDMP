// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using Rdmp.Core.CommandLine.Options;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation;
using ReusableLibraryCode.CommandExecution.AtomicCommands;
using ReusableLibraryCode.Icons.IconProvision;

namespace Rdmp.UI.CommandExecution.AtomicCommands.Automation
{
    public class ExecuteCommandRunDetached : AutomationCommandExecution, IAtomicCommand
    {
        public ExecuteCommandRunDetached(IActivateItems activator, Func<RDMPCommandLineOptions> commandGetter) : base(activator,commandGetter)
        {
            if(!File.Exists(AutomationServiceExecutable))
                SetImpossible(AutomationServiceExecutable + " not found.");
        }

        public override void Execute()
        {
            base.Execute();

            string command = GetCommandText();

            var psi = new ProcessStartInfo(AutomationServiceExecutable);
            psi.Arguments = command.Substring(AutomationServiceExecutable.Length);
            Process.Start(psi);
        }

        public override string GetCommandHelp()
        {
            return "Runs the activity in a seperate console process.";
        }

        public override Image GetImage(IIconProvider iconProvider)
        {
            return CatalogueIcons.Exe;
        }
    }
}