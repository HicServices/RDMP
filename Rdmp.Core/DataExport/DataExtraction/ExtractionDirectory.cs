// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataExtraction;

/// <summary>
///     The target directory for a given ExtractionConfiguration on a given day.  This is where linked anonymised project
///     extracts will appear when
///     an ExtractionConfiguration is executed.  It is also the location where the Release Engine will pick them up from
///     when it bundles together a
///     release package.
/// </summary>
public class ExtractionDirectory : IExtractionDirectory
{
    public const string EXTRACTION_SUB_FOLDER_NAME = "Extractions";
    public const string STANDARD_EXTRACTION_PREFIX = "Extr_";
    public const string GLOBALS_DATA_NAME = "Globals";
    public const string CUSTOM_COHORT_DATA_FOLDER_NAME = "CohortCustomData";
    public const string MASTER_DATA_FOLDER_NAME = "MasterData";
    public const string METADATA_FOLDER_NAME = "MetadataShareDefs";

    public DirectoryInfo ExtractionDirectoryInfo { get; }

    public ExtractionDirectory(string rootExtractionDirectory, IExtractionConfiguration configuration)
        : this(rootExtractionDirectory, configuration, DateTime.Now)
    {
    }

    private ExtractionDirectory(string rootExtractionDirectory, IExtractionConfiguration configuration,
        DateTime extractionDate)
    {
        if (string.IsNullOrWhiteSpace(rootExtractionDirectory))
            throw new NullReferenceException("Extraction Directory not set");

        if (!rootExtractionDirectory.StartsWith("\\"))
            if (!Directory.Exists(rootExtractionDirectory))
                throw new DirectoryNotFoundException($"Root directory \"{rootExtractionDirectory}\" does not exist");

        var root = new DirectoryInfo(Path.Combine(rootExtractionDirectory, EXTRACTION_SUB_FOLDER_NAME));
        if (!root.Exists)
            root.Create();

        var subdirectoryName = GetExtractionDirectoryPrefix(configuration);

        ExtractionDirectoryInfo = Directory.Exists(Path.Combine(root.FullName, subdirectoryName))
            ? new DirectoryInfo(Path.Combine(root.FullName, subdirectoryName))
            : root.CreateSubdirectory(subdirectoryName);
    }

    public static string GetExtractionDirectoryPrefix(IExtractionConfiguration configuration)
    {
        return STANDARD_EXTRACTION_PREFIX + configuration.ID;
    }

    public DirectoryInfo GetDirectoryForDataset(IExtractableDataSet dataset)
    {
        if (dataset.ToString().Equals(CUSTOM_COHORT_DATA_FOLDER_NAME))
            throw new Exception(
                $"You cannot call a dataset '{CUSTOM_COHORT_DATA_FOLDER_NAME}' because this string is reserved for cohort custom data the system spits out itself");

        if (!Catalogue.IsAcceptableName(dataset.Catalogue.Name, out var reason))
            throw new NotSupportedException(
                $"Cannot extract dataset {dataset} because it points at Catalogue with an invalid name, name is invalid because:{reason}");

        var datasetDirectory = dataset.ToString();
        try
        {
            return ExtractionDirectoryInfo.CreateSubdirectory(datasetDirectory);
        }
        catch (Exception e)
        {
            throw new Exception(
                $"Could not create a directory called '{datasetDirectory}' as a subfolder of Project extraction directory {ExtractionDirectoryInfo.Root}",
                e);
        }
    }

    public DirectoryInfo GetGlobalsDirectory()
    {
        return ExtractionDirectoryInfo.CreateSubdirectory(GLOBALS_DATA_NAME);
    }

    public static bool IsOwnerOf(IExtractionConfiguration configuration, DirectoryInfo directory)
    {
        //they passed a root directory like c:\bob?
        if (directory.Parent == null)
            return false;

        //The configuration number matches but directory isn't the currently configured Project extraction directory
        var p = configuration.Project;

        return directory.Parent.FullName == Path.Combine(p.ExtractionDirectory, EXTRACTION_SUB_FOLDER_NAME) &&
               directory.Name.StartsWith(STANDARD_EXTRACTION_PREFIX + configuration.ID);
    }

    public DirectoryInfo GetDirectoryForCohortCustomData()
    {
        return ExtractionDirectoryInfo.CreateSubdirectory(CUSTOM_COHORT_DATA_FOLDER_NAME);
    }

    public DirectoryInfo GetDirectoryForMasterData()
    {
        return ExtractionDirectoryInfo.CreateSubdirectory(MASTER_DATA_FOLDER_NAME);
    }

    public static void CleanupExtractionDirectory(object sender, string extractionDirectory,
        IEnumerable<IExtractionConfiguration> configurations, IDataLoadEventListener listener)
    {
        var projectExtractionDirectory =
            new DirectoryInfo(Path.Combine(extractionDirectory, EXTRACTION_SUB_FOLDER_NAME));
        var directoriesToDelete = new List<DirectoryInfo>();
        var filesToDelete = new List<FileInfo>();

        foreach (var extractionConfiguration in configurations)
        {
            var config = extractionConfiguration;
            var directoryInfos = projectExtractionDirectory.GetDirectories().Where(d => IsOwnerOf(config, d));

            foreach (var toCleanup in directoryInfos)
                AddDirectoryToCleanupList(toCleanup, true, directoriesToDelete, filesToDelete);
        }

        foreach (var fileInfo in filesToDelete)
        {
            listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Information,
                $"Deleting: {fileInfo.FullName}"));
            try
            {
                fileInfo.Delete();
            }
            catch (Exception e)
            {
                listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Error,
                    $"Error deleting: {fileInfo.FullName}", e));
            }
        }

        foreach (var directoryInfo in directoriesToDelete)
        {
            listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Information,
                $"Recursively deleting folder: {directoryInfo.FullName}"));
            try
            {
                directoryInfo.Delete(true);
            }
            catch (Exception e)
            {
                listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Error,
                    $"Error deleting: {directoryInfo.FullName}", e));
            }
        }
    }

    private static void AddDirectoryToCleanupList(DirectoryInfo toCleanup, bool isRoot,
        List<DirectoryInfo> directoriesToDelete, List<FileInfo> filesToDelete)
    {
        //only add root folders to the delete queue
        if (isRoot)
            if (!directoriesToDelete.Any(dir =>
                    dir.FullName.Equals(toCleanup.FullName))) //don't add the same folder twice
                directoriesToDelete.Add(toCleanup);

        filesToDelete.AddRange(toCleanup.EnumerateFiles());

        foreach (var dir in toCleanup.EnumerateDirectories())
            AddDirectoryToCleanupList(dir, false, directoriesToDelete, filesToDelete);
    }
}