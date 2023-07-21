// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.IO;
using System.Linq;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Engine.LoadExecution.Components;

/// <summary>
///     DLE post load disposal operation which deletes all the files in the ForLoading directory.  This is added to the
///     disposal stack and should be executed
///     after the archiving of ForLoading (See ArchiveFiles).
/// </summary>
public class DeleteForLoadingFilesOperation : IDisposeAfterDataLoad
{
    private readonly IDataLoadJob _job;

    public DeleteForLoadingFilesOperation(IDataLoadJob job)
    {
        _job = job;
    }


    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        // We only delete ForLoading files after a successful load
        if (exitCode == ExitCodeType.Success)
        {
            var LoadDirectory = _job.LoadDirectory;

            //if there are no files and there are no directories
            if (!LoadDirectory.ForLoading.GetFiles().Any() && !LoadDirectory.ForLoading.GetDirectories().Any())
            {
                //just skip it but tell user you are skipping it
                postLoadEventListener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        "No files found in ForLoading so not bothering to try and delete."));
                return;
            }

            // Check if the attacher has communicated its intent to handle archiving
            var archivingHandledByAttacher =
                File.Exists(Path.Combine(LoadDirectory.ForLoading.FullName, "attacher_is_handling_archiving"));

            if (!archivingHandledByAttacher && !ArchiveHasBeenCreated())
            {
                postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Error,
                    $"Refusing to delete files in ForLoading: the load has reported success but there is no archive of this dataset (was expecting the archive to be called '{_job.ArchiveFilepath}', check LoadMetadata.CacheArchiveType if the file extension is not what you expect)"));
                return;
            }

            _job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information,
                $"Deleting files in ForLoading ({LoadDirectory.ForLoading.FullName})"));

            if (archivingHandledByAttacher)
            {
                LoadDirectory.ForLoading.EnumerateFiles().Where(info => info.Name != "attacher_is_handling_archiving")
                    .ToList().ForEach(info => info.Delete());
                LoadDirectory.ForLoading.EnumerateDirectories().Where(info => info.Name != "__hidden_from_archiver__")
                    .ToList().ForEach(info => info.Delete(true));
            }
            else
            {
                LoadDirectory.ForLoading.EnumerateFiles().ToList().ForEach(info => info.Delete());
                LoadDirectory.ForLoading.EnumerateDirectories().ToList().ForEach(info => info.Delete(true));
            }
        }
    }

    private bool ArchiveHasBeenCreated()
    {
        return new FileInfo(_job.ArchiveFilepath).Exists;
    }
}