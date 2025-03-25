// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using FAnsi;
using NSubstitute;
using NUnit.Framework;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

public class RuntimeTaskFactoryTests : DatabaseTests
{
    [Test]
    [TestCase("Rdmp.Core.DataLoad.Modules.Web.WebFileDownloader")]
    [TestCase("Rdmp.Core.DataLoad.Modules.DataProvider.FlatFileManipulation.ExcelToCSVFilesConverter")]
    public void RuntimeTaskFactoryTest(string className)
    {
        var lmd = new LoadMetadata(CatalogueRepository);
        var task = new ProcessTask(CatalogueRepository, lmd, LoadStage.GetFiles);

        task.Path = className;
        task.ProcessTaskType = ProcessTaskType.DataProvider;
        task.SaveToDatabase();

        try
        {
            var ex = Assert.Throws<Exception>(() => RuntimeTaskFactory.Create(task,
                new StageArgs(LoadStage.AdjustRaw, GetCleanedServer(DatabaseType.MicrosoftSQLServer),
                    Substitute.For<ILoadDirectory>())));
            Assert.That(ex.InnerException.Message,
                Does.Contain(
                    "marked with DemandsInitialization but no corresponding argument was provided in ArgumentCollection"));
        }
        finally
        {
            task.DeleteInDatabase();
            lmd.DeleteInDatabase();
        }
    }
}