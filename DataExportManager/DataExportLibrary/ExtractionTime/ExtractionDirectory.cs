using System;
using System.IO;
using CatalogueLibrary.Data;
using DataExportLibrary.Interfaces.Data.DataTables;
using DataExportLibrary.Interfaces.ExtractionTime;
using DataExportLibrary.Data.DataTables;

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
        public const string CustomCohortDataFolderName = "CohortCustomData";
        public const string GlobalsDataFolderName = "Globals";

        public ExtractionDirectory(string rootExtractionDirectory, IExtractionConfiguration configuration)
        {
            if(string.IsNullOrWhiteSpace(rootExtractionDirectory ))
                throw new NullReferenceException("Extraction Directory not set");

            if (!rootExtractionDirectory.StartsWith("\\"))
                if (!Directory.Exists(rootExtractionDirectory))
                    throw new DirectoryNotFoundException("Root directory \"" + rootExtractionDirectory + "\" does not exist");
            
            root = new DirectoryInfo(rootExtractionDirectory);

            string subdirectoryName = GetExtractionDirectoryPrefix(configuration) + "_" + DateTime.Now.Year + DateTime.Now.Month + DateTime.Now.Day;
            
            if (!Directory.Exists(Path.Combine(root.FullName, subdirectoryName)))
                extractionDirectory = root.CreateSubdirectory(subdirectoryName);
            else
                extractionDirectory = new DirectoryInfo(Path.Combine(root.FullName, subdirectoryName));
        }

        public static string GetExtractionDirectoryPrefix(IExtractionConfiguration configuration)
        {
            return "Extraction_" + configuration.ID;
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
            
            if(!p.ExtractionDirectory.Equals(directory.Parent.FullName))
                return false;
            
            return directory.Name.StartsWith("Extraction_" + configuration.ID + "_");
        }

        public DirectoryInfo GetDirectoryForCohortCustomData()
        {
            return extractionDirectory.CreateSubdirectory(CustomCohortDataFolderName);
        }
    }
}
