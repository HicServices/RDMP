using System;
using CatalogueLibrary.Data;

namespace DataExportLibrary.DataRelease
{
    public class ReleaseEngineSettings
    {
        [DemandsInitialization("Output folder")]
        public bool UseProjectExtractionFolder { get; set; }

        [DemandsInitialization("Output folder")]
        public string CustomExtractionDirectory { get; set; }

        [DemandsInitialization("Output folder")]
        public bool CreateReleaseDirectoryIfNotFound { get; set; }

        public ReleaseEngineSettings()
        {
            UseProjectExtractionFolder = true;
            CreateReleaseDirectoryIfNotFound = true;
            CustomExtractionDirectory = String.Empty;
        }
    }
}