// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data.Aggregation;

namespace Rdmp.Core.Tests.CommandExecution
{
    /// <summary>
    /// Tests for <see cref="ExecuteCommandCreateNewFilter">
    /// </summary>
    class ExecuteCommandCreateNewFilterCliTests : CommandCliTests
    {
        [Test]
        public void TestNewFilterForAggregate()
        {
            var ac = WhenIHaveA<AggregateConfiguration>();

            // has no container to start with (no filters)
            Assert.IsNull(ac.RootFilterContainer_ID);
            Run("CreateNewFilter",$"{nameof(AggregateConfiguration)}:{ac.ID}");

            Assert.IsNotNull(ac.RootFilterContainer_ID,"Should now have a container");
            Assert.AreEqual(1,ac.RootFilterContainer.GetFilters().Count(),"Expected a single new filter");
        }
    }
}
