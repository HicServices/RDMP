using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using CatalogueLibrary.DataFlowPipeline;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.ExtractionTime;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;
using Ticketing;

namespace DataExportLibrary.DataRelease
{
    public class ReleaseEngine
    {
        protected readonly IRepository _repository;
        public Project Project { get; private set; }
        public bool ReleaseSuccessful { get; protected set; }
        public List<IExtractionConfiguration> ConfigurationsReleased { get; private set; }

        public ReleaseEngineSettings ReleaseSettings { get; set; }

        public DirectoryInfo SourceGlobalFolder { get; set; }
        public DirectoryInfo ReleaseFolder { get; set; }

        public ReleaseEngine(Project project, ReleaseEngineSettings settings = null)
        {
            _repository = project.Repository;
            Project = project;
            ReleaseSuccessful = false;
            ConfigurationsReleased = new List<IExtractionConfiguration>();

            ReleaseSettings = settings;
            if (ReleaseSettings == null)
                ReleaseSettings = new ReleaseEngineSettings();
                
        }

        public virtual void DoRelease(Dictionary<IExtractionConfiguration,List<ReleasePotential>> toRelease, ReleaseEnvironmentPotential environment, bool isPatch)
        {
            VerifyReleasability(toRelease, environment);

            SourceGlobalFolder = PrepareAndVerifySourceGlobalFolder(toRelease);
            ReleaseFolder = PrepareAndVerifyReleaseFolder();

            StreamWriter sw = PrepareAuditFile();
            
            ReleaseGlobalFolder();
            
            // Audit Global Folder if there are any
            if (SourceGlobalFolder != null)
            {
                AuditDirectoryCreation(SourceGlobalFolder.FullName, sw, 0);

                foreach (FileInfo fileInfo in SourceGlobalFolder.GetFiles())
                    AuditFileCreation(fileInfo.Name, sw, 1);
            }

            ReleaseAllExtractionConfigurations(toRelease, sw, environment, isPatch);
            
            sw.Flush();
            sw.Close();
            ReleaseSuccessful = true;
        }

        protected virtual void VerifyReleasability(Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease, ReleaseEnvironmentPotential environment)
        {
            //make sure everything is releasable
            var dodgyStates = toRelease.Where(
                kvp =>
                    kvp.Value.Any(
                        p =>
                            //these are the only permissable release states
                            p.Assesment != Releaseability.Releaseable &&
                            p.Assesment != Releaseability.ColumnDifferencesVsCatalogue)).ToArray();
            
            if (dodgyStates.Any())
            {
                StringBuilder sb = new StringBuilder();
                foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp in dodgyStates)
                {
                    sb.AppendLine(kvp.Key + ":");
                    foreach (var releasePotential in kvp.Value)
                        sb.AppendLine("\t" + releasePotential.Configuration.Name + " : " + releasePotential.Assesment);

                }

                throw new Exception("Attempted to release a dataset that was not evaluated as being releaseable.  The following Release Potentials were at a dodgy state:" + sb);
            }

            if (toRelease.Any(kvp => kvp.Key.Project_ID != Project.ID))
                throw new Exception("Mismatch between project passed into constructor and DoRelease projects");

            if (environment.Assesment != TicketingReleaseabilityEvaluation.Releaseable && environment.Assesment != TicketingReleaseabilityEvaluation.TicketingLibraryMissingOrNotConfiguredCorrectly)
                throw new Exception("Ticketing system decided that the Environment is not ready for release. Reason: " + environment.Reason);
        }

        protected virtual DirectoryInfo PrepareAndVerifySourceGlobalFolder(Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease)
        {
            var globalDirectoriesFound = new List<DirectoryInfo>();

            foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> releasePotentials in toRelease)
                globalDirectoriesFound.AddRange(GetAllFoldersCalled(ExtractionDirectory.GlobalsDataFolderName, releasePotentials));

            if (globalDirectoriesFound.Any())
            {
                var firstGlobal = globalDirectoriesFound.First();

                foreach (var directoryInfo in globalDirectoriesFound.Distinct(new DirectoryInfoComparer()))
                {
                    ConfirmContentsOfDirectoryAreTheSame(firstGlobal, directoryInfo);
                }

                return firstGlobal;
            }

            return null;
        }

        protected virtual DirectoryInfo PrepareAndVerifyReleaseFolder()
        {
            var folder = GetIntendedReleaseDirectory();
            if (!folder.Exists)
            {
                if (ReleaseSettings.CreateReleaseDirectoryIfNotFound)
                    folder.Create();
                else
                    throw new Exception("Intended release directory was not found and I was forbidden to create it: " + folder.FullName);
            }

            //make sure user isn't sneaking any pollution into this directory
            if (folder.EnumerateDirectories().Any() || folder.EnumerateFiles().Any())
                throw new Exception("Intended release directory is not empty:" + folder.FullName);

            return folder;
        }

        protected virtual StreamWriter PrepareAuditFile()
        {
            var sw = new StreamWriter(Path.Combine(ReleaseFolder.FullName, "contents.txt"));

            sw.WriteLine("----------Details Of Release---------:" + DateTime.Now);
            sw.WriteLine("ProjectName:" + Project.Name);
            sw.WriteLine("ProjectNumber:" + Project.ProjectNumber);
            sw.WriteLine("Project.ID:" + Project.ID);
            sw.WriteLine("ThisFileWasCreated:" + DateTime.Now);

            sw.WriteLine("----------Contents Of Directory---------:" + DateTime.Now);

            return sw;
        }

        protected virtual void ReleaseGlobalFolder()
        {
            //if we found at least one global folder and all the global folders we did find had the same contents
            if (SourceGlobalFolder != null)
            {
                if (ReleaseSettings.DeleteFilesOnSuccess)
                {
                    SourceGlobalFolder.MoveTo(Path.Combine(ReleaseFolder.FullName, SourceGlobalFolder.Name));
                }
                else
                {
                    var destination = new DirectoryInfo(Path.Combine(ReleaseFolder.FullName, SourceGlobalFolder.Name));
                    SourceGlobalFolder.CopyAll(destination);
                }
            }
        }

        protected virtual void ReleaseAllExtractionConfigurations(Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease, StreamWriter sw, ReleaseEnvironmentPotential environment, bool isPatch)
        {
            //for each configuration, all the release potentials can be released
            foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp in toRelease)
            {
                var extractionIdentifier = "";
                if (!String.IsNullOrWhiteSpace(kvp.Key.RequestTicket) && !String.IsNullOrWhiteSpace(kvp.Key.ReleaseTicket))
                    extractionIdentifier = String.Format("{0}_{1}", kvp.Key.RequestTicket, kvp.Key.ReleaseTicket);
                else
                    extractionIdentifier = kvp.Key.Name + "_" + kvp.Key.ID;

                //create a root folder with the same name as the configuration (e.g. controls folder then next loop iteration a cases folder - with a different cohort)
                DirectoryInfo configurationSubDirectory = ReleaseFolder.CreateSubdirectory("Configuration-" + extractionIdentifier);

                AuditExtractionConfigurationDetails(sw, configurationSubDirectory, kvp, extractionIdentifier);

                AuditDirectoryCreation(configurationSubDirectory.Name, sw, 0);

                var customDataFolder = ReleaseCustomData(kvp, configurationSubDirectory);
                if (customDataFolder != null)
                    AuditDirectoryCreation(customDataFolder.FullName, sw, 1);

                //generate release document
                var generator = new WordDataReleaseFileGenerator(kvp.Key, _repository);
                if (generator.RequirementsMet())
                {
                    generator.GenerateWordFile(Path.Combine(configurationSubDirectory.FullName, "ReleaseDocument_" + extractionIdentifier + ".docx"));
                    AuditFileCreation("ReleaseDocument" + extractionIdentifier + ".docx", sw, 1);
                }
                else
                    sw.WriteLine("Release Document Not Generated Because Office Not Installed");

                //only copy across directories that are explicitly validated with a ReleasePotential
                foreach (ReleasePotential rp in kvp.Value)
                {
                    DirectoryInfo rpDirectory = configurationSubDirectory.CreateSubdirectory(rp.ExtractDirectory.Name);
                    AuditDirectoryCreation(rpDirectory.Name, sw, 1);

                    CutTreeRecursive(rp.ExtractDirectory, rpDirectory, sw, 2);
                    AuditProperRelease(rp, environment, rpDirectory, isPatch);
                }

                //mark configuration as released
                kvp.Key.IsReleased = true;
                kvp.Key.SaveToDatabase();

                ConfigurationsReleased.Add(kvp.Key);
            }
        }

        protected virtual DirectoryInfo ReleaseCustomData(KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp, DirectoryInfo configurationSubDirectory)
        {
            //if there is custom data copy that across for the specific cohort
            DirectoryInfo fromCustomData = ThrowIfCustomDataConflictElseReturnFirstCustomDataFolder(kvp);
            if (fromCustomData != null)
            {
                if (ReleaseSettings.DeleteFilesOnSuccess)
                    fromCustomData.MoveTo(Path.Combine(configurationSubDirectory.FullName, fromCustomData.Name));
                else
                {
                    var destination = new DirectoryInfo(Path.Combine(configurationSubDirectory.FullName, fromCustomData.Name));
                    fromCustomData.CopyAll(destination);
                }
            }
            return fromCustomData;
        }

        protected virtual void AuditExtractionConfigurationDetails(StreamWriter sw, DirectoryInfo configurationSubDirectory, KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp, string extractionIdentifier)
        {
            //audit in contents.txt
            sw.WriteLine("Folder:" + configurationSubDirectory.Name);
            sw.WriteLine("ConfigurationName:" + kvp.Key.Name);
            sw.WriteLine("ConfigurationDescription:" + kvp.Key.Description);
            sw.WriteLine("ExtractionConfiguration.ID:" + kvp.Key.ID);
            sw.WriteLine("ExtractionConfiguration Identifier:" + extractionIdentifier);
            sw.WriteLine("CumulativeExtractionResult.ID(s):" +
                         kvp.Value.Select(v => v.ExtractionResults.ID)
                             .Distinct()
                             .Aggregate("", (s, n) => s + n + ",")
                             .TrimEnd(','));
            sw.WriteLine("CohortName:" + _repository.GetObjectByID<ExtractableCohort>((int)kvp.Key.Cohort_ID));
            sw.WriteLine("CohortID:" + kvp.Key.Cohort_ID);
        }

        protected void AuditProperRelease(ReleasePotential rp, ReleaseEnvironmentPotential environment, DirectoryInfo rpDirectory, bool isPatch)
        {
            ReleaseLogWriter logWriter = new ReleaseLogWriter(rp, environment, _repository);

            var expectedFilename = rp.DataSet + ".csv";
            var datasetFile = rpDirectory.EnumerateFiles().SingleOrDefault(f => f.Name.Equals(expectedFilename));
            if (datasetFile == null)
            {
                throw new Exception("Expected to find file called " + expectedFilename + " in directory " + rpDirectory.FullName + ", but could not");
            }

            logWriter.GenerateLogEntry(isPatch, rpDirectory, datasetFile);
        }

        protected DirectoryInfo GetIntendedReleaseDirectory()
        {
            if (ReleaseSettings.UseProjectExtractionFolder)
            {
                if (string.IsNullOrWhiteSpace(Project.ExtractionDirectory))
                    return null;

                var prefix = DateTime.UtcNow.ToString("yyyy-MM-dd_");
                var suffix = "";
                if (String.IsNullOrWhiteSpace(Project.MasterTicket))
                    suffix = Project.ID + "_" + Project.Name;
                else
                    suffix = Project.MasterTicket;

                return new DirectoryInfo(Path.Combine(Project.ExtractionDirectory, prefix + "Release-" + suffix)); 
            }
            
            return ReleaseSettings.CustomExtractionDirectory;
        }

        protected DirectoryInfo ThrowIfCustomDataConflictElseReturnFirstCustomDataFolder(KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease)
        {
            var customDirectoriesFound = GetAllFoldersCalled(ExtractionDirectory.CustomCohortDataFolderName, toRelease);
            return GetUniqueDirectoryFrom(customDirectoriesFound.Distinct(new DirectoryInfoComparer()).ToList());
        }

        protected IEnumerable<DirectoryInfo> GetAllFoldersCalled(string folderName, KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease)
        {
            foreach (ReleasePotential releasePotential in toRelease.Value)
            {
                Debug.Assert(releasePotential.ExtractDirectory.Parent != null, "releasePotential.ExtractDirectory.Parent != null");
                DirectoryInfo globalFolderForThisExtract = releasePotential.ExtractDirectory.Parent.EnumerateDirectories(folderName, SearchOption.TopDirectoryOnly).SingleOrDefault();

                if (globalFolderForThisExtract == null) //this particualar release didn't include globals/custom data at all
                    continue;

                //if we haven't added it yet, cant use Equals because apparently its by ref not value eh
                yield return (globalFolderForThisExtract);
            }
        }
        
        protected DirectoryInfo GetUniqueDirectoryFrom(List<DirectoryInfo> directoryInfos)
        {
            if (!directoryInfos.Any())
                return null;

            DirectoryInfo first = directoryInfos.First();

            foreach (DirectoryInfo directoryInfo in directoryInfos)
            {
                ConfirmValidityOfGlobalsOrCustomDataDirectory(directoryInfo); //check there are no polution in globals directories
                ConfirmContentsOfDirectoryAreTheSame(first, directoryInfo); //this checks first against first then first against second, then first against third etc
            }

            return first;
        }

        protected void ConfirmValidityOfGlobalsOrCustomDataDirectory(DirectoryInfo globalsDirectoryInfo)
        {
            if(globalsDirectoryInfo.EnumerateDirectories().Any())
                throw new Exception("Folder \"" + globalsDirectoryInfo.FullName + "\" contains subdirectories, this is not permitted");
        }

        protected void ConfirmContentsOfDirectoryAreTheSame(DirectoryInfo first, DirectoryInfo other)
        {
            if(first.EnumerateFiles().Count()!= other.EnumerateFiles().Count())
                throw new Exception("found different number of files in Globals directory " + first.FullName + " and " + other.FullName);

            var filesInFirst = first.EnumerateFiles().ToArray();
            var filesInOther = other.EnumerateFiles().ToArray();

            for (int i = 0; i < filesInFirst.Length; i++)
            {
                FileInfo file1 = filesInFirst[i];
                FileInfo file2 = filesInOther[i];
                if(!file1.Name.Equals(file2.Name))
                    throw new Exception("Although there were the same number of files in Globals directories " + first.FullName + " and " + other.FullName + ", there were differing file names ("+file1.Name +" and "+file2.Name+")");

                if(!UsefulStuff.MD5File(file1.FullName).Equals(UsefulStuff.MD5File(file2.FullName)))
                    throw new Exception("File found in Globals directory which has a different MD5 from another Globals file.  Files were \"" + file1.FullName + "\" and \"" + file2.FullName + "\"");
            }
        }

        protected void CutTreeRecursive(DirectoryInfo from, DirectoryInfo into, StreamWriter audit, int tabDepth)
        {
            //found files in current directory
            foreach (FileInfo file in from.GetFiles())
            {
                //audit as -Filename at tab indent 
                AuditFileCreation(file.Name, audit, tabDepth);
                if (ReleaseSettings.DeleteFilesOnSuccess)
                    file.MoveTo(Path.Combine(into.FullName, file.Name));
                else
                    file.CopyTo(Path.Combine(into.FullName, file.Name));
            }

            //found subdirectory
            foreach (DirectoryInfo dir in from.GetDirectories())
            {
                //if it is not completely empty, copy it across
                if (dir.GetFiles().Any() || dir.GetDirectories().Any())
                {
                    //audit as +DirectoryName at tab indent
                    AuditDirectoryCreation(dir.Name, audit, tabDepth);
                    CutTreeRecursive(dir, into.CreateSubdirectory(dir.Name), audit, tabDepth + 1);
                }

            }

        }

        protected void AuditFileCreation(string name, StreamWriter audit, int tabDepth)
        {
            for (int i = 0; i < tabDepth; i++)
                audit.Write("\t");

            audit.WriteLine("-" + name);
        }

        protected void AuditDirectoryCreation(string dir, StreamWriter audit, int tabDepth)
        {
            for (int i = 0; i < tabDepth; i++)
                audit.Write("\t");

            audit.WriteLine("+" + dir);
            
        }
    }
}
