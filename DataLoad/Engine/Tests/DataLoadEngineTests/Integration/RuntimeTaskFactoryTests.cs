// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using CatalogueLibrary;
using CatalogueLibrary.Data.DataLoad;
using DataLoadEngine.LoadExecution.Components.Arguments;
using DataLoadEngine.LoadExecution.Components.Runtime;
using NUnit.Framework;
using Rhino.Mocks;
using Tests.Common;

namespace DataLoadEngineTests.Integration
{
    public class RuntimeTaskFactoryTests : DatabaseTests
    {
        [Test]
        [TestCase("LoadModules.Generic.Web.WebFileDownloader")]
        [TestCase("LoadModules.Generic.DataProvider.FlatFileManipulation.ExcelToCSVFilesConverter")]
        public void RuntimeTaskFactoryTest(string className)
        {

            var lmd = new LoadMetadata(CatalogueRepository);
            var task = new ProcessTask(CatalogueRepository, lmd,LoadStage.GetFiles);

            var f = new RuntimeTaskFactory(CatalogueRepository);

            task.Path = className;
            task.ProcessTaskType = ProcessTaskType.DataProvider;
            task.SaveToDatabase();
            
            try
            {
                var ex = Assert.Throws<Exception>(() => f.Create(task, new StageArgs(LoadStage.AdjustRaw, DiscoveredDatabaseICanCreateRandomTablesIn, MockRepository.GenerateMock<IHICProjectDirectory>())));
                Assert.IsTrue(ex.InnerException.Message.Contains("marked with DemandsInitialization but no corresponding argument was provided in ArgumentCollection"));
            }
            finally 
            {
                task.DeleteInDatabase();
                lmd.DeleteInDatabase();
            }


        }
    }
}