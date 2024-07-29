// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.Mutilators;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Linq;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

/// <summary>
/// This component will run the DQE engine on the associated catalogue after the data load has ran
/// </summary>
public class DQEPostLoadRunner : IMutilateDataTables
{

    public void Check(ICheckNotifier notifier)
    {
    }

    public void Initialize(DiscoveredDatabase dbInfo, LoadStage loadStage)
    {
        if (loadStage != LoadStage.PostLoad)
        {
            throw new Exception("DQL Runner can only be done in the PostLoad stage.");
        }
    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public ExitCodeType Mutilate(IDataLoadJob job)
    {
        var lmdID = job.LoadMetadata.ID;
        var linkage = job.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<LoadMetadataCatalogueLinkage>("LoadMetadataID", lmdID).FirstOrDefault();
        var catalogue = job.RepositoryLocator.CatalogueRepository.GetAllObjectsWhere<Catalogue>("ID", linkage.CatalogueID).FirstOrDefault();
        if (catalogue is null) return ExitCodeType.Success;
        if (catalogue.TimeCoverage_ExtractionInformation_ID == null)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
           "Catalogue does not have a Time Coverage column set. DQE will not be run"));
            return ExitCodeType.Success;
        }

        if (string.IsNullOrWhiteSpace(catalogue.ValidatorXML))
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
            "Catalogue does not have any validation rules configured.DQE will not be run."));
            return ExitCodeType.Success;
        }
        var dqeServer = job.RepositoryLocator.CatalogueRepository.GetDefaultFor(PermissableDefaults.DQE);
        if (dqeServer == null)
        {
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
            "There is no DQE server. DQE will not be run."));
            return ExitCodeType.Success;
        }

        DqeOptions options = new()
        {
            Catalogue = catalogue.ID.ToString(),
            Command = CommandLineActivity.run
        };

        var runner = RunnerFactory.CreateRunner(new ThrowImmediatelyActivator(job.RepositoryLocator), options);
        runner.Run(job.RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, new AcceptAllCheckNotifier(),
                    new GracefulCancellationToken(), job.DataLoadInfo.ID);

        return ExitCodeType.Success;
    }
}
