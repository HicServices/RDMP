// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data.ImportExport;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataProvider;

/// <summary>
///     Data Provider Process Task for DLE which will look for *.sd files and import them into RDMP
/// </summary>
public class ShareDefinitionImporter : IPluginDataProvider
{
    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
    }

    public void Check(ICheckNotifier notifier)
    {
    }

    public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
    }

    public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        var imported = 0;
        try
        {
            var shareManager = new ShareManager(job.RepositoryLocator);

            foreach (var shareDefinitionFile in job.LoadDirectory.ForLoading.EnumerateFiles("*.sd"))
            {
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Found '{shareDefinitionFile.Name}'"));
                using (var stream = File.Open(shareDefinitionFile.FullName, FileMode.Open))
                {
                    shareManager.ImportSharedObject(stream);
                }

                imported++;
                job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                    $"Imported '{shareDefinitionFile.Name}' Successfully"));
            }
        }
        catch (SharingException ex)
        {
            job.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning, "Error occurred importing ShareDefinitions", ex));
        }

        job.OnNotify(this, new NotifyEventArgs(
            imported == 0 ? ProgressEventType.Warning : ProgressEventType.Information,
            $"Imported {imported} ShareDefinition files"));

        return ExitCodeType.Success;
    }
}