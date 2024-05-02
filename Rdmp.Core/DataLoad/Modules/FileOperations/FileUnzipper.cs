// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FAnsi.Discovery;
using Rdmp.Core.Curation;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.DataLoad.Engine.DataProvider;
using Rdmp.Core.DataLoad.Engine.Job;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataLoad.Modules.FileOperations;

/// <summary>
///     load component which Unzips files in ForLoading
///     <para>
///         Searches the forLoading directory for *.zip and unzips all entries in all zip archives found.  If the
///         forLoading directory already contains a file with the same name then
///         it is overwritten (unless the file size is also the same in which case the entry is skipped)
///     </para>
/// </summary>
public class FileUnzipper : IPluginDataProvider
{
    [DemandsInitialization(
        "Leave blank to extract all zip archives or populate with a REGULAR EXPRESSION to extract only specific zip filenames e.g. \"nhs_readv2*\\.zip\" - notice the escaped dot to match the dot exactly")]
    public Regex ZipArchivePattern { get; set; }

    [DemandsInitialization(
        "Leave blank to extract all files or populate with a REGULAR EXPRESSION to extract only specific files e.g. \".*\\.txt\" to extract all .txt files - notice how the pattern is a regular expression, so the dot must be escaped to prevent matching anything")]
    public Regex ZipEntryPattern { get; set; }

    private readonly List<FileInfo> _entriesUnzipped = new();

    public void Initialize(ILoadDirectory directory, DiscoveredDatabase dbInfo)
    {
    }

    public ExitCodeType Fetch(IDataLoadJob job, GracefulCancellationToken cancellationToken)
    {
        foreach (var fileInfo in job.LoadDirectory.ForLoading.GetFiles("*.zip"))
        {
            //do it as regex rather than in GetFiles above because that method probably doesn't do regex
            if (ZipArchivePattern != null && !string.IsNullOrWhiteSpace(ZipArchivePattern.ToString()) &&
                !ZipArchivePattern.IsMatch(fileInfo.Name)) continue;
            using var zipFile = ZipFile.Open(fileInfo.FullName, ZipArchiveMode.Read);
            //fire event telling user we found some files in the zip file
            job.OnNotify(this, new NotifyEventArgs(ProgressEventType.Information, zipFile.Entries.Aggregate(
                "Identified the following zip entries:", (s, n) =>
                    $"{n.Name},").TrimEnd(',')));


            foreach (var entry in zipFile.Entries)
            {
                if (entry.Length == 0)
                    continue;

                //if we are matching everything or we are matching on a regex that matches the entry name
                if (ZipEntryPattern != null && !string.IsNullOrWhiteSpace(ZipEntryPattern.ToString()) &&
                    !ZipEntryPattern.IsMatch(entry.Name)) continue;
                //extract it
                var existingFile = job.LoadDirectory.ForLoading.GetFiles(entry.Name).FirstOrDefault();

                if (existingFile != null && existingFile.Length == entry.Length)
                    continue;

                UnzipWithEvents(entry, job.LoadDirectory, job);
            }
        }

        return ExitCodeType.Success;
    }

    private void UnzipWithEvents(ZipArchiveEntry entry, ILoadDirectory destination, IDataLoadJob job)
    {
        //create a task
        var entryDestination = Path.Combine(destination.ForLoading.FullName, entry.Name);
        using var unzipJob = Task.Factory.StartNew(() => entry.ExtractToFile(entryDestination, true));

        //create a stopwatch to time how long bits take
        var s = Stopwatch.StartNew();
        var f = new FileInfo(entryDestination);
        _entriesUnzipped.Add(f);

        //monitor it
        while (!unzipJob.IsCompleted)
        {
            unzipJob.Wait(1000);
            if (f.Exists)
                job.OnProgress(this,
                    new ProgressEventArgs(entryDestination,
                        new ProgressMeasurement((int)(f.Length / 1000), ProgressType.Kilobytes), s.Elapsed));
        }
    }

    public string GetDescription()
    {
        throw new NotImplementedException();
    }

    public static IDataProvider Clone()
    {
        return new FileUnzipper();
    }


    public void LoadCompletedSoDispose(ExitCodeType exitCode, IDataLoadEventListener postLoadEventListener)
    {
        if (exitCode is ExitCodeType.Success or ExitCodeType.OperationNotRequired)
        {
            var countOfEntriesThatDisappeared = _entriesUnzipped.Count(e => !e.Exists);

            if (countOfEntriesThatDisappeared != 0)
                postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                    $"{countOfEntriesThatDisappeared} of {_entriesUnzipped.Count} entries were created by {GetType().Name} during unzip phase but had disappeared at cleanup time - following successful data load"));

            //cleanup required
            foreach (var f in _entriesUnzipped.Where(e => e.Exists))
                try
                {
                    f.Delete();
                }
                catch (Exception e)
                {
                    postLoadEventListener.OnNotify(this, new NotifyEventArgs(ProgressEventType.Warning,
                        $"Could not delete file {f.FullName}", e));
                }
        }
    }


    public void Check(ICheckNotifier notifier)
    {
        if (ZipArchivePattern != null)
            notifier.OnCheckPerformed(new CheckEventArgs($"Found ZipArchivePattern {ZipArchivePattern}",
                CheckResult.Success));

        if (ZipEntryPattern != null)
            notifier.OnCheckPerformed(new CheckEventArgs($"Found ZipEntryPattern {ZipEntryPattern}",
                CheckResult.Success));
    }
}