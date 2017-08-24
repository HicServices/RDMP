using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.DataRelease.Audit;
using DataExportLibrary.ExtractionTime;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode;

namespace DataExportLibrary.DataRelease
{
    public class ReleaseEngine
    {
        private readonly IRepository _repository;
        public Project Project { get; private set; }
        public bool Releasesuccessful { get; private set; }
        public List<IExtractionConfiguration> ConfigurationsReleased { get; private set; }

        public ReleaseEngine(Project project)
        {
            _repository = project.Repository;
            Project = project;
            Releasesuccessful = false;
            ConfigurationsReleased= new List<IExtractionConfiguration>();
        }

        public DirectoryInfo GetIntendedReleaseDirectory()
        {
            if (string.IsNullOrWhiteSpace(Project.ExtractionDirectory))
                return null;

            return new DirectoryInfo(Path.Combine(Project.ExtractionDirectory,"Release")); 
        }

        public void DoRelease(Dictionary<IExtractionConfiguration,List<ReleasePotential>> toRelease, ReleaseEnvironmentPotential environment,bool isPatch)
        {
            //make sure everything is releasable
            if (toRelease.Any(kvp => kvp.Value.Any(p=>p.Assesment != Releaseability.Releaseable && p.Assesment != Releaseability.ColumnDifferencesVsCatalogue)))//these are the only permissable release states
                throw new Exception("Attempted to release a dataset that was not evaluated as being releaseable");
            
            if (toRelease.Any(kvp => kvp.Key.Project_ID != Project.ID))
                throw new Exception("Mismatch between project passed into constructor and DoRelease projects");

            DirectoryInfo globalFolder = ThrowIfGlobalConflictElseReturnFirstGlobalFolder(toRelease);
            DirectoryInfo intendedReleaseDirectory = GetIntendedReleaseDirectory();

            if (!intendedReleaseDirectory.Exists)
                intendedReleaseDirectory.Create();

            //make sure user isn't sneaking any pollution into this directory
            if(intendedReleaseDirectory.EnumerateDirectories().Any() || intendedReleaseDirectory.EnumerateFiles().Any()) 
                throw new Exception("Intended release directory is not empty:" +intendedReleaseDirectory.FullName );

            StreamWriter sw = new StreamWriter(Path.Combine(intendedReleaseDirectory.FullName,"contents.txt"));
            
            sw.WriteLine("----------Details Of Release---------:" + DateTime.Now);
            sw.WriteLine("ProjectName:" + Project.Name );
            sw.WriteLine("ProjectNumber:" + Project.ProjectNumber );
            sw.WriteLine("Project.ID:" + Project.ID);
            sw.WriteLine("ThisFileWasCreated:" + DateTime.Now);

            sw.WriteLine("----------Contents Of Directory---------:" + DateTime.Now);

            //if we found at least one global folder and all the global folders we did find had the same contents
            if (globalFolder != null)
            {
                globalFolder.MoveTo(Path.Combine(intendedReleaseDirectory.FullName,globalFolder.Name));
                AuditDirectoryCreation(globalFolder.FullName,sw,0);

                foreach (FileInfo fileInfo in globalFolder.GetFiles())
                    AuditFileCreation(fileInfo.Name,sw,1);
            }

            //for each configuration, all the release potentials can be released
            foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> kvp in toRelease)
            {
                //create a root folder with the same name as the configuration (e.g. controls folder then next loop iteration a cases folder - with a different cohort)
                DirectoryInfo configurationSubDirectory = intendedReleaseDirectory.CreateSubdirectory("Configuration " +  kvp.Key.ID);

                //audit in contents.txt
                sw.WriteLine("Folder:" + configurationSubDirectory.Name);
                sw.WriteLine("ConfigurationName:" + kvp.Key.Name);
                sw.WriteLine("ConfigurationDescription:" + kvp.Key.Description);
                sw.WriteLine("ExtractionConfiguration.ID:" + kvp.Key.ID);
                sw.WriteLine("CumulativeExtractionResult.ID(s):" + kvp.Value.Select(v=>v.ExtractionResults.ID).Distinct().Aggregate("",(s,n)=>s+n+",").TrimEnd(','));
                sw.WriteLine("CohortName:" + _repository.GetObjectByID<ExtractableCohort>((int) kvp.Key.Cohort_ID));
                sw.WriteLine("CohortID:" + kvp.Key.Cohort_ID);

                AuditDirectoryCreation(configurationSubDirectory.Name, sw, 0);

                //if there is custom data copy that across for the specific cohort
                DirectoryInfo fromCustomData = ThrowIfCustomDataConflictElseReturnFirstCustomDataFolder(kvp);
                if (fromCustomData != null)
                {
                    fromCustomData.MoveTo(Path.Combine(configurationSubDirectory.FullName, fromCustomData.Name));
                    AuditDirectoryCreation(fromCustomData.FullName, sw, 1);
                }

                //generate release document
                WordDataReleaseFileGenerator generator = new WordDataReleaseFileGenerator(kvp.Key, _repository);
                if(generator.RequirementsMet())
                {
                    generator.GenerateWordFile(Path.Combine(configurationSubDirectory.FullName,"ReleaseDocument_"+kvp.Key.ID+".docx"));
                    AuditFileCreation("ReleaseDocument" + kvp.Key.ID + ".docx", sw, 1);
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
            
            sw.Flush();
            sw.Close();
            Releasesuccessful = true;
        }

        private void AuditProperRelease(ReleasePotential rp, ReleaseEnvironmentPotential environment, DirectoryInfo rpDirectory, bool isPatch)
        {
            ReleaseLogWriter logWriter = new ReleaseLogWriter(rp, environment, _repository);

            var expectedFilename = rp.DataSet + ".csv";
            var datasetFile = rpDirectory.EnumerateFiles().SingleOrDefault(f=>f.Name.Equals(expectedFilename));
            if (datasetFile == null)
            {
                throw new Exception("Expected to find file called " + expectedFilename + " in directory " + rpDirectory.FullName + ", but could not");
            }

            logWriter.GenerateLogEntry(isPatch, rpDirectory, datasetFile);
        }


        private DirectoryInfo ThrowIfCustomDataConflictElseReturnFirstCustomDataFolder(KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease)
        {
            List<DirectoryInfo> customDirectoriesFound = GetAllFoldersCalled(ExtractionDirectory.CustomCohortDataFolderName, toRelease,new List<DirectoryInfo>());
            return GetUniqueDirectoryFrom(customDirectoriesFound);
        }

        private List<DirectoryInfo> GetAllFoldersCalled(string folderName, KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> toRelease, List<DirectoryInfo> alreadySeenBefore)
        {
            foreach (ReleasePotential releasePotential in toRelease.Value)
            {
                DirectoryInfo globalFolderForThisExtract = releasePotential.ExtractDirectory.Parent.EnumerateDirectories(folderName,SearchOption.TopDirectoryOnly).SingleOrDefault();

                if (globalFolderForThisExtract == null)//this particualar release didn't include globals/custom data at all
                    continue;

                //if we haven't added it yet, cant use Equals because apparently its by ref not value eh
                if (!alreadySeenBefore.Any(d => d.FullName.Equals(globalFolderForThisExtract.FullName)))
                    alreadySeenBefore.Add(globalFolderForThisExtract);
            }

            return alreadySeenBefore;
        }

        private DirectoryInfo ThrowIfGlobalConflictElseReturnFirstGlobalFolder(Dictionary<IExtractionConfiguration, List<ReleasePotential>> toRelease)
        {
            List<DirectoryInfo> GlobalDirectoriesFound = new List<DirectoryInfo>();

            foreach (KeyValuePair<IExtractionConfiguration, List<ReleasePotential>> releasePotentials in toRelease)
                GlobalDirectoriesFound = GetAllFoldersCalled(ExtractionDirectory.GlobalsDataFolderName,releasePotentials, GlobalDirectoriesFound);

            return GetUniqueDirectoryFrom(GlobalDirectoriesFound);
        }

        private DirectoryInfo GetUniqueDirectoryFrom(List<DirectoryInfo> directoryInfos)
        {
            if (!directoryInfos.Any())
                return null;

            DirectoryInfo first = directoryInfos.First();

            foreach (DirectoryInfo directoryInfo in directoryInfos)
            {
                ConfirmValidityOfGlobalsOrCustomDataDirectory(directoryInfo); //check there are no polution in globals directories
                ConfirmContentsOfDirectoryAreTheSame(first, directoryInfo);//this checks first against first then first against second, then first against third etc
            }

            return first;
        }


        private void ConfirmValidityOfGlobalsOrCustomDataDirectory(DirectoryInfo globalsDirectoryInfo)
        {
            if(globalsDirectoryInfo.EnumerateDirectories().Any())
                throw new Exception("Folder \"" + globalsDirectoryInfo.FullName + "\" contains subdirectories, this is not permitted");
        }

        private void ConfirmContentsOfDirectoryAreTheSame(DirectoryInfo first, DirectoryInfo other)
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

        private void CutTreeRecursive(DirectoryInfo from, DirectoryInfo into, StreamWriter audit, int tabDepth)
        {
            //found files in current directory
            foreach (FileInfo file in from.GetFiles())
            {
                //audit as -Filename at tab indent 
                AuditFileCreation(file.Name, audit, tabDepth);
                file.MoveTo(Path.Combine(into.FullName, file.Name));
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

        private void AuditFileCreation(string name, StreamWriter audit, int tabDepth)
        {
            for (int i = 0; i < tabDepth; i++)
                audit.Write("\t");

            audit.WriteLine("-" + name);
        }

        private void AuditDirectoryCreation(string dir, StreamWriter audit, int tabDepth)
        {
            for (int i = 0; i < tabDepth; i++)
                audit.Write("\t");

            audit.WriteLine("+" + dir);
            
        }
    }
}
