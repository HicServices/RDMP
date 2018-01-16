using System.Collections.Generic;
using System.Runtime.Remoting.Contexts;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    /// <summary>
    /// Collection passed down the Release Pipeline.  Release is the process of taking extracted files for a Project, bundling them together into a release
    /// structure and sending that artifact to a release directory.  The Releasability of each dataset in the extraction is checked prior to release to confirm 
    /// that the extracted files match the current system configuration and that all expected files are there (See ReleasePotential).  In addition the ticketing
    /// system (if any) is consulted to confirm that it is happy for the collection to be released (See EnvironmentPotential)
    /// </summary>
    public class ReleaseData
    {
        public Dictionary<IExtractionConfiguration, List<ReleasePotential>> ConfigurationsForRelease { get; set; }
        public ReleaseEnvironmentPotential EnvironmentPotential { get; set; }
        public ReleaseState ReleaseState { get; set; }
    }
}