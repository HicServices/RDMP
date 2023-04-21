// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution;

public class CommandInvokerTests : UnitTests
{
    protected override void OneTimeSetUp()
    {
        base.OneTimeSetUp();

        SetupMEF();
    }

    [Test]
    [Timeout(5000)]
    public void Test_ListSupportedCommands_NoPicker()
    {
        var mgr =  GetActivator();
        var invoker = new CommandInvoker(mgr);
            
        invoker.ExecuteCommand(typeof(ExecuteCommandListSupportedCommands),null);
    }

    [Test]
    [Timeout(5000)]
    public void Test_Delete_WithPicker()
    {
        var mgr = GetActivator();
        var invoker = new CommandInvoker(mgr);

        WhenIHaveA<Catalogue>();

        var picker = new CommandLineObjectPicker(new[] {"Catalogue:*"}, mgr);
        invoker.ExecuteCommand(typeof(ExecuteCommandDelete),picker);
    }

    [Test]
    [Timeout(5000)]
    public void Test_Generic_WithPicker()
    {
        var mgr = GetActivator();
        var invoker = new CommandInvoker(mgr);

        WhenIHaveA<Catalogue>();

        invoker.ExecuteCommand(typeof(GenericTestCommand<DatabaseEntity>),GetPicker("Catalogue:*"));
        invoker.ExecuteCommand(typeof(GenericTestCommand<Type>),GetPicker("Pipeline"));
        invoker.ExecuteCommand(typeof(GenericTestCommand<DiscoveredDatabase,bool>),
            GetPicker(
                "DatabaseType:MicrosoftSqlServer:Name:imaging:Server=localhost\\sqlexpress;Database=master;Trusted_Connection=True;",
                "true"));
            
    }

    private CommandLineObjectPicker GetPicker(params string[] args)
    {
        return new CommandLineObjectPicker(args, GetActivator());
    }

    private class GenericTestCommand<T> : BasicCommandExecution
    {
        private readonly T _arg;
            
        public GenericTestCommand(T a)
        {
            _arg = a;
        }

        public override void Execute()
        {
            base.Execute();
            Console.Write("Arg was " + _arg);
            Assert.IsNotNull(_arg);
        }
    }

    private class GenericTestCommand<T1,T2> : BasicCommandExecution
    {
        private readonly T1 _a;
        private readonly T2 _b;
            
        public GenericTestCommand(T1 a, T2 b)
        {
            _a = a;
            _b = b;
        }

        public override void Execute()
        {
            base.Execute();
            Console.Write("_a was " + _a);
            Console.Write("_b was " + _b);
            Assert.IsNotNull(_a);
            Assert.IsNotNull(_b);
        }
    }
}