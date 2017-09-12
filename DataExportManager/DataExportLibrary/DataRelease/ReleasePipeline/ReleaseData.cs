using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Contexts;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using DataExportLibrary.Interfaces.Data.DataTables;

namespace DataExportLibrary.DataRelease.ReleasePipeline
{
    public class ReleaseData
    {
        public HashSet<FileInfo> FilesToRelease { get; set; }
        public Dictionary<IExtractionConfiguration, List<ReleasePotential>> ConfigurationsForRelease { get; set; }
        public ReleaseEnvironmentPotential EnvironmentPotential { get; set; }
        public ReleaseState ReleaseState { get; set; }
    }

    public static class ReleaseContext
    {
        public static DataFlowPipelineContext<ReleaseData> Context { get; set; }

        static ReleaseContext()
        {
            var contextFactory = new DataFlowPipelineContextFactory<ReleaseData>();
            Context = contextFactory.Create(PipelineUsage.None);
            Context.CannotHave.Add(typeof(IDataFlowSource<ReleaseData>));

            Context.MustHaveDestination = typeof(IDataFlowDestination<ReleaseData>);   
        }   
    }
}