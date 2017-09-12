using System;
using CatalogueLibrary.Data;

namespace DataExportLibrary.DataRelease
{
    public class ReleaseEngineSettings
    {
        [DemandsInitialization("Check to release to the project extraction folder insted of specifying a custom one")]
        public bool UseProjectExtractionFolder { get; set; }

        [DemandsInitialization("Specify a custom Release folder, ignored if 'Use Project Extraction Folder' is checked")]
        public string CustomExtractionDirectory { get; set; }

        [DemandsInitialization("If unchecked, it will report an error if the destination folder does not exists", DefaultValue = true)]
        public bool CreateReleaseDirectoryIfNotFound { get; set; }
        
        [DemandsInitialization("Delete the released files from the origin location if release is succesful", DefaultValue = true)]
        public bool DeleteFilesOnSuccess { get; set; }

        public ReleaseEngineSettings()
        {
            UseProjectExtractionFolder = true;
            CreateReleaseDirectoryIfNotFound = true;
            CustomExtractionDirectory = String.Empty;
            DeleteFilesOnSuccess = true;
        }
    }
}