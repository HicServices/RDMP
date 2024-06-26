// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.UI.CommandExecution.Proposals;
using Rdmp.UI.ProjectUI;
using Rdmp.UI.Raceway;
using Tests.Common;

namespace Rdmp.UI.Tests.DesignPatternTests;

public class AllUIsDocumentedTest : UnitTests
{
    [Test]
    public void EveryClassInAppropriateNamespace()
    {
        var Errors = new List<string>();

        Assembly.Load(typeof(RacewayRenderAreaUI).Assembly.FullName);
        Assembly.Load(typeof(ExtractionConfigurationUI).Assembly.FullName);
        // Assembly.Load(typeof(ActivateItems).Assembly.FullName);

        //commands
        Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(ICommandExecution),
            "CommandExecution",
            "CommandExecution.AtomicCommands",
            "CommandExecution.AtomicCommands.PluginCommands",
            "CommandExecution.AtomicCommands.WindowArranging")); //legal namespaces

        Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(IAtomicCommand),
            "CommandExecution",
            "CommandExecution.AtomicCommands",
            "CommandExecution.AtomicCommands.PluginCommands",
            "CommandExecution.AtomicCommands.WindowArranging")); //legal namespaces

        //proposals
        Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(ICommandExecutionProposal), "CommandExecution.Proposals"));

        //menus
        Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(ContextMenuStrip), "Menus"));
        Errors.AddRange(EnforceTypeBelongsInNamespace(typeof(ToolStripMenuItem), "Menus.MenuItems"));

        foreach (var error in Errors)
            Console.WriteLine($"FATAL NAMESPACE ERROR FAILURE:{error}");

        Assert.That(Errors, Is.Empty);
    }

    private string[] _exemptNamespaces =
    {
        "System.ComponentModel.Design",
        "System.Windows.Forms",
        "Rdmp.UI.ScintillaHelper"
    };

    private IEnumerable<string> EnforceTypeBelongsInNamespace(Type InterfaceType, params string[] legalNamespaces)
    {
        return Core.Repositories.MEF.GetAllTypes()
            .Where(InterfaceType.IsAssignableFrom)
            .Where(type => type.Namespace?.Contains(".Tests") == false &&
                           !_exemptNamespaces.Any(e => type.Namespace.Contains(e)) &&
                           !legalNamespaces.Any(ns => type.Namespace.Contains(ns)))
            .Select(type =>
                $"Expected Type '{type.Name}' to be in namespace(s) '{string.Join("' or '", legalNamespaces)}' but it was in '{type.Namespace}'");
    }
}