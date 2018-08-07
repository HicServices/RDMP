using System;
using System.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace LoadModules.Generic.Attachers
{
    /// <summary>
    /// Use case for the user configured pipeline for reading from a flat file.  Used by KVPAttacher (See KVPAttacher) to allow the user control over how the 
    /// source file format is read (e.g. csv, fixed width, excel etc).
    /// </summary>
    public class KVPAttacherPipelineUseCase : PipelineUseCase
    {
        public KVPAttacherPipelineUseCase(KVPAttacher kvpAttacher,FlatFileToLoad file)
        {
            ExplicitDestination = kvpAttacher;
            AddInitializationObject(file);
        }

        protected override IDataFlowPipelineContext GenerateContext()
        {
            var context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.FixedDestination);
            context.MustHaveSource = typeof(IDataFlowSource<DataTable>);

            return context;
        }
    }
}