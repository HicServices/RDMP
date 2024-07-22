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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Modules.Mutilators;

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

        //This does everything, we just want the new data load bits

        DqeOptions options = new DqeOptions()
        {
            Catalogue = catalogue.ID.ToString(),
            Command = CommandLineActivity.run
        };
        var runner = RunnerFactory.CreateRunner(new ThrowImmediatelyActivator(job.RepositoryLocator), options);
        runner.Run(job.RepositoryLocator, ThrowImmediatelyDataLoadEventListener.Quiet, new AcceptAllCheckNotifier(),
                    new GracefulCancellationToken());//does the whole catalogue, rather than just the new stuff

        return ExitCodeType.Success;
    }
}
