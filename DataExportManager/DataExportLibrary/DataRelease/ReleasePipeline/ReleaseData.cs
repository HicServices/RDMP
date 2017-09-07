using System.Collections.Generic;
using System.IO;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class ReleaseData
    {
        public HashSet<FileInfo> FilesToRelease { get; set; }
        public Dictionary<IExtractionConfiguration, List<ReleasePotential>> ConfigurationsForRelease { get; set; }
        public ReleaseEnvironmentPotential EnvironmentPotential { get; set; }
    }
}