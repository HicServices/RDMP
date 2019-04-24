// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Linq;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CatalogueLibrary.Data;
using Rdmp.Core.CatalogueLibrary.Data.DataLoad;
using Rdmp.Core.CatalogueLibrary.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.Data.DataTables;
using Rdmp.Core.DataExport.ExtractionTime;
using Rdmp.Core.DataExport.ExtractionTime.Commands;
using Rdmp.Core.DataExport.ExtractionTime.ExtractionPipeline;
using Rdmp.Core.DataExport.ExtractionTime.ExtractionPipeline.Destinations;
using Rdmp.Core.DataExport.ExtractionTime.ExtractionPipeline.Sources;
using Rdmp.Core.DataExport.ExtractionTime.UserPicks;
using Tests.Common;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport.DataExtraction
{
    public class ExecuteFullExtractionToDatabaseMSSqlDestinationTest :TestsRequiringAnExtractionConfiguration
    {
        private ExternalDatabaseServer _extractionServer;
        
        private readonly string _expectedTableName = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest_TestTable";
        
        [Test]
        public void SQLServerDestination()
        {
            DiscoveredDatabase dbToExtractTo = null;

            var ci = new CatalogueItem(CatalogueRepository, _catalogue, "YearOfBirth");
            var columnToTransform = _columnInfos.Single(c=>c.GetRuntimeName().Equals("DateOfBirth",StringComparison.CurrentCultureIgnoreCase));

            string transform = "YEAR(" + columnToTransform.Name + ")";

            var ei = new ExtractionInformation(CatalogueRepository, ci, columnToTransform, transform);
            ei.Alias = "YearOfBirth";
            ei.ExtractionCategory = ExtractionCategory.Core;
            ei.SaveToDatabase();
            
            //make it part of the ExtractionConfiguration
            var newColumn = new ExtractableColumn(DataExportRepository, _selectedDataSet.ExtractableDataSet, (ExtractionConfiguration)_selectedDataSet.ExtractionConfiguration, ei, 0, ei.SelectSQL);
            newColumn.Alias = ei.Alias;
            newColumn.SaveToDatabase();

            _extractableColumns.Add(newColumn);
            
            //recreate the extraction command so it gets updated with the new column too.
            _request = new ExtractDatasetCommand(_configuration, _extractableCohort, new ExtractableDatasetBundle(_extractableDataSet),
                _extractableColumns, new HICProjectSalt(_project), 
                new ExtractionDirectory(@"C:\temp\", _configuration));

            try
            {
                _configuration.Name = "ExecuteFullExtractionToDatabaseMSSqlDestinationTest";
                _configuration.SaveToDatabase();
                
                ExtractionPipelineUseCase execute;
                IExecuteDatasetExtractionDestination result;

                var dbname = TestDatabaseNames.GetConsistentName(_project.Name + "_" + _project.ProjectNumber);
                dbToExtractTo = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.ExpectDatabase(dbname);
                if (dbToExtractTo.Exists())
                    dbToExtractTo.Drop();

                base.Execute(out execute, out result);

                var destinationTable = dbToExtractTo.ExpectTable(_expectedTableName);
                Assert.IsTrue(destinationTable.Exists());

                var dt = destinationTable.GetDataTable();

                Assert.AreEqual(1, dt.Rows.Count);
                Assert.AreEqual(_cohortKeysGenerated[_cohortKeysGenerated.Keys.First()].Trim(),dt.Rows[0]["ReleaseID"]);
                Assert.AreEqual(new DateTime(2001,1,1), dt.Rows[0]["DateOfBirth"]);
                Assert.AreEqual(2001, dt.Rows[0]["YearOfBirth"]);

                Assert.AreEqual(columnToTransform.Data_type, destinationTable.DiscoverColumn("DateOfBirth").DataType.SQLType);
                Assert.AreEqual("int",destinationTable.DiscoverColumn("YearOfBirth").DataType.SQLType);

            }
            finally
            {
                if(_extractionServer != null)
                    _extractionServer.DeleteInDatabase();

                if(dbToExtractTo != null)
                    dbToExtractTo.Drop();
            }
        }
        
        
        protected override Pipeline SetupPipeline()
        {
            //create a target server pointer
            _extractionServer = new ExternalDatabaseServer(CatalogueRepository, "myserver",null);
            _extractionServer.Server = DiscoveredServerICanCreateRandomDatabasesAndTablesOn.Name;
            _extractionServer.SaveToDatabase();

            //create a pipeline
            var pipeline = new Pipeline(CatalogueRepository, "Empty extraction pipeline");
            
            //set the destination pipeline
            var component = new PipelineComponent(CatalogueRepository, pipeline, typeof(ExecuteFullExtractionToDatabaseMSSql), 0, "MS SQL Destination");
            var destinationArguments = component.CreateArgumentsForClassIfNotExists<ExecuteFullExtractionToDatabaseMSSql>().ToList();
            IArgument argumentServer = destinationArguments.Single(a => a.Name == "TargetDatabaseServer");
            IArgument argumentDbNamePattern = destinationArguments.Single(a => a.Name == "DatabaseNamingPattern");
            IArgument argumentTblNamePattern = destinationArguments.Single(a => a.Name == "TableNamingPattern");

            Assert.AreEqual("TargetDatabaseServer", argumentServer.Name);
            argumentServer.SetValue(_extractionServer);
            argumentServer.SaveToDatabase();
            argumentDbNamePattern.SetValue(TestDatabaseNames.Prefix + "$p_$n");
            argumentDbNamePattern.SaveToDatabase();
            argumentTblNamePattern.SetValue("$c_$d");
            argumentTblNamePattern.SaveToDatabase();
            
            var component2 = new PipelineComponent(CatalogueRepository, pipeline, typeof(ExecuteCrossServerDatasetExtractionSource), -1, "Source");
            var arguments2 = component2.CreateArgumentsForClassIfNotExists<ExecuteCrossServerDatasetExtractionSource>().ToArray();
            arguments2.Single(a=>a.Name.Equals("AllowEmptyExtractions")).SetValue(false);
            arguments2.Single(a => a.Name.Equals("AllowEmptyExtractions")).SaveToDatabase();

            //configure the component as the destination
            pipeline.DestinationPipelineComponent_ID = component.ID;
            pipeline.SourcePipelineComponent_ID = component2.ID;
            pipeline.SaveToDatabase();

            return pipeline;
        }
    }
}
