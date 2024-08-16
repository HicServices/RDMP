// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.Reports.ExtractionTime;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.Progress;

namespace Rdmp.Core.DataExport.DataRelease;

/// <summary>
/// Facilitates the release of anonymous project extracts to researchers including the generation of the release documents / Audit.  This typically involves
/// collecting all the extracted files (csv data extracts, docx metadata documents, custom data files and supporting documents etc) and moving them into a single
/// release directory followed by deleting all redundant extraction artifacts.
/// 
/// <para>In order to DoRelease you will need to evaluate the environment and each ExtractionConfiguration to confirm they are in a releasable state (extracted files
/// match current configuration, ticketing system says that the project has governance approval for release etc).  </para>
/// </summary>
public class ReleaseEngine
{
    protected readonly IDataLoadEventListener _listener;
    protected readonly IDataExportRepository _repository;
    public Project Project { get; private set; }
    public bool ReleaseSuccessful { get; protected set; }
    public List<IExtractionConfiguration> ConfigurationsReleased { get; private set; }
    public Dictionary<IExtractionConfiguration, List<ReleasePotential>> ConfigurationsToRelease { get; protected set; }

    public ReleaseEngineSettings ReleaseSettings { get; set; }

    public ReleaseAudit ReleaseAudit { get; set; }

    public ReleaseEngine(Project project, ReleaseEngineSettings settings, IDataLoadEventListener listener,
        ReleaseAudit releaseAudit)
    {
        _repository = project.DataExportRepository;
        Project = project;
        ReleaseSuccessful = false;
        ConfigurationsReleased = new List<IExtractionConfiguration>();

        ReleaseSettings = settings ?? new ReleaseEngineSettings();
        _listener = listener ?? new ToMemoryDataLoadEventListener(false);

        ReleaseAudit = releaseAudit;
    }

    public virtual void DoRelease(Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease,
        Dictionary<IExtractionConfiguration, ReleaseEnvironmentPotential> environments, bool isPatch)
    {
        ConfigurationsToRelease = toRelease;

        using (var sw = PrepareAuditFile())
        {
            ReleaseGlobalFolder();

            // Audit Global Folder if there are any
            if (ReleaseAudit.SourceGlobalFolder != null)
            {
                AuditDirectoryCreation(ReleaseAudit.SourceGlobalFolder.FullName, sw, 0);

                foreach (var fileInfo in ReleaseAudit.SourceGlobalFolder.GetFiles())
                    AuditFileCreation(fileInfo.Name, sw, 1);
            }

            ReleaseAllExtractionConfigurations(toRelease, sw, environments, isPatch);

            sw.Flush();
            sw.Close();
        }

        ReleaseSuccessful = true;
    }

    protected virtual StreamWriter PrepareAuditFile()
    {
        var sw = new StreamWriter(Path.Combine(ReleaseAudit.ReleaseFolder.FullName, "contents.txt"));

        sw.WriteLine($"----------Details Of Release---------:{DateTime.Now}");
        sw.WriteLine($"ProjectName:{Project.Name}");
        sw.WriteLine($"ProjectNumber:{Project.ProjectNumber}");
        sw.WriteLine($"Project.ID:{Project.ID}");
        sw.WriteLine($"ThisFileWasCreated:{DateTime.Now}");

        sw.WriteLine($"----------Contents Of Directory---------:{DateTime.Now}");

        return sw;
    }

    protected virtual void ReleaseGlobalFolder()
    {
        //if we found at least one global folder and all the global folders we did find had the same contents
        if (ReleaseAudit.SourceGlobalFolder != null)
        {
            var destination = new DirectoryInfo(Path.Combine(ReleaseAudit.ReleaseFolder.FullName,
                ReleaseAudit.SourceGlobalFolder.Name));
            ReleaseAudit.SourceGlobalFolder.CopyAll(destination);
        }
    }

    protected virtual void ReleaseAllExtractionConfigurations(
        Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease, StreamWriter sw,
        Dictionary<IExtractionConfiguration, ReleaseEnvironmentPotential> environments, bool isPatch)
    {
        //for each configuration, all the release potentials can be released
        foreach (var kvp in toRelease)
        {
            var extractionIdentifier = $"{kvp.Key.Name}_{kvp.Key.ID}";

            //create a root folder with the same name as the configuration (e.g. controls folder then next loop iteration a cases folder - with a different cohort)
            var configurationSubDirectory = ReleaseAudit.ReleaseFolder.CreateSubdirectory(extractionIdentifier);

            AuditExtractionConfigurationDetails(sw, configurationSubDirectory, kvp, extractionIdentifier);

            AuditDirectoryCreation(configurationSubDirectory.Name, sw, 0);

            var customDataFolder = ReleaseCustomData(kvp, configurationSubDirectory);
            if (customDataFolder != null)
                AuditDirectoryCreation(customDataFolder.FullName, sw, 1);

            var otherDataFolder = ReleaseMasterData(kvp, configurationSubDirectory);
            if (otherDataFolder != null)
                AuditDirectoryCreation(otherDataFolder.FullName, sw, 1);

            var metadataFolder = ReleaseMetadata(kvp, configurationSubDirectory);
            if (metadataFolder != null)
                AuditDirectoryCreation(metadataFolder.FullName, sw, 1);

            //generate release document
            var generator = new WordDataReleaseFileGenerator(kvp.Key, _repository);
            generator.GenerateWordFile(Path.Combine(configurationSubDirectory.FullName,
                $"ReleaseDocument_{extractionIdentifier}.docx"));
            AuditFileCreation($"ReleaseDocument_{extractionIdentifier}.docx", sw, 1);

            //only copy across directories that are explicitly validated with a ReleasePotential
            foreach (var rp in kvp.Value)
            {
                if (rp.ExtractDirectory == null)
                    continue;

                var rpDirectory = configurationSubDirectory.CreateSubdirectory(rp.ExtractDirectory.Name);
                AuditDirectoryCreation(rpDirectory.Name, sw, 1);

                CutTreeRecursive(rp.ExtractDirectory, rpDirectory, sw, 2);
                AuditProperRelease(rp, environments[kvp.Key], rpDirectory, isPatch);
            }

            ConfigurationsReleased.Add(kvp.Key);
        }
    }

    protected virtual DirectoryInfo ReleaseCustomData(
        KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp, DirectoryInfo configurationSubDirectory)
    {
        //if there is custom data copy that across for the specific cohort
        var fromCustomData = ThrowIfCustomDataConflictElseReturnFirstCustomDataFolder(kvp);
        if (fromCustomData != null)
        {
            var destination = new DirectoryInfo(Path.Combine(configurationSubDirectory.FullName, fromCustomData.Name));
            fromCustomData.CopyAll(destination);
        }

        return fromCustomData;
    }

    protected virtual DirectoryInfo ReleaseMasterData(
        KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp, DirectoryInfo configurationSubDirectory)
    {
        //if there is custom data copy that across for the specific cohort
        var fromMasterData = ThrowIfMasterDataConflictElseReturnFirstOtherDataFolder(kvp);
        if (fromMasterData != null)
        {
            var destination =
                new DirectoryInfo(Path.Combine(configurationSubDirectory.Parent.FullName, fromMasterData.Name));
            fromMasterData.CopyAll(destination);
        }

        return fromMasterData;
    }

    protected virtual DirectoryInfo ReleaseMetadata(KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp,
        DirectoryInfo configurationSubDirectory)
    {
        //if there is custom data copy that across for the specific cohort
        var folderFound = GetAllFoldersCalled(ExtractionDirectory.METADATA_FOLDER_NAME, kvp);
        var source = GetUniqueDirectoryFrom(folderFound.Distinct(new DirectoryInfoComparer()).ToList());

        if (source != null)
        {
            var destination = new DirectoryInfo(Path.Combine(configurationSubDirectory.Parent.FullName, source.Name));
            source.CopyAll(destination);
        }

        return source;
    }

    protected virtual void AuditExtractionConfigurationDetails(StreamWriter sw, DirectoryInfo configurationSubDirectory,
        KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp, string extractionIdentifier)
    {
        //audit in contents.txt
        sw.WriteLine($"Folder:{configurationSubDirectory.Name}");
        sw.WriteLine($"ConfigurationName:{kvp.Key.Name}");
        sw.WriteLine($"ConfigurationDescription:{kvp.Key.Description}");
        sw.WriteLine($"ExtractionConfiguration.ID:{kvp.Key.ID}");
        sw.WriteLine($"ExtractionConfiguration Identifier:{extractionIdentifier}");
        sw.WriteLine(
            $"CumulativeExtractionResult.ID(s):{kvp.Value.Select(v => v.DatasetExtractionResult.ID).Distinct().Aggregate("", (s, n) => $"{s}{n},").TrimEnd(',')}");
        sw.WriteLine($"CohortName:{_repository.GetObjectByID<ExtractableCohort>((int)kvp.Key.Cohort_ID)}");
        sw.WriteLine($"CohortID:{kvp.Key.Cohort_ID}");
    }

    protected void AuditProperRelease(ReleasePotential rp, ReleaseEnvironmentPotential environment,
        DirectoryInfo rpDirectory, bool isPatch)
    {
        FileInfo datasetFile = null;

        if (rp.ExtractFile != null)
        {
            var expectedFilename = rp.ExtractFile.Name;
            datasetFile = rpDirectory.EnumerateFiles().SingleOrDefault(f => f.Name.Equals(expectedFilename));
            if (datasetFile == null)
                throw new Exception(
                    $"Expected to find file called {expectedFilename} in directory {rpDirectory.FullName}, but could not");
        }

        //creates a new one in the database
        new ReleaseLog(_repository, rp, environment, isPatch, rpDirectory, datasetFile);
    }

    protected static DirectoryInfo ThrowIfCustomDataConflictElseReturnFirstCustomDataFolder(
        KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease)
    {
        var customDirectoriesFound = GetAllFoldersCalled(ExtractionDirectory.CUSTOM_COHORT_DATA_FOLDER_NAME, toRelease);
        return GetUniqueDirectoryFrom(customDirectoriesFound.Distinct(new DirectoryInfoComparer()).ToList());
    }

    protected static DirectoryInfo ThrowIfMasterDataConflictElseReturnFirstOtherDataFolder(
        KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease)
    {
        var masterDataDirectoriesFound = GetAllFoldersCalled(ExtractionDirectory.MASTER_DATA_FOLDER_NAME, toRelease);
        return GetUniqueDirectoryFrom(masterDataDirectoriesFound.Distinct(new DirectoryInfoComparer()).ToList());
    }

    protected static IEnumerable<DirectoryInfo> GetAllFoldersCalled(string folderName,
        KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease)
    {
        return toRelease.Value.Where(releasePotential => releasePotential.ExtractDirectory?.Parent != null)
            .Select(releasePotential => releasePotential.ExtractDirectory.Parent
                .EnumerateDirectories(folderName, SearchOption.TopDirectoryOnly)
                .SingleOrDefault())
            .Where(globalFolderForThisExtract => globalFolderForThisExtract != null)
            .Select(globalFolderForThisExtract => globalFolderForThisExtract);
    }

    protected static DirectoryInfo GetUniqueDirectoryFrom(List<DirectoryInfo> directoryInfos)
    {
        if (!directoryInfos.Any())
            return null;

        var first = directoryInfos.First();

        foreach (var directoryInfo in directoryInfos)
        {
            ConfirmValidityOfGlobalsOrCustomDataDirectory(
                directoryInfo); //check there are no pollution in globals directories
            UsefulStuff.ConfirmContentsOfDirectoryAreTheSame(first,
                directoryInfo); //this checks first against first then first against second, then first against third etc
        }

        return first;
    }

    protected static void ConfirmValidityOfGlobalsOrCustomDataDirectory(DirectoryInfo globalsDirectoryInfo)
    {
        if (globalsDirectoryInfo.EnumerateDirectories().Any())
            throw new Exception(
                $"Folder \"{globalsDirectoryInfo.FullName}\" contains subdirectories, this is not permitted");
    }

    protected void CutTreeRecursive(DirectoryInfo from, DirectoryInfo into, StreamWriter audit, int tabDepth)
    {
        //found files in current directory
        foreach (var file in from.GetFiles())
        {
            //audit as -Filename at tab indent
            AuditFileCreation(file.Name, audit, tabDepth);
            file.CopyTo(Path.Combine(into.FullName, file.Name));
        }

        //found subdirectory
        foreach (var dir in from.GetDirectories())
            //if it is not completely empty, copy it across
            if (dir.GetFiles().Any() || dir.GetDirectories().Any())
            {
                //audit as +DirectoryName at tab indent
                AuditDirectoryCreation(dir.Name, audit, tabDepth);
                CutTreeRecursive(dir, into.CreateSubdirectory(dir.Name), audit, tabDepth + 1);
            }
    }

    protected static void AuditFileCreation(string name, StreamWriter audit, int tabDepth)
    {
        for (var i = 0; i < tabDepth; i++)
            audit.Write("\t");

        audit.WriteLine($"-{name}");
    }

    protected static void AuditDirectoryCreation(string dir, StreamWriter audit, int tabDepth)
    {
        for (var i = 0; i < tabDepth; i++)
            audit.Write("\t");

        audit.WriteLine($"+{dir}");
    }
}