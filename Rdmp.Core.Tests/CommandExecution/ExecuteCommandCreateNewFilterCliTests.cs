// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.DataExport.Data;

namespace Rdmp.Core.Tests.CommandExecution;

/// <summary>
/// Tests for <see cref="ExecuteCommandCreateNewFilter" />
/// </summary>
internal class ExecuteCommandCreateNewFilterCliTests : CommandCliTests
{
    [Test]
    public void TestNewFilterForAggregate()
    {
        var ac = WhenIHaveA<AggregateConfiguration>();

        // has no container to start with (no filters)
        Assert.That(ac.RootFilterContainer_ID, Is.Null);
        Run("CreateNewFilter", $"{nameof(AggregateConfiguration)}:{ac.ID}");

        Assert.Multiple(() =>
        {
            Assert.That(ac.RootFilterContainer_ID, Is.Not.Null, "Should now have a container");
            Assert.That(ac.RootFilterContainer.GetFilters(), Has.Length.EqualTo(1), "Expected a single new filter");
        });
    }

    [Test]
    public void TestNewFilterForExtractionConfiguration()
    {
        var sds = WhenIHaveA<SelectedDataSets>();

        // has no container to start with (no filters)
        Assert.That(sds.RootFilterContainer_ID, Is.Null);
        Run("CreateNewFilter", $"{nameof(SelectedDataSets)}:{sds.ID}");

        Assert.Multiple(() =>
        {
            Assert.That(sds.RootFilterContainer_ID, Is.Not.Null, "Should now have a container");
            Assert.That(sds.RootFilterContainer.GetFilters(), Has.Length.EqualTo(1), "Expected a single new filter");
        });
    }

    [Test]
    public void TestNewFilterForCatalogue()
    {
        var ei = WhenIHaveA<ExtractionInformation>();

        // no Catalogue level filters
        Assert.That(ei.ExtractionFilters, Is.Empty);
        Run("CreateNewFilter", $"{nameof(ExtractionInformation)}:{ei.ID}", "My cool filter", "hb='t'");

        var f = ei.ExtractionFilters.Single();
        Assert.Multiple(() =>
        {
            Assert.That(f.Name, Is.EqualTo("My cool filter"));
            Assert.That(f.WhereSQL, Is.EqualTo("hb='t'"));
        });
    }
}