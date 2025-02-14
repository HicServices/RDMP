// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataExport.DataExtraction.Pipeline;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataExport.DataExtraction.UserPicks;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.Logging;
using Rdmp.Core.QueryBuilding;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using TypeGuesser;

namespace Tests.Common.Scenarios;

[TestFixture]
public class TestsRequiringAnExtractionConfiguration : TestsRequiringACohort
{
    protected ICatalogue _catalogue;
    protected ITableInfo _tableInfo;
    protected ExtractableDataSet _extractableDataSet;
    protected Project _project;
    protected ExtractionConfiguration _configuration;
    protected ExtractionInformation[] _extractionInformations;
    protected List<IColumn> _extractableColumns = new();
    protected ExtractDatasetCommand _request;

    private readonly string _testDatabaseName = TestDatabaseNames.GetConsistentName("ExtractionConfiguration");

    protected bool AllowEmptyExtractions = false;
    protected SelectedDataSets _selectedDataSet;
    protected ColumnInfo[] _columnInfos;

    /// <summary>
    /// Called when pipeline components are created during <see cref="SetupPipeline"/>.  Allows you to make last minute changes to them e.g. before pipeline is executed
    /// </summary>
    protected Action<PipelineComponent> AdjustPipelineComponentDelegate { get; set; }

    protected string ProjectDirectory { get; private set; }

    /// <summary>
    /// The database in which the referenced data is stored, created during <see cref="TestsRequiringACohort.OneTimeSetUp"/>
    /// </summary>
    public DiscoveredDatabase Database { get; private set; }

    [SetUp]
    protected override void SetUp()
    {
        base.SetUp();

        Reset();
    }

    protected void Reset()
    {
        _extractableColumns = new List<IColumn>();

        ProjectDirectory = Path.Combine(TestContext.CurrentContext.WorkDirectory, "TestProject");

        SetupCatalogueConfigurationEtc();

        SetupDataExport();

        _configuration.Cohort_ID = _extractableCohort.ID;
        _configuration.SaveToDatabase();


        _request = new ExtractDatasetCommand(_configuration, _extractableCohort,
            new ExtractableDatasetBundle(_extractableDataSet),
            _extractableColumns, new HICProjectSalt(_project),
            new ExtractionDirectory(ProjectDirectory, _configuration));
    }

    private void SetupDataExport()
    {
        _extractableDataSet = new ExtractableDataSet(DataExportRepository, _catalogue);

        _project = new Project(DataExportRepository, _testDatabaseName)
        {
            ProjectNumber = 1
        };

        Directory.CreateDirectory(ProjectDirectory);
        _project.ExtractionDirectory = ProjectDirectory;

        _project.SaveToDatabase();

        _configuration = new ExtractionConfiguration(DataExportRepository, _project);

        //select the dataset for extraction under this configuration
        _selectedDataSet = new SelectedDataSets(RepositoryLocator.DataExportRepository, _configuration,
            _extractableDataSet, null);

        //select all the columns for extraction
        foreach (var toSelect in _extractionInformations)
        {
            var col = new ExtractableColumn(DataExportRepository, _extractableDataSet, _configuration, toSelect,
                toSelect.Order, toSelect.SelectSQL);

            if (col.GetRuntimeName().Equals("PrivateID"))
                col.IsExtractionIdentifier = true;

            col.SaveToDatabase();

            _extractableColumns.Add(col);
        }
    }

    private void SetupCatalogueConfigurationEtc()
    {
        Database = GetCleanedServer(FAnsi.DatabaseType.MicrosoftSQLServer);

        var dt = new DataTable();
        dt.Columns.Add("PrivateID");
        dt.Columns.Add("Name");
        dt.Columns.Add("DateOfBirth");

        dt.Rows.Add(_cohortKeysGenerated.Keys.First(), "Dave", "2001-01-01");

        var tbl = Database.CreateTable("TestTable", dt,
            new[] { new DatabaseColumnRequest("Name", new DatabaseTypeRequest(typeof(string), 50)) });

        _catalogue = Import(tbl, out _tableInfo, out _columnInfos, out _, out _extractionInformations);

        var _privateID = _extractionInformations.First(e => e.GetRuntimeName().Equals("PrivateID"));
        _privateID.IsExtractionIdentifier = true;
        _privateID.SaveToDatabase();
    }

    protected void ExecuteRunner()
    {
        var pipeline = SetupPipeline();

        var runner = new ExtractionRunner(new ThrowImmediatelyActivator(RepositoryLocator), new ExtractionOptions
        {
            Command = CommandLineActivity.run, ExtractionConfiguration = _configuration.ID.ToString(),
            ExtractGlobals = true, Pipeline = pipeline.ID.ToString()
        });

        var returnCode = runner.Run(
            RepositoryLocator,
            ThrowImmediatelyDataLoadEventListener.Quiet,
            ThrowImmediatelyCheckNotifier.Quiet,
            new GracefulCancellationToken());

        Assert.That(returnCode,Is.EqualTo(0), "Return code from runner was non zero");
    }

    protected void Execute(out ExtractionPipelineUseCase pipelineUseCase,
        out IExecuteDatasetExtractionDestination results, IDataLoadEventListener listener = null)
    {
        listener ??= ThrowImmediatelyDataLoadEventListener.Quiet;

        var d = new DataLoadInfo("Internal", _testDatabaseName, "IgnoreMe", "", true,
            new DiscoveredServer(UnitTestLoggingConnectionString));

        Pipeline pipeline = null;

        //because extractable columns is likely to include chi column, it will be removed from the collection (for a substitution identifier)
        var before = _extractableColumns.ToArray();

        try
        {
            pipeline = SetupPipeline();
            pipelineUseCase = new ExtractionPipelineUseCase(new ThrowImmediatelyActivator(RepositoryLocator),
                _request.Configuration.Project, _request, pipeline, d);

            pipelineUseCase.Execute(listener);

            Assert.That(pipelineUseCase.Source.Request.QueryBuilder.SQL,Is.Not.Empty);

            Assert.That(pipelineUseCase.ExtractCommand.State == ExtractCommandState.Completed);
        }
        finally
        {
            pipeline?.DeleteInDatabase();
        }

        results = pipelineUseCase.Destination;
        _extractableColumns = new List<IColumn>(before);
    }


    protected virtual Pipeline SetupPipeline()
    {
        var repository = RepositoryLocator.CatalogueRepository;
        var pipeline = new Pipeline(repository, "Empty extraction pipeline");

        var component = new PipelineComponent(repository, pipeline, typeof(ExecuteDatasetExtractionFlatFileDestination),
            0, "Destination");
        var arguments = component.CreateArgumentsForClassIfNotExists<ExecuteDatasetExtractionFlatFileDestination>()
            .ToArray();

        if (arguments.Length < 3)
            throw new Exception(
                "Expected only 2 arguments for type ExecuteDatasetExtractionFlatFileDestination, did somebody add another [DemandsInitialization]? if so handle it below");

        arguments.Single(a => a.Name.Equals("DateFormat")).SetValue("yyyy-MM-dd");
        arguments.Single(a => a.Name.Equals("DateFormat")).SaveToDatabase();

        arguments.Single(a => a.Name.Equals("FlatFileType")).SetValue(ExecuteExtractionToFlatFileType.CSV);
        arguments.Single(a => a.Name.Equals("FlatFileType")).SaveToDatabase();

        AdjustPipelineComponentDelegate?.Invoke(component);

        var component2 =
            new PipelineComponent(repository, pipeline, typeof(ExecuteDatasetExtractionSource), -1, "Source");
        var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteDatasetExtractionSource>().ToArray();

        arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(AllowEmptyExtractions);
        arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

        AdjustPipelineComponentDelegate?.Invoke(component2);

        //configure the component as the destination
        pipeline.DestinationPipelineComponent_ID = component.ID;
        pipeline.SourcePipelineComponent_ID = component2.ID;
        pipeline.SaveToDatabase();

        return pipeline;
    }
}