using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class ReleaseData
    {
        public Dictionary<IExtractionConfiguration, List<ReleasePotential>> ConfigurationsForRelease { get; set; }
        public ReleaseEnvironmentPotential EnvironmentPotential { get; set; }
        public ReleaseState ReleaseState { get; set; }
    }
}