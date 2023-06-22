// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using NUnit.Framework;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.DataExport.Checks;
using Rdmp.Core.DataExport.DataExtraction.Pipeline.Sources;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Tests.Common.Scenarios;

namespace Rdmp.Core.Tests.DataExport;

public class ProjectChecksTestsComplex : TestsRequiringAnExtractionConfiguration
{
    [Test]
    public void CheckBasicConfiguration()
    {
        new ProjectChecker(new ThrowImmediatelyActivator(RepositoryLocator),_project).Check(ThrowImmediatelyCheckNotifier.QuietPicky);
    }

    [Test]
    public void DatasetIsDisabled()
    {
        _extractableDataSet.DisableExtraction = true;
        _extractableDataSet.SaveToDatabase();

        //checking should fail
        var exception = Assert.Throws<Exception>(() => new ProjectChecker(new ThrowImmediatelyActivator(RepositoryLocator), _project).Check(ThrowImmediatelyCheckNotifier.QuietPicky));
        Assert.AreEqual("Dataset TestTable is set to DisableExtraction=true, probably someone doesn't want you extracting this dataset at the moment", exception.Message);

        //but if the user goes ahead and executes the extraction that should fail too
        var source = new ExecuteDatasetExtractionSource();
        source.PreInitialize(_request, ThrowImmediatelyDataLoadEventListener.Quiet);
        var exception2 = Assert.Throws<Exception>(() => source.GetChunk(ThrowImmediatelyDataLoadEventListener.Quiet, new GracefulCancellationToken()));

        Assert.AreEqual("Cannot extract TestTable because DisableExtraction is set to true", exception2?.Message);
    }
}