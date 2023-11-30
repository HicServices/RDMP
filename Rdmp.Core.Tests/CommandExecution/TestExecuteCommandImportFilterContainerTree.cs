// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using NUnit.Framework;
using Rdmp.Core.CommandExecution.AtomicCommands;
using Rdmp.Core.CommandLine.Interactive;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.DataExport.Data;
using System.Linq;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.Tests.CommandExecution;

internal class TestExecuteCommandImportFilterContainerTree : CommandInvokerTests
{
    [Test]
    public void TestImportTree_FromCohortIdentificationConfiguration_ToSelectedDatasets()
    {
        var sds = WhenIHaveA<SelectedDataSets>();

        var cata = sds.ExtractableDataSet.Catalogue;

        var cic = new CohortIdentificationConfiguration(Repository, "my cic");
        cic.CreateRootContainerIfNotExists();

        var ac = new AggregateConfiguration(Repository, cata, "myagg");
        ac.CreateRootContainerIfNotExists();
        cic.RootCohortAggregateContainer.AddChild(ac, 1);

        var filterToImport = new AggregateFilter(Repository, "MyFilter") { WhereSQL = "true" };
        ac.RootFilterContainer.AddChild(filterToImport);

        //there should be no root container
        Assert.That(sds.RootFilterContainer, Is.Null);

        //run the command
        var mgr = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
        {
            DisallowInput = true
        };
        var cmd = new ExecuteCommandImportFilterContainerTree(mgr, sds, ac);

        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
        cmd.Execute();

        sds.ClearAllInjections();
        Assert.That(sds.RootFilterContainer, Is.Not.Null);
        Assert.That(sds.RootFilterContainer.GetFilters(), Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(sds.RootFilterContainer.GetFilters()[0].Name, Is.EqualTo("MyFilter"));
            Assert.That(sds.RootFilterContainer.GetFilters()[0].WhereSQL, Is.EqualTo("true"));

            Assert.That(sds.RootFilterContainer.GetFilters()[0].GetType(), Is.Not.EqualTo(filterToImport.GetType()));
        });
    }

    [Test]
    public void TestImportTree_FromSelectedDatasets_ToCohortIdentificationConfiguration()
    {
        // Import From Selected Dataset
        var sds = WhenIHaveA<SelectedDataSets>();
        sds.CreateRootContainerIfNotExists();

        var filterToImport =
            new DeployedExtractionFilter(Repository, "MyFilter", (FilterContainer)sds.RootFilterContainer)
                { WhereSQL = "true" };
        filterToImport.SaveToDatabase();

        var cata = sds.ExtractableDataSet.Catalogue;

        // Into an Aggregate Configuration
        var cic = new CohortIdentificationConfiguration(Repository, "my cic");
        cic.CreateRootContainerIfNotExists();
        var ac = new AggregateConfiguration(Repository, cata, "myagg");

        cic.RootCohortAggregateContainer.AddChild(ac, 1);

        //there should be no root container
        Assert.That(ac.RootFilterContainer, Is.Null);

        //run the command
        var mgr = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
        {
            DisallowInput = true
        };
        var cmd = new ExecuteCommandImportFilterContainerTree(mgr, ac, sds);

        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
        cmd.Execute();

        ac.ClearAllInjections();
        Assert.That(ac.RootFilterContainer, Is.Not.Null);
        Assert.That(ac.RootFilterContainer.GetFilters(), Has.Length.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(ac.RootFilterContainer.GetFilters()[0].Name, Is.EqualTo("MyFilter"));
            Assert.That(ac.RootFilterContainer.GetFilters()[0].WhereSQL, Is.EqualTo("true"));

            Assert.That(ac.RootFilterContainer.GetFilters()[0].GetType(), Is.Not.EqualTo(filterToImport.GetType()));
        });
    }


    [Test]
    public void TestImportTree_FromCohortIdentificationConfiguration_ToSelectedDatasets_PreserveOperation()
    {
        var sds = WhenIHaveA<SelectedDataSets>();

        var cata = sds.ExtractableDataSet.Catalogue;

        var cic = new CohortIdentificationConfiguration(Repository, "my cic");
        cic.CreateRootContainerIfNotExists();

        var ac = new AggregateConfiguration(Repository, cata, "myagg");
        ac.CreateRootContainerIfNotExists();
        cic.RootCohortAggregateContainer.AddChild(ac, 1);

        var filterToImport = new AggregateFilter(Repository, "MyFilter") { WhereSQL = "true" };
        var root = ac.RootFilterContainer;
        root.AddChild(filterToImport);
        root.Operation = FilterContainerOperation.OR;
        root.SaveToDatabase();

        // add 2 subcontainers, these should also get cloned and should preserve the Operation correctly
        root.AddChild(new AggregateFilterContainer(Repository, FilterContainerOperation.AND));
        root.AddChild(new AggregateFilterContainer(Repository, FilterContainerOperation.OR));

        //there should be no root container
        Assert.That(sds.RootFilterContainer, Is.Null);

        //run the command
        var mgr = new ConsoleInputManager(RepositoryLocator, ThrowImmediatelyCheckNotifier.Quiet)
        {
            DisallowInput = true
        };
        var cmd = new ExecuteCommandImportFilterContainerTree(mgr, sds, ac);

        Assert.That(cmd.IsImpossible, Is.False, cmd.ReasonCommandImpossible);
        cmd.Execute();

        sds.ClearAllInjections();
        Assert.Multiple(() =>
        {
            Assert.That(sds.RootFilterContainer.Operation, Is.EqualTo(FilterContainerOperation.OR));
            Assert.That(sds.RootFilterContainer, Is.Not.Null);
        });
        Assert.That(sds.RootFilterContainer.GetFilters(), Has.Length.EqualTo(1));

        var subContainers = sds.RootFilterContainer.GetSubContainers();
        Assert.That(subContainers, Has.Length.EqualTo(2));
        Assert.Multiple(() =>
        {
            Assert.That(subContainers.Count(e => e.Operation == FilterContainerOperation.AND), Is.EqualTo(1));
            Assert.That(subContainers.Count(e => e.Operation == FilterContainerOperation.OR), Is.EqualTo(1));
        });
    }
}