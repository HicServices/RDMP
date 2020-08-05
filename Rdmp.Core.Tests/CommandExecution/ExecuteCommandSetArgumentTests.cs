// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive.Picking;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;

namespace Rdmp.Core.Tests.CommandExecution
{
    class ExecuteCommandSetArgumentTests : CommandCliTests
    {
        [Test]
        public void TestSetArgument_WrongArgCount()
        {
            var picker = new CommandLineObjectPicker(new []{"yyy" },RepositoryLocator);
            var cmd = new ExecuteCommandSetArgument(GetMockActivator().Object,picker);

            Assert.IsTrue(cmd.IsImpossible);
            Assert.AreEqual("Wrong number of parameters supplied to command, expected 3 but got 1",cmd.ReasonCommandImpossible);
        }
        [Test]
        public void TestSetArgument_NotAHost()
        {
            var c = WhenIHaveA<Catalogue>();

            var picker = new CommandLineObjectPicker(new []{$"Catalogue:{c.ID}","fff","yyy" },RepositoryLocator);
            var cmd = new ExecuteCommandSetArgument(GetMockActivator().Object,picker);

            Assert.IsTrue(cmd.IsImpossible);
            Assert.AreEqual("First parameter must be an IArgumentHost",cmd.ReasonCommandImpossible);             
        }

        [Test]
        public void TestSetArgument_NoArgumentFound()
        {
            var pt = WhenIHaveA<ProcessTask>();
            

            var picker = new CommandLineObjectPicker(new []{$"ProcessTask:{pt.ID}","fff","yyy" },RepositoryLocator);
            var cmd = new ExecuteCommandSetArgument(GetMockActivator().Object,picker);

            Assert.IsTrue(cmd.IsImpossible);
            StringAssert.StartsWith("Could not find argument called 'fff' on ",cmd.ReasonCommandImpossible);             
        }
        
        [Test]
        public void TestSetArgument_ArgumentWrongType()
        {
            var pta = WhenIHaveA<ProcessTaskArgument>();
            var pt = pta.ProcessTask;
            
            pta.Name = "fff";
            
            // Argument expects int but is given string value "yyy"
            pta.SetType(typeof(int));

            var picker = new CommandLineObjectPicker(new []{$"ProcessTask:{pt.ID}","fff","yyy" },RepositoryLocator);
            var cmd = new ExecuteCommandSetArgument(GetMockActivator().Object,picker);

            Assert.IsTrue(cmd.IsImpossible);
            StringAssert.StartsWith("Provided value 'yyy' does not match expected Type 'Int32' of ",cmd.ReasonCommandImpossible);        
        }

        
        [Test]
        public void TestSetArgument_Int_Valid()
        {
            var pta = WhenIHaveA<ProcessTaskArgument>();
            var pt = pta.ProcessTask;
            
            pta.Name = "fff";
            pta.SetType(typeof(int));

            Assert.IsNull(pta.Value);

            var picker = new CommandLineObjectPicker(new []{$"ProcessTask:{pt.ID}","fff","5" },RepositoryLocator);
            
            Assert.DoesNotThrow(() =>  GetInvoker().ExecuteCommand(typeof(ExecuteCommandSetArgument),picker));

            Assert.AreEqual(5,pta.GetValueAsSystemType());
        }
    }
}