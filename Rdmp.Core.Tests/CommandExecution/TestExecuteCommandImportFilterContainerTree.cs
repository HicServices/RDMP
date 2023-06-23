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
        Assert.IsNull(sds.RootFilterContainer);

        //run the command
        var mgr = new ConsoleInputManager(RepositoryLocator,ThrowImmediatelyCheckNotifier.Quiet)
        {
            DisallowInput = true
        };
        var cmd = new ExecuteCommandImportFilterContainerTree(mgr,sds,ac);
            
        Assert.IsFalse(cmd.IsImpossible,cmd.ReasonCommandImpossible);
        cmd.Execute();

        sds.ClearAllInjections();
        Assert.IsNotNull(sds.RootFilterContainer);
        Assert.AreEqual(1, sds.RootFilterContainer.GetFilters().Length);
        Assert.AreEqual("MyFilter", sds.RootFilterContainer.GetFilters()[0].Name);
        Assert.AreEqual("true", sds.RootFilterContainer.GetFilters()[0].WhereSQL);

        Assert.AreNotEqual(filterToImport.GetType(), sds.RootFilterContainer.GetFilters()[0].GetType());
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
        Assert.IsNull(ac.RootFilterContainer);

        //run the command
        var mgr = new ConsoleInputManager(RepositoryLocator,ThrowImmediatelyCheckNotifier.Quiet)
        {
            DisallowInput = true
        };
        var cmd = new ExecuteCommandImportFilterContainerTree(mgr,ac,sds);
            
        Assert.IsFalse(cmd.IsImpossible,cmd.ReasonCommandImpossible);
        cmd.Execute();

        ac.ClearAllInjections();
        Assert.IsNotNull(ac.RootFilterContainer);
        Assert.AreEqual(1, ac.RootFilterContainer.GetFilters().Length);
        Assert.AreEqual("MyFilter", ac.RootFilterContainer.GetFilters()[0].Name);
        Assert.AreEqual("true", ac.RootFilterContainer.GetFilters()[0].WhereSQL);

        Assert.AreNotEqual(filterToImport.GetType(), ac.RootFilterContainer.GetFilters()[0].GetType());
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
        Assert.IsNull(sds.RootFilterContainer);

        //run the command
        var mgr = new ConsoleInputManager(RepositoryLocator,ThrowImmediatelyCheckNotifier.Quiet)
        {
            DisallowInput = true
        };
        var cmd = new ExecuteCommandImportFilterContainerTree(mgr,sds,ac);
            
        Assert.IsFalse(cmd.IsImpossible,cmd.ReasonCommandImpossible);
        cmd.Execute();

        sds.ClearAllInjections();
        Assert.AreEqual(FilterContainerOperation.OR, sds.RootFilterContainer.Operation);
        Assert.IsNotNull(sds.RootFilterContainer);
        Assert.AreEqual(1, sds.RootFilterContainer.GetFilters().Length);

        var subContainers = sds.RootFilterContainer.GetSubContainers();
        Assert.AreEqual(2, subContainers.Length);
        Assert.AreEqual(1, subContainers.Count(e => e.Operation == FilterContainerOperation.AND));
        Assert.AreEqual(1, subContainers.Count(e => e.Operation == FilterContainerOperation.OR));
    }
}