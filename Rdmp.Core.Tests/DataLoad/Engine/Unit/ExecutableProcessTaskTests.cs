// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Spontaneous;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Arguments;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.Tests.DataLoad.Engine.Unit;

[Category("Unit")]
internal class ExecutableProcessTaskTests
{
    [Test]
    public void TestCreateArgString()
    {
        const string db = "my-db";

        var customArgs = new List<SpontaneouslyInventedArgument>
        {
            new(new MemoryRepository(), "DatabaseName", db)
        };

        var processTask = Mock.Of<IProcessTask>();
        var task = new ExecutableRuntimeTask(processTask, new RuntimeArgumentCollection(customArgs.ToArray(), null));

        var argString = task.CreateArgString();
        const string expectedArgString = $"--database-name={db}";

        Assert.AreEqual(expectedArgString, argString);
    }

    [Test]
    public void TestConstructionFromProcessTask()
    {
        var processTask = Mock.Of<IProcessTask>(pt => pt.Path == "path");

        var runtimeTask = new ExecutableRuntimeTask(processTask, null);
        Assert.AreEqual("path", runtimeTask.ExeFilepath);
    }
}