// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using Rdmp.Core.Caching.Layouts;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.DataLoad.Engine.Checks.Checkers;
using Rdmp.Core.DataLoad.Engine.DatabaseManagement.EntityNaming;
using Rdmp.Core.DataLoad.Engine.LoadProcess;
using Rdmp.Core.ReusableLibraryCode.Checks;

namespace Rdmp.Core.DataLoad.Engine.Checks;

/// <summary>
///     Checks a LoadMetadata it is in a fit state to be executed (does it have primary keys, backup trigger etc).
/// </summary>
public class CheckEntireDataLoadProcess : ICheckable
{
    private readonly HICDatabaseConfiguration _databaseConfiguration;
    private readonly HICLoadConfigurationFlags _loadConfigurationFlags;
    public ILoadMetadata LoadMetadata { get; set; }

    public CheckEntireDataLoadProcess(ILoadMetadata loadMetadata, HICDatabaseConfiguration databaseConfiguration,
        HICLoadConfigurationFlags loadConfigurationFlags)
    {
        _databaseConfiguration = databaseConfiguration;
        _loadConfigurationFlags = loadConfigurationFlags;
        LoadMetadata = loadMetadata;
    }

    public void Check(ICheckNotifier notifier)
    {
        var catalogueLoadChecks =
            new CatalogueLoadChecks(LoadMetadata, _loadConfigurationFlags, _databaseConfiguration);
        var metadataLoggingConfigurationChecks = new MetadataLoggingConfigurationChecks(LoadMetadata);
        var processTaskChecks = new ProcessTaskChecks(LoadMetadata);
        var preExecutionChecks = new PreExecutionChecker(LoadMetadata, _databaseConfiguration);

        //If the load is a progressable (loaded over time) then make sure any associated caches are compatible with the load ProcessTasks
        foreach (var loadProgress in LoadMetadata.LoadProgresses)
        {
            loadProgress.Check(notifier);

            var cp = loadProgress.CacheProgress;
            if (cp != null)
                try
                {
                    var f = new CacheLayoutFactory();
                    CacheLayoutFactory.CreateCacheLayout(loadProgress, LoadMetadata);
                }
                catch (Exception e)
                {
                    notifier.OnCheckPerformed(new CheckEventArgs(
                        $"Load contains a CacheProgress '{cp}' but we were unable to generate an ICacheLayout, see Inner Exception for details",
                        CheckResult.Fail, e));
                }
        }

        //Make sure there are some load tasks and they are valid
        processTaskChecks.Check(notifier);


        try
        {
            metadataLoggingConfigurationChecks.Check(notifier);

            preExecutionChecks.Check(notifier);

            if (!preExecutionChecks.HardFail)
                catalogueLoadChecks.Check(notifier);
        }
        catch (Exception e)
        {
            notifier.OnCheckPerformed(new CheckEventArgs("Entire check process crashed in an unexpected way",
                CheckResult.Fail, e));
        }
    }
}