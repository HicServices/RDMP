// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

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
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;
using Rdmp.Core.ReusableLibraryCode.Settings;
using Rdmp.Core.Startup;
using System;

namespace Rdmp.Core.DataLoad.Modules.DataProvider;

/// <summary>
/// Triggers another data load run
/// Checks are run during initial checks of primary data load, and assumed to be correct hwne running post-load
/// </summary>
public class DataLoadDataProvider : IDataProvider, IInteractiveCheckable
{
    [DemandsInitialization("The Data Load you wish to run", Mandatory = true)]
    public LoadMetadata DataLoad { get; set; }


    [DemandsInitialization("When running the chained data load, accept all check changes RDMP wishes to make.", Mandatory = true)]
    public bool AcceptAllCheckNotificationOnRun{ get; set; }

    private IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

    private DleRunner _runner;
    private ICheckNotifier _checker;
    private IBasicActivateItems _activator;
    private IDataLoadEventListener _listener;

    public void Check(ICheckNotifier notifier)
    {
        if (DataLoad is null)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("No LoadMetadata Configured", CheckResult.Fail));
            return;
        }

        var catalogueString = (_activator.RepositoryLocator.CatalogueRepository as TableRepository).ConnectionString;

        var dataExportManagerConnectionString = (_activator.RepositoryLocator.DataExportRepository as TableRepository).ConnectionString;

        LinkedRepositoryProvider newrepo;

        try
        {
            newrepo = new LinkedRepositoryProvider(catalogueString, dataExportManagerConnectionString);
        }
        catch (Exception ex)
        {
            notifier.OnCheckPerformed(new CheckEventArgs($"Unable to create {nameof(LinkedRepositoryProvider)}", CheckResult.Fail, ex));
            return;
        }
        var finder = newrepo;
        var dleOptions = new DleOptions()
        {
            LoadMetadata = DataLoad.ID.ToString(),
            Command = CommandLineActivity.check,
        };
        _runner = new DleRunner(_activator, dleOptions);
        _checker = notifier;
        _listener = new FromCheckNotifierToDataLoadEventListener(notifier);
        var exitCode = _runner.Run(finder, _listener, notifier, new GracefulCancellationToken());
    }

    public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        var dleOptions = new DleOptions()
        {
            LoadMetadata = DataLoad.ID.ToString(),
            Command = CommandLineActivity.check,
        };
        _runner = new DleRunner(_activator, dleOptions);

        _repositoryLocator = job.RepositoryLocator;
        var exitCode = _runner.Run(_repositoryLocator, job, _checker, cancellationToken);
        if (exitCode == 0)
        {
            dleOptions = new DleOptions()
            {
                LoadMetadata = DataLoad.ID.ToString(),
                Command = CommandLineActivity.run,
            };
            _runner = new DleRunner(_activator, dleOptions);

            _repositoryLocator = job.RepositoryLocator;
            exitCode = _runner.Run(_repositoryLocator, job, _checker, cancellationToken);
        }
        return (ExitCodeType)exitCode;
    }

    public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
        _checker = new ToMemoryCheckNotifier(AcceptAllCheckNotificationOnRun?new AcceptAllCheckNotifier():null);

    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public void SetActivator(IBasicActivateItems activator)
    {
        _activator = activator;
    }
}
