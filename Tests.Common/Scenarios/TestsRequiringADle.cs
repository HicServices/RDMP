// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using BadMedicine;
using BadMedicine.Datasets;
using FAnsi.Discovery;
using NUnit.Framework;
using Rdmp.Core.CommandLine.Options.Abstracts;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Modules.Attachers;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Progress;
using System;
using System.IO;
using System.Linq;

namespace Tests.Common.Scenarios
{
    /// <summary>
    /// Scenario where you want to run a full DLE load of records into a table
    /// </summary>
    public class TestsRequiringADle:TestsRequiringA
    {
                
        protected int RowsBefore;
        protected int RowsNow => LiveTable.GetRowCount();

        protected LoadMetadata TestLoadMetadata;
        protected Catalogue TestCatalogue;
        protected LoadDirectory LoadDirectory;

        public DiscoveredTable LiveTable { get; private set; }

        [SetUp]

        public void SetUpDle()
        {
            var rootFolder = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
            var subdir = rootFolder.CreateSubdirectory("TestsRequiringADle");
            LoadDirectory = LoadDirectory.CreateDirectoryStructure(rootFolder,subdir.FullName,true);
            
            LiveTable = CreateDataset<Demography>(500,5000,new Random(190));
            LiveTable.CreatePrimaryKey(new DiscoveredColumn[]{
                LiveTable.DiscoverColumn("chi"),
                LiveTable.DiscoverColumn("dtCreated"),
                LiveTable.DiscoverColumn("hb_extract")
                });

            TestCatalogue = Import(LiveTable);
            RowsBefore = 5000;

            TestLoadMetadata = new LoadMetadata (CatalogueRepository,"Loading Test Catalogue");
            TestLoadMetadata.LocationOfFlatFiles = LoadDirectory.RootPath.FullName;
            TestLoadMetadata.SaveToDatabase();


            //make the load load the table
            TestCatalogue.LoadMetadata_ID = TestLoadMetadata.ID;
            TestCatalogue.SaveToDatabase();
            
            var csvProcessTask  = new ProcessTask(CatalogueRepository,TestLoadMetadata,LoadStage.Mounting);
            var args = csvProcessTask.CreateArgumentsForClassIfNotExists<AnySeparatorFileAttacher>();
            csvProcessTask.Path = typeof(AnySeparatorFileAttacher).FullName;
            csvProcessTask.ProcessTaskType = ProcessTaskType.Attacher;
            csvProcessTask.SaveToDatabase();

            var filePattern = args.Single(a=>a.Name == "FilePattern");
            filePattern.SetValue("*.csv");
            filePattern.SaveToDatabase();

            var tableToLoad = args.Single(a=>a.Name == "TableToLoad");
            tableToLoad.SetValue(TestCatalogue.GetTableInfoList(false).Single());
            tableToLoad.SaveToDatabase();

            var separator = args.Single(a=>a.Name == "Separator");
            separator.SetValue(",");
            separator.SaveToDatabase();
            
            var ignoreDataLoadRunIDCol = args.Single(a=>a.Name == "IgnoreColumns");
            ignoreDataLoadRunIDCol.SetValue("hic_dataLoadRunID");
            ignoreDataLoadRunIDCol.SaveToDatabase();

                       
            //Get DleRunner to run pre load checks (includes trigger creation etc)
            var runner = new DleRunner(new DleOptions() { LoadMetadata = TestLoadMetadata.ID,Command = CommandLineActivity.check});
            runner.Run(RepositoryLocator,new ThrowImmediatelyDataLoadEventListener(), new AcceptAllCheckNotifier(), new GracefulCancellationToken());
        }

        /// <summary>
        /// Creates a new demography file ready for loading in the ForLoading directory of the load with the specified number of <paramref name="rows"/>
        /// </summary>
        /// <param name="filename">Filename to generate in ForLoading e.g. "bob.csv" (cannot be relative)</param>
        /// <param name="rows"></param>
        /// <param name="r">Seed random to ensure tests are reproducible</param>
        protected FileInfo CreateFileInForLoading(string filename,int rows, Random r)
        {
            var fi = new FileInfo(Path.Combine(LoadDirectory.ForLoading.FullName,Path.GetFileName(filename)));

            var demog = new Demography(r);
            var people = new PersonCollection();
            people.GeneratePeople(500,r);

            demog.GenerateTestDataFile(people,fi,rows);

            return fi;
        }
               
        public void RunDLE(int timeoutInMilliseconds)
        {
            var runner = new DleRunner(new DleOptions() { LoadMetadata = TestLoadMetadata.ID,Command = CommandLineActivity.run});
            runner.Run(RepositoryLocator,new ThrowImmediatelyDataLoadEventListener(), new ThrowImmediatelyCheckNotifier(), new GracefulCancellationToken());
        }
        
        [TearDown]

        public void DestroyDle()
        {
            TestCatalogue.DeleteInDatabase();
            TestLoadMetadata.DeleteInDatabase();
            LoadDirectory.RootPath.Delete(true);
        }
    }
}
