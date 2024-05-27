// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Data;
using System.IO;
using System.Linq;
using SynthEHR.Datasets;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Destinations;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction;

public class ExecuteFullExtractionToDatabaseMSSqlDestinationTest : TestsRequiringAnExtractionConfiguration
{
    private ExternalDatabaseServer _extractionServer;

    private const string _expectedTableName = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest_TestTable";
    private ColumnInfo _columnToTransform;
    private Pipeline _pipeline;

    [Test]
    public void SQLServerDestination()
    {
        DiscoveredDatabase dbToExtractTo = null;

        var ci = new CatalogueItem(CatalogueRepository, _catalogue, "YearOfBirth");
        _columnToTransform = _columnInfos.Single(c =>
            c.GetRuntimeName().Equals("DateOfBirth", StringComparison.CurrentCultureIgnoreCase));

        var transform = $"YEAR({_columnToTransform.Name})";


        if (_catalogue.GetAllExtractionInformation(ExtractionCategory.Any)
            .All(ei => ei.GetRuntimeName() != "YearOfBirth"))
        {
            var ei = new ExtractionInformation(CatalogueRepository, ci, _columnToTransform, transform)
            {
                Alias = "YearOfBirth",
                ExtractionCategory = ExtractionCategory.Core
            };
            ei.SaveToDatabase();

            //make it part of the ExtractionConfiguration
            var newColumn = new ExtractableColumn(DataExportRepository, _selectedDataSet.ExtractableDataSet,
                (ExtractionConfiguration)_selectedDataSet.ExtractionConfiguration, ei, 0, ei.SelectSQL)
            {
                Alias = ei.Alias
            };
            newColumn.SaveToDatabase();

            _extractableColumns.Add(newColumn);
        }

        CreateLookupsEtc();

        try
        {
            _configuration.Name = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest";
            _configuration.SaveToDatabase();

            var dbname = TestDatabaseNames.GetConsistentName($"{_project.Name}_{_project.ProjectNumber}");
            dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
            if (dbToExtractTo.Exists())
                dbToExtractTo.Drop();

            ExecuteRunner();

            var destinationTable = dbToExtractTo.ExpectTable(_expectedTableName);
            Assert.That(destinationTable.Exists());

            var dt = destinationTable.GetDataTable();

            Assert.That(dt.Rows, Has.Count.EqualTo(1));
            Assert.Multiple(() =>
            {
                Assert.That(dt.Rows[0]["ReleaseID"], Is.EqualTo(_cohortKeysGenerated[_cohortKeysGenerated.Keys.First()].Trim()));
                Assert.That(dt.Rows[0]["DateOfBirth"], Is.EqualTo(new DateTime(2001, 1, 1)));
                Assert.That(dt.Rows[0]["YearOfBirth"], Is.EqualTo(2001));

                Assert.That(destinationTable.DiscoverColumn("DateOfBirth").DataType.SQLType, Is.EqualTo(_columnToTransform.Data_type));
                Assert.That(destinationTable.DiscoverColumn("YearOfBirth").DataType.SQLType, Is.EqualTo("int"));
            });

            AssertLookupsEtcExist(dbToExtractTo);
        }
        finally
        {
            if (dbToExtractTo?.Exists() == true)
                dbToExtractTo.Drop();

            _pipeline?.DeleteInDatabase();
        }
    }

    private static void AssertLookupsEtcExist(DiscoveredDatabase dbToExtractTo)
    {
        Assert.Multiple(() =>
        {
            Assert.That(dbToExtractTo.ExpectTable("ExecuteFullExtractionToDatabaseMSSqlDestinationTest_TestTable_Biochem")
                    .Exists());
            Assert.That(dbToExtractTo.ExpectTable("ExecuteFullExtractionToDatabaseMSSqlDestinationTest_Globals_Hosp")
                .Exists());
            Assert.That(dbToExtractTo.ExpectTable("ExecuteFullExtractionToDatabaseMSSqlDestinationTest_TestTable_z_fff")
                .Exists());
        });
    }

    private void CreateLookupsEtc()
    {
        //an extractable file
        var filename = Path.Combine(TestContext.CurrentContext.WorkDirectory, "bob.txt");

        File.WriteAllText(filename, "fishfishfish");
        var doc = new SupportingDocument(CatalogueRepository, _catalogue, "bob")
        {
            URL = new Uri($"file://{filename}"),
            Extractable = true
        };
        doc.SaveToDatabase();

        //an extractable global file (comes out regardless of datasets)
        var filename2 = Path.Combine(TestContext.CurrentContext.WorkDirectory, "bob2.txt");

        File.WriteAllText(filename2, "fishfishfish2");
        var doc2 = new SupportingDocument(CatalogueRepository, _catalogue, "bob2")
        {
            URL = new Uri($"file://{filename2}"),
            Extractable = true,
            IsGlobal = true
        };
        doc2.SaveToDatabase();

        //an supplemental table in the database (not linked against cohort)
        var tbl = CreateDataset<Biochemistry>(Database, 500, 1000, new Random(50));

        var sql = new SupportingSQLTable(CatalogueRepository, _catalogue, "Biochem");
        var server = new ExternalDatabaseServer(CatalogueRepository, "myserver", null);
        server.SetProperties(tbl.Database);
        sql.ExternalDatabaseServer_ID = server.ID;
        sql.SQL = $"SELECT * FROM {tbl.GetFullyQualifiedName()}";
        sql.Extractable = true;
        sql.SaveToDatabase();


        //an supplemental (global) table in the database (not linked against cohort)
        var tbl2 = CreateDataset<HospitalAdmissions>(Database, 500, 1000, new Random(50));

        var sql2 = new SupportingSQLTable(CatalogueRepository, _catalogue, "Hosp")
        {
            ExternalDatabaseServer_ID = server.ID,
            SQL = $"SELECT * FROM {tbl2.GetFullyQualifiedName()}",
            Extractable = true,
            IsGlobal = true
        };
        sql2.SaveToDatabase();


        var dtLookup = new DataTable();
        dtLookup.Columns.Add("C");
        dtLookup.Columns.Add("D");

        dtLookup.Rows.Add("F", "Female");
        dtLookup.Rows.Add("M", "Male");
        dtLookup.Rows.Add("NB", "Non Binary");

        var lookupTbl = tbl2.Database.CreateTable("z_fff", dtLookup);

        Import(lookupTbl, out _, out var columnInfos);

        _ = new Lookup(CatalogueRepository, columnInfos[0],
            _columnToTransform,
            columnInfos[1],
            ExtractionJoinType.Left, null);

        //we need a CatalogueItem for the description in order to pick SetUp the Lookup as associated with the Catalogue
        var ci = new CatalogueItem(CatalogueRepository, _catalogue, "SomeDesc")
        {
            ColumnInfo_ID = columnInfos[1].ID
        };
        ci.SaveToDatabase();
    }


    protected override Pipeline SetupPipeline()
    {
        //create a target server pointer
        _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver", null)
        {
            Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name,
            Username = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitUsernameIfAny,
            Password = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExplicitPasswordIfAny
        };
        _extractionServer.SaveToDatabase();

        //create a pipeline
        _pipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline");

        //set the destination pipeline
        var component = new PipelineComponent(CatalogueRepository, _pipeline,
            typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
        var destinationArguments = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>()
            .ToList();
        var argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
        var argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
        var argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");

        Assert.That(argumentServer.Name, Is.EqualTo("TargetDatabaseServer"));
        argumentServer.SetValue(_extractionServer);
        argumentServer.SaveToDatabase();
        argumentDbNamePattern.SetValue($"{TestDatabaseNames.Prefix}$p_$n");
        argumentDbNamePattern.SaveToDatabase();
        argumentTblNamePattern.SetValue("$c_$d");
        argumentTblNamePattern.SaveToDatabase();
        AdjustPipelineComponentDelegate?.Invoke(component);

        var component2 = new PipelineComponent(CatalogueRepository, _pipeline,
            typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
        var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>()
            .ToArray();
        arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
        arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();
        AdjustPipelineComponentDelegate?.Invoke(component2);

        //configure the component as the destination
        _pipeline.DestinationPipelineComponent_ID = component.ID;
        _pipeline.SourcePipelineComponent_ID = component2.ID;
        _pipeline.SaveToDatabase();

        return _pipeline;
    }
}