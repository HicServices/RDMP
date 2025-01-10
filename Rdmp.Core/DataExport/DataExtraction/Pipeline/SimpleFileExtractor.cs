// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.DataExtraction.Commands;
using Rdmp.Core.DataFlowPipeline;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataExtraction.Pipeline;

/// <summary>
/// <para>
/// Component for copying directory trees or top level files from a location on disk to the output directory
/// of a project extraction.  Supports substituting private identifiers for release identifiers in top level
/// file/directory names.
/// </para>
/// <para>IMPORTANT: File extractor operates as part of the 'Extract Globals' section of the extraction pipeline.
/// This means that you must enable globals in the extraction for the component to operate.</para>
/// </summary>
public class SimpleFileExtractor : FileExtractor
{
    [DemandsInitialization("Location of files on disk that should be copied to the output directory", Mandatory = true)]
    public DirectoryInfo LocationOfFiles { get; set; }

    [DemandsInitialization(
        "True if the LocationOfFiles contains a number of directories to be copied.  False if it contains files only (no subdirectories)",
        Mandatory = true, DefaultValue = true)]
    public bool Directories { get; set; }

    [DemandsInitialization(
        "True if there is 1 or more files/folders per patient (if so Pattern must contain $p).  False if there is one arbitrary file/folder that needs copied once only",
        Mandatory = true, DefaultValue = true)]
    public bool PerPatient { get; set; }

    [DemandsInitialization(
        "Expected naming pattern of files to be moved.  If PerPatient is true then this should include the symbol $p to indicate the private identifier value of each patient to be moved e.g. $p.txt.  This symbol will be replaced in the file/path names (but not file body)",
        Mandatory = true)]
    public string Pattern { get; set; } = "$p";

    [DemandsInitialization(@"Directory where files should be put 
$p - Project Extraction Directory (e.g. c:\MyProject\)
$n - Project Number (e.g. 234)
$c - Configuration Extraction Directory  (e.g. c:\MyProject\Extractions\Extr_16)
", Mandatory = true, DefaultValue = "$c\\Files\\")]
    public string OutputDirectoryName { get; set; } = "$c\\Files\\";

    [DemandsInitialization(
        "Determines behaviour when the destination file already exists either due to an old run or cohort private identifier aliases.  Set to true to overwrite or false to crash.",
        DefaultValue = true)]
    public bool Overwrite { get; set; } = true;

    public override void Check(ICheckNotifier notifier)
    {
        base.Check(notifier);

        if (PerPatient && !Pattern.Contains("$p"))
            notifier.OnCheckPerformed(
                new CheckEventArgs($"PerPatient is true but Pattern {Pattern} did not contain token $p",
                    CheckResult.Fail));

        if (!PerPatient && Pattern.Contains("$p"))
            notifier.OnCheckPerformed(new CheckEventArgs(
                $"PerPatient is false but Pattern {Pattern} contains token $p.  This token will never be matched in MoveAll mode",
                CheckResult.Fail));
    }

    protected override void MoveFiles(ExtractGlobalsCommand command, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        if (!LocationOfFiles.Exists)
            throw new Exception($"LocationOfFiles {LocationOfFiles} did not exist");

        var destinationDirectory = GetDestinationDirectory();

        if (!destinationDirectory.Exists)
            destinationDirectory.Create();

        if (PerPatient)
        {
            var cohort = command.Configuration.Cohort;
            var cohortData = cohort.FetchEntireCohort();

            var priv = cohort.GetPrivateIdentifier(true);
            var rel = cohort.GetReleaseIdentifier(true);

            foreach (DataRow r in cohortData.Rows)
                MovePatient(r[priv], r[rel], destinationDirectory, listener, cancellationToken);
        }
        else
        {
            MoveAll(destinationDirectory, listener, cancellationToken);
        }
    }

    /// <summary>
    /// Resolves tokens (if any) in OutputDirectoryName into a single path
    /// </summary>
    /// <returns></returns>
    public DirectoryInfo GetDestinationDirectory()
    {
        var path = OutputDirectoryName;

        if (path.Contains("$p")) path = path.Replace("$p", _command.Project.ExtractionDirectory);
        if (path.Contains("$n")) path = path.Replace("$n", _command.Project.ProjectNumber.ToString());

        if (path.Contains("$c") )
            path = path.Replace("$c",
                new ExtractionDirectory(_command.Project.ExtractionDirectory, _command.Configuration)
                    .ExtractionDirectoryInfo.FullName);

        return new DirectoryInfo(path);
    }

    /// <summary>
    /// Called when <see cref="PerPatient"/> is false.  Called once per extraction
    /// </summary>
    public virtual void MoveAll(DirectoryInfo destinationDirectory, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        var atLeastOne = false;

        var infos = new List<FileSystemInfo>();

        if (Pattern.Contains('*'))
        {
            infos.AddRange(LocationOfFiles.EnumerateFileSystemInfos(Pattern));
        }
        else
        {
            var f = LocationOfFiles.GetFiles()
                .FirstOrDefault(f => f.Name.Equals(Pattern, StringComparison.OrdinalIgnoreCase));

            if (f != null)
                infos.Add(f);

            var d = LocationOfFiles.GetDirectories()
                .FirstOrDefault(d => d.Name.Equals(Pattern, StringComparison.OrdinalIgnoreCase));

            if (d != null)
                infos.Add(d);
        }

        foreach (var e in infos)
        {
            if (Directories && e is DirectoryInfo dir)
            {
                var dest = Path.Combine(destinationDirectory.FullName, dir.Name);

                // Recursively copy all files from input path to destination path
                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        $"Copying directory '{e.FullName}' to '{dest}'"));
                CopyFolder(e.FullName, dest);
                atLeastOne = true;
            }

            if (!Directories && e is FileInfo f)
            {
                var dest = Path.Combine(destinationDirectory.FullName, f.Name);
                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information, $"Copying file '{f.FullName}' to '{dest}'"));
                File.Copy(f.FullName, dest, Overwrite);
                atLeastOne = true;
            }
        }

        if (!atLeastOne)
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    $"No {(Directories ? "Directories" : "Files")} were found matching Pattern {Pattern} in {LocationOfFiles.FullName}"));
    }

    /// <summary>
    /// Called when <see cref="PerPatient"/> is true.  Called once per private identifier.  Note that it is possible for 2 private identifiers to map to the same release identifier - be careful
    /// </summary>
    public virtual void MovePatient(object privateIdentifier, object releaseIdentifier,
        DirectoryInfo destinationDirectory, IDataLoadEventListener listener,
        GracefulCancellationToken cancellationToken)
    {
        var atLeastOne = false;

        if (privateIdentifier == DBNull.Value || string.IsNullOrWhiteSpace(privateIdentifier?.ToString()))
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    "Skipped NULL private identifier found in cohort when trying to copy files"));
            return;
        }

        if (releaseIdentifier == DBNull.Value || string.IsNullOrWhiteSpace(releaseIdentifier?.ToString()))
        {
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Error,
                    $"Found NULL release identifier in cohort when trying to copy files.  This is not allowed as it breaks file name substitutions.  Private identifier was {privateIdentifier}"));
            return;
        }

        // What we will be writing into the file/path names in place of the private identifier
        var releaseSub = UsefulStuff.RemoveIllegalFilenameCharacters(releaseIdentifier.ToString());

        var patternAfterTokenInsertion = Pattern.Replace("$p", privateIdentifier.ToString());

        foreach (var e in LocationOfFiles.EnumerateFileSystemInfos(patternAfterTokenInsertion))
        {
            if (Directories && e is DirectoryInfo dir)
            {
                var dest = Path.Combine(
                    destinationDirectory.FullName,
                    dir.Name.Replace(privateIdentifier.ToString(), releaseSub));

                // Recursively copy all files from input path to destination path
                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information,
                        $"Copying directory '{e.FullName}' to '{dest}'"));
                CopyFolder(e.FullName, dest);
                atLeastOne = true;
            }

            if (!Directories && e is FileInfo f)
            {
                var dest = Path.Combine(
                    destinationDirectory.FullName,
                    f.Name.Replace(privateIdentifier.ToString(), releaseSub));

                listener.OnNotify(this,
                    new NotifyEventArgs(ProgressEventType.Information, $"Copying file '{f.FullName}' to '{dest}'"));
                File.Copy(f.FullName, dest, Overwrite);
                atLeastOne = true;
            }
        }

        if (!atLeastOne)
            listener.OnNotify(this,
                new NotifyEventArgs(ProgressEventType.Warning,
                    $"No {(Directories ? "Directories" : "Files")} were found matching Pattern {patternAfterTokenInsertion} in {LocationOfFiles.FullName}.  For private identifier '{privateIdentifier}'"));
    }

    protected void CopyFolder(string sourceFolder, string destFolder)
    {
        if (!Directory.Exists(destFolder))
            Directory.CreateDirectory(destFolder);
        var files = Directory.GetFiles(sourceFolder);
        foreach (var file in files)
        {
            var name = Path.GetFileName(file);
            var dest = Path.Combine(destFolder, name);
            File.Copy(file, dest, Overwrite);
        }

        var folders = Directory.GetDirectories(sourceFolder);
        foreach (var folder in folders)
        {
            var name = Path.GetFileName(folder);
            var dest = Path.Combine(destFolder, name);
            CopyFolder(folder, dest);
        }
    }
}