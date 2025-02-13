// Copyright (c) The University of Dundee 2024-2024
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using Amazon.S3.Model;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataExport.DataExtraction;
using Rdmp.Core.DataExport.DataRelease.Audit;
using Rdmp.Core.DataExport.DataRelease.Pipeline;
using Rdmp.Core.DataExport.DataRelease.Potential;
using Rdmp.Core.Reports.ExtractionTime;
using Rdmp.Core.ReusableLibraryCode;
using Rdmp.Core.ReusableLibraryCode.AWS;
using Rdmp.Core.ReusableLibraryCode.Progress;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Terminal.Gui;

namespace Rdmp.Core.DataExport.DataRelease;

/// <summary>
/// Release engine for S3 buckets.
/// Write the release directory structure to an S3 bucket, with optional subdirectory
/// </summary>
public class AWSReleaseEngine : ReleaseEngine
{

    private readonly AWSS3 _s3Helper;
    private readonly S3Bucket _bucket;
    private readonly string _bucketFolder;

    public AWSReleaseEngine(Project project, ReleaseEngineSettings settings, AWSS3 s3Helper, S3Bucket bucket, string bucketFolder, IDataLoadEventListener listener, ReleaseAudit releaseAudit) : base(project, settings, listener, releaseAudit)
    {
        _s3Helper = s3Helper;
        _bucket = bucket;
        _bucketFolder = bucketFolder;
    }

    public override void DoRelease(Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease, Dictionary<IExtractionConfiguration, ReleaseEnvironmentPotential> environments, bool isPatch)
    {
        ConfigurationsToRelease = toRelease;
        const string contentsFileName = "contents.txt";
        var auditFilePath = Path.Combine(Path.GetTempPath(), contentsFileName);
        using (var sw = PrepareAuditFile(auditFilePath))
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

            Task.Run(async () =>
            {
                await _s3Helper.PutObject(_bucket.BucketName, contentsFileName, auditFilePath, GetLocation(null, null));
            }
            ).Wait();
            File.Delete(auditFilePath);

        }
        ReleaseSuccessful = true;
    }

    protected override void ReleaseGlobalFolder()
    {

        var directory = ReleaseAudit.SourceGlobalFolder;

        if (ReleaseAudit.SourceGlobalFolder != null)
        {
            foreach (var file in directory.EnumerateFiles())
            {
                var location = GetLocation("Globals", null);
                Task.Run(async () => await _s3Helper.PutObject(_bucket.BucketName, file.Name, file.FullName, location)).Wait();
            }
            foreach (var dir in directory.EnumerateDirectories())
            {
                foreach (var file in dir.EnumerateFiles())
                {
                    var location = GetLocation("Globals", dir.Name);
                    Task.Run(async () => await _s3Helper.PutObject(_bucket.BucketName, file.Name, file.FullName, location)).Wait();
                }
            }
        }
    }


    private string GetLocation(string existingPath, string newNode)
    {
        var location = "";
        if (_bucketFolder != null) location = $"{_bucketFolder}/";
        if (existingPath != null) location = $"{location}{existingPath}/";
        if (newNode != null) location = $"{location}{newNode}";
        return location;
    }

    protected override void ReleaseAllExtractionConfigurations(Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease, StreamWriter sw,
    Dictionary<IExtractionConfiguration, ReleaseEnvironmentPotential> environments, bool isPatch)
    {
        //for each configuration, all the release potentials can be released
        foreach (var kvp in toRelease)
        {
            var extractionIdentifier = $"{kvp.Key.Name}_{kvp.Key.ID}";
            var locationWithinBucket = GetLocation(null, extractionIdentifier);
            AuditExtractionConfigurationDetails(sw, locationWithinBucket, kvp, extractionIdentifier);
            AuditDirectoryCreation(locationWithinBucket, sw, 0);
            var customDataFolder = ReleaseCustomData(kvp, locationWithinBucket);
            if (customDataFolder != null)
                AuditDirectoryCreation(customDataFolder, sw, 1);
            var otherDataFolder = ReleaseMasterData(kvp, locationWithinBucket);
            if (otherDataFolder != null)
                AuditDirectoryCreation(otherDataFolder, sw, 1);
            var metadataFolder = ReleaseMetadata(kvp, locationWithinBucket);
            if (metadataFolder != null)
                AuditDirectoryCreation(metadataFolder, sw, 1);

            var wordDocName = $"ReleaseDocument_{extractionIdentifier}.docx";
            var wordDocPath = Path.Combine(Path.GetTempPath(),
               wordDocName);
            var generator = new WordDataReleaseFileGenerator(kvp.Key, _repository);
            generator.GenerateWordFile(wordDocPath);
            Task.Run(async () => await _s3Helper.PutObject(_bucket.BucketName, wordDocName, wordDocPath, locationWithinBucket)).Wait();
            AuditFileCreation(wordDocName, sw, 1);

            foreach (var rp in kvp.Value.Where(rp => rp.ExtractDirectory != null))
            {
                var rpDirectory = !string.IsNullOrWhiteSpace(locationWithinBucket) ? $"{locationWithinBucket}/{rp.ExtractDirectory.Name}" : rp.ExtractDirectory.Name;
                AuditDirectoryCreation(rpDirectory, sw, 1);
                CutTreeRecursive(rp.ExtractDirectory, rpDirectory, sw, 2);
                AuditProperRelease(rp, environments[kvp.Key], rpDirectory, isPatch);


            }
            ConfigurationsReleased.Add(kvp.Key);
        }
    }

    protected void AuditProperRelease(ReleasePotential rp, ReleaseEnvironmentPotential environment,
    string rpDirectory, bool isPatch)
    {
        FileInfo datasetFile = null;

        if (rp.ExtractFile != null)
        {
            var expectedFilename = rp.ExtractFile.Name;
            var directory = !string.IsNullOrWhiteSpace(rpDirectory) ? $"{rpDirectory}/{expectedFilename}" : expectedFilename;
            if (!_s3Helper.ObjectExists(directory, _bucket.BucketName))
            {
                throw new Exception(
                    $"Expected to find file called {expectedFilename} in S3 Bucket under {rpDirectory}, but could not");
            }
        }

        //creates a new one in the database
        new ReleaseLog(_repository, rp, environment, isPatch, new DirectoryInfo(rpDirectory), datasetFile);
    }

    protected void CutTreeRecursive(DirectoryInfo from, string into, StreamWriter audit, int tabDepth)
    {
        //found files in current directory
        foreach (var file in from.GetFiles())
        {
            //audit as -Filename at tab indent
            AuditFileCreation(file.Name, audit, tabDepth);
            Task.Run(async () => await _s3Helper.PutObject(_bucket.BucketName, file.Name, file.FullName, into)).Wait();

        }

        //found subdirectory
        foreach (var dir in from.GetDirectories().Where(dir => dir.GetFiles().Any() || dir.GetDirectories().Any())){
            //audit as +DirectoryName at tab indent
            AuditDirectoryCreation(dir.Name, audit, tabDepth);
            CutTreeRecursive(dir, !string.IsNullOrWhiteSpace(into) ? $"{into}/{dir.Name}" : dir.Name, audit, tabDepth + 1);
        }
    }
    protected string ReleaseMetadata(KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp,
  string configurationSubDirectory)
    {
        //if there is custom data copy that across for the specific cohort
        var folderFound = GetAllFoldersCalled(ExtractionDirectory.METADATA_FOLDER_NAME, kvp);
        var source = GetUniqueDirectoryFrom(folderFound.Distinct(new DirectoryInfoComparer()).ToList());

        if (source != null)
        {
            var locationPath = !string.IsNullOrWhiteSpace(configurationSubDirectory) ? $"{configurationSubDirectory}/{source.Name}" : source.Name;
            foreach (var file in source.EnumerateFiles())
            {
                Task.Run(async () => await _s3Helper.PutObject(_bucket.BucketName, file.Name, file.FullName, locationPath)).Wait();
            }
            return locationPath;
        }

        return null;
    }


    protected string ReleaseMasterData(
  KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp, string configurationSubDirectory)
    {
        //if there is custom data copy that across for the specific cohort
        var fromMasterData = ThrowIfMasterDataConflictElseReturnFirstOtherDataFolder(kvp);
        if (fromMasterData != null)
        {
            var locationPath = !string.IsNullOrWhiteSpace(configurationSubDirectory) ? $"{configurationSubDirectory}/{fromMasterData.Name}" : fromMasterData.Name;
            foreach (var file in fromMasterData.EnumerateFiles())
            {
                Task.Run(async () => await _s3Helper.PutObject(_bucket.BucketName, file.Name, file.FullName, locationPath)).Wait();
            }
            return locationPath;
        }

        return null;
    }

    protected string ReleaseCustomData(
  KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp, string configurationSubDirectory)
    {
        //if there is custom data copy that across for the specific cohort
        var fromCustomData = ThrowIfCustomDataConflictElseReturnFirstCustomDataFolder(kvp);
        if (fromCustomData != null)
        {
            var locationPath = !string.IsNullOrWhiteSpace(configurationSubDirectory) ? $"{configurationSubDirectory}/{fromCustomData.Name}" : fromCustomData.Name;
            foreach (var file in fromCustomData.EnumerateFiles())
            {
                Task.Run(async () => await _s3Helper.PutObject(_bucket.BucketName, file.Name, file.FullName, locationPath)).Wait();
            }
            return locationPath;
        }

        return null;
    }

    protected void AuditExtractionConfigurationDetails(StreamWriter sw, string configurationSubDirectory,
   KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp, string extractionIdentifier)
    {
        //audit in contents.txt
        sw.WriteLine($"Folder:{configurationSubDirectory}");
        sw.WriteLine($"ConfigurationName:{kvp.Key.Name}");
        sw.WriteLine($"ConfigurationDescription:{kvp.Key.Description}");
        sw.WriteLine($"ExtractionConfiguration.ID:{kvp.Key.ID}");
        sw.WriteLine($"ExtractionConfiguration Identifier:{extractionIdentifier}");
        sw.WriteLine(
            $"CumulativeExtractionResult.ID(s):{kvp.Value.Select(v => v.DatasetExtractionResult.ID).Distinct().Aggregate("", (s, n) => $"{s}{n},").TrimEnd(',')}");
        sw.WriteLine($"CohortName:{_repository.GetObjectByID<ExtractableCohort>((int)kvp.Key.Cohort_ID)}");
        sw.WriteLine($"CohortID:{kvp.Key.Cohort_ID}");
    }


    protected StreamWriter PrepareAuditFile(string path)
    {
        var sw = new StreamWriter(path);

        sw.WriteLine($"----------Details Of Release---------:{DateTime.Now}");
        sw.WriteLine($"ProjectName:{Project.Name}");
        sw.WriteLine($"ProjectNumber:{Project.ProjectNumber}");
        sw.WriteLine($"Project.ID:{Project.ID}");
        sw.WriteLine($"ThisFileWasCreated:{DateTime.Now}");

        sw.WriteLine($"----------Contents Of Directory---------:{DateTime.Now}");

        return sw;
    }
}
