// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CohortCreation.Execution;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Aggregation;
using Tests.Common;

namespace Rdmp.Core.Tests.CommandExecution
{
    public class ExecuteCommandAddNewFilterContainerTests : UnitTests
    {
        [Test]
        public void TestNormalCase()
        {
            var ac = WhenIHaveA<AggregateConfiguration>();
            var cmd = new ExecuteCommandAddNewFilterContainer(new ThrowImmediatelyActivator(RepositoryLocator), ac);
            
            Assert.IsNull(ac.RootFilterContainer_ID);
            
            Assert.IsNull(cmd.ReasonCommandImpossible);
            Assert.IsFalse(cmd.IsImpossible);

            cmd.Execute();
            
            Assert.IsNotNull(ac.RootFilterContainer_ID);
        }
        [Test]
        public void Impossible_BecauseAlreadyHasContainer()
        {
            var ac = WhenIHaveA<AggregateConfiguration>();

            ac.CreateRootContainerIfNotExists();
            Assert.IsNotNull(ac.RootFilterContainer_ID);

            var cmd = new ExecuteCommandAddNewFilterContainer(new ThrowImmediatelyActivator(RepositoryLocator), ac);

            Assert.AreEqual("There is already a root filter container on this object", cmd.ReasonCommandImpossible);
            Assert.IsTrue(cmd.IsImpossible);
        }
        [Test]
        public void Impossible_BecauseAPI()
        {
            var ac = WhenIHaveA<AggregateConfiguration>();

            var c = ac.Catalogue;
            c.Name = PluginCohortCompiler.ApiPrefix + "MyAwesomeAPI";
            c.SaveToDatabase();

            Assert.IsTrue(c.IsApiCall());

            var cmd = new ExecuteCommandAddNewFilterContainer(new ThrowImmediatelyActivator(RepositoryLocator), ac);

            Assert.AreEqual("Filters cannot be added to API calls", cmd.ReasonCommandImpossible);
            Assert.IsTrue(cmd.IsImpossible);
        }
    }
}
