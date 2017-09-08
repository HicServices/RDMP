using System;

namespace DataExportLibrary.DataRelease
{
    public class ReleaseEngineConfiguration
    {
        public bool UseProjectExtractionFolder { get; set; }
        public string CustomExtractionDirectory { get; set; }
        public bool CreateReleaseDirectoryIfNotFound { get; set; }

        public ReleaseEngineConfiguration()
        {
            UseProjectExtractionFolder = true;
            CreateReleaseDirectoryIfNotFound = true;
            CustomExtractionDirectory = String.Empty;
        }
    }
}