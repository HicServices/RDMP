// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;

namespace Rdmp.Core.Tests.CommandExecution;

internal class TestExecuteCommandDescribeCommand : CommandCliTests
{
    /// <summary>
    /// Asserts that the help text <paramref name="forCommand"/> matches your <paramref name="expectedHelp"/> text
    /// </summary>
    /// <param name="expectedHelp"></param>
    /// <param name="forCommand"></param>
    private void AssertHelpIs(string expectedHelp, Type forCommand)
    {
        var activator = GetMockActivator();

        var cmd = new ExecuteCommandDescribe(activator,
            new CommandLineObjectPicker(new[] { forCommand.Name }, activator));
        Assert.IsFalse(cmd.IsImpossible, cmd.ReasonCommandImpossible);

        cmd.Execute();
        StringAssert.Contains(expectedHelp, cmd.HelpShown);
    }

    [Test]
    public void Test_DescribeDeleteCommand()
    {
        AssertHelpIs(@" Delete <deletables> <deleteMany> 

PARAMETERS:
deletables	IDeleteable[]	The object(s) you want to delete.  If multiple you must set deleteMany to true",
            typeof(ExecuteCommandDelete));
    }


    [Test]
    public void Test_ImportTableInfo_CommandHelp()
    {
        AssertHelpIs(
            @" ImportTableInfo <table> <createCatalogue> 

PARAMETERS:
table	DiscoveredTable	The table or view you want to reference from RDMP.  See PickTable for syntax
createCatalogue	Boolean	True to create a Catalogue as well as a TableInfo"
            , typeof(ExecuteCommandImportTableInfo));
    }

    [Test]
    public void Test_DescribeCommand_ExecuteCommandNewObject()
    {
        AssertHelpIs(@" NewObject <type> <arg1> <arg2> <etc>

PARAMETERS:
type	The object to create e.g. Catalogue
args    Dynamic list of values to satisfy the types constructor", typeof(ExecuteCommandNewObject));
    }

    [Test]
    public void Test_DescribeCommand_ExecuteCommandSetArgument()
    {
        AssertHelpIs(@" SetArgument <component> <argName> <argValue>

PARAMETERS:
component    Module to set value on e.g. ProcessTask:1
argName Name of an argument to set on the component e.g. Retry
argValue    New value for argument e.g. Null, True, Catalogue:5 etc
", typeof(ExecuteCommandSetArgument));
    }
}
