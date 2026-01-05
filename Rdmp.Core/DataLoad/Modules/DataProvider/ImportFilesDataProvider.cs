// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.DataProvider;

/// <summary>
/// Data load component that copies files into the ForLoading directory from the remote directory (that match the file pattern e.g. *.csv).  A good use case
/// for this is if you want to expose a network location as a share for data providers to send you files to but want the DLE to take a copy of the files at
/// runtime for the purposes of loading.
///
/// <para>Optionally deletes files from the fetch location if the data load is successful</para>
/// </summary>
public class ImportFilesDataProvider : IPluginDataProvider
{
    private FileInfo[] _files;

    [DemandsInitialization("The path you want to copy files from", Mandatory = true)]
    public string DirectoryPath { get; set; }

    [DemandsInitialization("The file pattern to match on the DirectoryPath", Mandatory = true)]
    public string FilePattern { get; set; }

    [DemandsInitialization(
        "If true then at the end of a successful data load the files that were originally matched and copied to forLoading will be deleted from the remote DirectoryPath.  Note that only the files copied will be deleted, any new files that appear during the load will not be deleted")]
    public bool DeleteFilesOnsuccessfulLoad { get; set; }

    public void Check(ICheckNotifier notifier)
    {
        if (string.IsNullOrWhiteSpace(DirectoryPath))
            notifier.OnCheckPerformed(new CheckEventArgs(
                "No DirectoryPath has been specified, this should be set to the remote folder you want to copy files out of",
                CheckResult.Fail));

        if (string.IsNullOrWhiteSpace(FilePattern))
            notifier.OnCheckPerformed(new CheckEventArgs(
                "No FilePattern has been specified, this should be a pattern that matches files in the remote folder you want to copy files out of e.g. *.*",
                CheckResult.Fail));
        notifier.OnCheckPerformed(new DirectoryInfo(DirectoryPath).Exists
            ? new CheckEventArgs($"Path {DirectoryPath} was found", CheckResult.Success)
            : new CheckEventArgs($"Path {DirectoryPath} was not found", CheckResult.Fail));
    }

    public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
    }

    public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        _files = new DirectoryInfo(DirectoryPath).GetFiles(FilePattern);

        foreach (var f in _files)
        {
            var to = Path.Combine(job.LoadDirectory.ForLoading.FullName, f.Name);
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Copying file {f.FullName} to directory {to}"));
            f.CopyTo(to, true);
        }

        return ExitCodeType.Success;
    }

    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventsListener)
    {
        if (exitCode == ExitCodeType.Success)
            if (DeleteFilesOnsuccessfulLoad)
                foreach (var f in _files)
                {
                    postLoadEventsListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                        $"About to delete {f.FullName}"));
                    f.Delete();
                }
    }
}