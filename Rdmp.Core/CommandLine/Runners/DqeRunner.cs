// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Triggers;
using Rdmp.Core.DataQualityEngine.Reports;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.CommandLine.Runners;

internal class DqeRunner : Runner
{
    private readonly DqeOptions _options;

    public DqeRunner(DqeOptions options)
    {
        _options = options;
    }

    public override int Run(IRDMPPlatformRepositoryServiceLocator repositoryLocator, IDataLoadEventListener listener,
        ICheckNotifier checkNotifier, GracefulCancellationToken token, int? dataLoadId = null)
    {
        var catalogue = GetObjectFromCommandLineString<Catalogue>(repositoryLocator, _options.Catalogue);
        var report = new CatalogueConstraintReport(catalogue, SpecialFieldNames.DataLoadRunID);

        switch (_options.Command)
        {
            case CommandLineActivity.run:
                if (dataLoadId is not null)
                    report.UpdateReport(catalogue, (int)dataLoadId, listener, token.AbortToken);
                else
                    report.GenerateReport(catalogue, listener, token.AbortToken);
                return 0;

            case CommandLineActivity.check:
                report.Check(checkNotifier);
                return 0;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}