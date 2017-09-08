using System;

namespace DataExportLibrary.DataRelease
{
    public class ReleaseEngineSettings
    {
        public bool UseProjectExtractionFolder { get; set; }
        public string CustomExtractionDirectory { get; set; }
        public bool CreateReleaseDirectoryIfNotFound { get; set; }

        public ReleaseEngineSettings()
        {
            UseProjectExtractionFolder = true;
            CreateReleaseDirectoryIfNotFound = true;
            CustomExtractionDirectory = String.Empty;
        }
    }
}