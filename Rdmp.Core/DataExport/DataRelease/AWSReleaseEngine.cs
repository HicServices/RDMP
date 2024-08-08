using Amazon.S3.Model;
using Amazon.Util.Internal;
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
using System.Text;
using System.Threading.Tasks;

namespace Rdmp.Core.DataExport.DataRelease
{
    public class AWSReleaseEngine : ReleaseEngine
    {

        AWSS3 _s3Helper;
        S3Bucket _bucket;
        string _bucketFolder;

        public AWSReleaseEngine(Project project, ReleaseEngineSettings settings, AWSS3 s3Helper, S3Bucket bucket, string bucketFolder, IDataLoadEventListener listener, ReleaseAudit releaseAudit) : base(project, settings, listener, releaseAudit)
        {
            _s3Helper = s3Helper;
            _bucket = bucket;
            _bucketFolder = bucketFolder;
        }

        public override void DoRelease(Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease, Dictionary<IExtractionConfiguration, ReleaseEnvironmentPotential> environments, bool isPatch)
        {

            ConfigurationsToRelease = toRelease;
            var auditFilePath = Path.Combine(Path.GetTempPath(), "contents.txt");
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
                    await _s3Helper.PutObject(_bucket.BucketName, "contents.txt", auditFilePath, GetLocation(ReleaseAudit.ReleaseFolder != null ? ReleaseAudit.ReleaseFolder.Name : null, null));
                }
                ).Wait();
                File.Delete(auditFilePath);

            }
            ReleaseSuccessful = true;
        }

        protected void ReleaseGlobalFolder(DirectoryInfo directory = null)
        {
            //todo untested
            if (directory == null)
                directory = ReleaseAudit.SourceGlobalFolder;

            if (ReleaseAudit.SourceGlobalFolder != null)
            {
                foreach (var dir in directory.GetDirectories())
                {
                    ReleaseGlobalFolder(dir);// todo this won't put it in the currect subfolder
                }
                foreach (var file in directory.EnumerateFiles())
                {
                    Task.Run(async () => await _s3Helper.PutObject(_bucket.BucketName, file.Name, file.FullName)).Wait(); //TODO put these somewhere
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
                var locationWithinBucket = GetLocation(ReleaseAudit.ReleaseFolder != null ? ReleaseAudit.ReleaseFolder.Name : null, extractionIdentifier);// $"{!string.IsNullOrWhiteSpace(_bucketFolder)?}{ReleaseAudit.ReleaseFolder.Name}/{extractionIdentifier}";
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

                foreach (var rp in kvp.Value)
                {
                    if (rp.ExtractDirectory == null)
                        continue;
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
                //datasetFile = rpDirectory.EnumerateFiles().SingleOrDefault(f => f.Name.Equals(expectedFilename));
                //if (datasetFile == null)
                //    throw new Exception(
                //        $"Expected to find file called {expectedFilename} in directory {rpDirectory}, but could not");
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
            foreach (var dir in from.GetDirectories())
                //if it is not completely empty, copy it across
                if (dir.GetFiles().Any() || dir.GetDirectories().Any())
                {
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
}
