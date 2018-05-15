using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime;
using DataExportLibrary.Data.DataTables;
using ReusableLibraryCode.Progress;

namespace DataExportLibrary.ExtractionTime
{
    /// <summary>
    /// The target directory for a given ExtractionConfiguration on a given day.  This is where linked anonymised project extracts will appear when 
    /// an ExtractionConfiguration is executed.  It is also the location where the Release Engine will pick them up from when it bundles together a
    /// release package.
    /// </summary>
    public class ExtractionDirectory : IExtractionDirectory
    {
        private readonly DirectoryInfo root;
        private readonly DirectoryInfo extractionDirectory;
        
        public const string ExtractionSubFolderName = "Extractions";
        public const string StandardExtractionPrefix = "Extr_";
        public const string GlobalsDataFolderName = "Globals";
        public const string CustomCohortDataFolderName = "CohortCustomData";
        public const string MasterDataFolderName = "MasterData";

        public ExtractionDirectory(string rootExtractionDirectory, IExtractionConfiguration configuration)
            : this(rootExtractionDirectory, configuration, DateTime.Now)
        {
        }

        private ExtractionDirectory(string rootExtractionDirectory, IExtractionConfiguration configuration, DateTime extractionDate)
        {
            if (string.IsNullOrWhiteSpace(rootExtractionDirectory))
                throw new NullReferenceException("Extraction Directory not set");

            if (!rootExtractionDirectory.StartsWith("\\"))
                if (!Directory.Exists(rootExtractionDirectory))
                    throw new DirectoryNotFoundException("Root directory \"" + rootExtractionDirectory + "\" does not exist");

            root = new DirectoryInfo(Path.Combine(rootExtractionDirectory, ExtractionSubFolderName));
            if (!root.Exists)
                root.Create();

            string subdirectoryName = GetExtractionDirectoryPrefix(configuration);

            if (!Directory.Exists(Path.Combine(root.FullName, subdirectoryName)))
                extractionDirectory = root.CreateSubdirectory(subdirectoryName);
            else
                extractionDirectory = new DirectoryInfo(Path.Combine(root.FullName, subdirectoryName));
        }

        public static string GetExtractionDirectoryPrefix(IExtractionConfiguration configuration)
        {
            return StandardExtractionPrefix + configuration.ID;
        }

        public static ExtractionDirectory GetForDifferentDate(string rootExtractionDirectory, IExtractionConfiguration configuration, DateTime extractDate)
        {
            return new ExtractionDirectory(rootExtractionDirectory, configuration, extractDate);
        }

        public DirectoryInfo GetDirectoryForDataset(IExtractableDataSet dataset)
        {
            if(dataset.ToString().Equals(CustomCohortDataFolderName))
                throw new Exception("You cannot call a dataset '"+CustomCohortDataFolderName+"' because this string is reserved for cohort custom data the system spits out itself");

            string reason;
            if(!Catalogue.IsAcceptableName(dataset.Catalogue.Name,out reason))
                throw new NotSupportedException("Cannot extract dataset " + dataset + " because it points at Catalogue with an invalid name, name is invalid because:" + reason);

            var datasetDirectory = dataset.ToString();
            try
            {
                return extractionDirectory.CreateSubdirectory(datasetDirectory);
            }
            catch (Exception e)
            {
                throw new Exception("Could not create a directory called '" + datasetDirectory +"' as a subfolder of Project extraction directory " + extractionDirectory.Root ,e);
            }
        }

        public DirectoryInfo GetGlobalsDirectory()
        {
            return extractionDirectory.CreateSubdirectory(GlobalsDataFolderName);
        }

        public static bool IsOwnerOf(IExtractionConfiguration configuration, DirectoryInfo directory)
        {
            //they passed a root directory like c:\bob?
            if (directory.Parent == null)
                return false;

            //The configuration number matches but directory isn't the currently configured Project extraction directory
            IProject p = configuration.Project;

            if (directory.Parent.FullName != Path.Combine(p.ExtractionDirectory, ExtractionSubFolderName))
                return false;
            
            return directory.Name.StartsWith(StandardExtractionPrefix + configuration.ID);
        }

        public DirectoryInfo GetDirectoryForCohortCustomData()
        {
            return extractionDirectory.CreateSubdirectory(CustomCohortDataFolderName);
        }

        public DirectoryInfo GetDirectoryForMasterData()
        {
            return extractionDirectory.CreateSubdirectory(MasterDataFolderName);
        }

        public static void CleanupExtractionDirectory(object sender, string extractionDirectory, IEnumerable<IExtractionConfiguration> configurations, IDataLoadEventListener listener)
        {
            DirectoryInfo projectExtractionDirectory = new DirectoryInfo(Path.Combine(extractionDirectory, ExtractionSubFolderName));
            var directoriesToDelete = new List<DirectoryInfo>();
            var filesToDelete = new List<FileInfo>();

            foreach (var extractionConfiguration in configurations)
            {
                var config = extractionConfiguration;
                var directoryInfos = projectExtractionDirectory.GetDirectories().Where(d => IsOwnerOf(config, d));

                foreach (DirectoryInfo toCleanup in directoryInfos)
                    AddDirectoryToCleanupList(toCleanup, true, directoriesToDelete, filesToDelete);
            }

            foreach (var fileInfo in filesToDelete)
            {
                listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Information, "Deleting: " + fileInfo.FullName));
                try
                {
                    fileInfo.Delete();
                }
                catch (Exception e)
                {
                    listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Error, "Error deleting: " + fileInfo.FullName, e));
                }
            }

            foreach (var directoryInfo in directoriesToDelete)
            {
                listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Information, "Recursively deleting folder: " + directoryInfo.FullName));
                try
                {
                    directoryInfo.Delete(true);
                }
                catch (Exception e)
                {
                    listener.OnNotify(sender, new NotifyEventArgs(ProgressEventType.Error, "Error deleting: " + directoryInfo.FullName, e));
                }
            }
        }

        private static void AddDirectoryToCleanupList(DirectoryInfo toCleanup, bool isRoot, List<DirectoryInfo> directoriesToDelete, List<FileInfo> filesToDelete)
        {
            //only add root folders to the delete queue
            if (isRoot)
                if (!directoriesToDelete.Any(dir => dir.FullName.Equals(toCleanup.FullName))) //dont add the same folder twice
                    directoriesToDelete.Add(toCleanup);

            filesToDelete.AddRange(toCleanup.EnumerateFiles());

            foreach (var dir in toCleanup.EnumerateDirectories())
                AddDirectoryToCleanupList(dir, false, directoriesToDelete, filesToDelete);
        }
    }
}
