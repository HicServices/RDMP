// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Linq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Tests.Common;

namespace Rdmp.Core.Tests.DataLoad.Engine.Integration;

internal class ExecutableProcessTaskTests : DatabaseTests
{
    [Test]
    public void TestConstructionFromProcessTaskUsingDatabase()
    {
        const string expectedPath = @"\\a\fake\path.exe";
            
        var loadMetadata = new LoadMetadata(CatalogueRepository);
        var processTask = new ProcessTask(CatalogueRepository, loadMetadata, LoadStage.Mounting)
        {
            Name = "Test process task",
            Path = expectedPath
        };
        processTask.SaveToDatabase();

        var argument = new ProcessTaskArgument(CatalogueRepository, processTask)
        {
            Name = "DatabaseName",
            Value = @"Foo_STAGING"
        };
        argument.SaveToDatabase();

        try
        {
            var args =
                new RuntimeArgumentCollection(processTask.ProcessTaskArguments.Cast<IArgument>().ToArray(), null);

            var runtimeTask = new ExecutableRuntimeTask(processTask, args);
            Assert.AreEqual(expectedPath, runtimeTask.ExeFilepath);
                
            Assert.AreEqual(1, runtimeTask.RuntimeArguments.GetAllArgumentsOfType<string>().Count());

            var dictionaryOfStringArguments = runtimeTask.RuntimeArguments.GetAllArgumentsOfType<string>().ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            Assert.IsNotNull(dictionaryOfStringArguments["DatabaseName"]);
            Assert.AreEqual("Foo_STAGING", dictionaryOfStringArguments["DatabaseName"]);
        }
        finally
        {
            loadMetadata.DeleteInDatabase();
        }
    }
}