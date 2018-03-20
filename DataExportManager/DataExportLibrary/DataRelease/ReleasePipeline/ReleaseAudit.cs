using System.IO;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    /// <summary>
    /// Nothing to see here
    /// </summary>
    public class ReleaseAudit
    {
        public DirectoryInfo ReleaseFolder { get; set; }
        public DirectoryInfo SourceGlobalFolder { get; set; }
    }
}