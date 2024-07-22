using FAnsi.Discovery;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.CommandLine.Options;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.DataLoad.Engine.LoadExecution.Components.Runtime;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.Core.Startup;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Rdmp.Core.DataLoad.Modules.DataProvider;

public class DataLoadDataProvider : IDataProvider
{
    [DemandsInitialization("The Data Load you wish to run", Mandatory = true)]
    public LoadMetadata DataLoad { get; set; }

    private IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

    private DleRunner _runner;
    private ICheckNotifier _checker;
    private IDataLoadEventListener _listener;

    public void Check(ICheckNotifier notifier)
    {
        if (DataLoad is null)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("No LoadMetadata Configured", CheckResult.Fail));
        }
        var catalogueString = UserSettings.CatalogueConnectionString;

        //user may have a DataExportManager
        var dataExportManagerConnectionString = UserSettings.DataExportConnectionString;

        LinkedRepositoryProvider newrepo;

        try
        {
            newrepo = new LinkedRepositoryProvider(catalogueString, dataExportManagerConnectionString);
        }
        catch (Exception ex)
        {
            throw new CorruptRepositoryConnectionDetailsException(
                $"Unable to create {nameof(LinkedRepositoryProvider)}", ex);
        }
        var finder = newrepo;
        var dleOptions = new DleOptions()
        {
            LoadMetadata = DataLoad.ID.ToString(),
            Command = CommandLineActivity.check,
        };
        _runner = new DleRunner(dleOptions);
        _checker = notifier;
        _listener = new FromCheckNotifierToDataLoadEventListener(notifier);
        var exitCode = _runner.Run(finder, _listener, notifier, new GracefulCancellationToken());

        //var y = _checker.Messages.Where(m => m.ProposedFix is not null).ToList();

        //foreach (var log in _checker.Messages)
        //{
        //    var x = notifier.OnCheckPerformed(log);
        //    Console.WriteLine(x);
        //}
    }

    public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        var dleOptions = new DleOptions()
        {
            LoadMetadata = DataLoad.ID.ToString(),
            Command = CommandLineActivity.run,
        };
        _runner = new DleRunner(dleOptions);

        _repositoryLocator = job.RepositoryLocator;
        var exitCode = _runner.Run(_repositoryLocator, job, _checker, cancellationToken);
        //foreach (var log in _listener.EventsReceivedBySender)
        //{
        //    foreach (var args in log.Value)
        //    {
        //        job.OnNotify(log.Key.ToString(), args);
        //    }
        //}
        return (ExitCodeType)exitCode;
    }

    public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
        Console.WriteLine("a");
        //_listener = new ToMemoryDataLoadEventListener(false);
        //_checker = new ToMemoryCheckNotifier();

    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }
}
