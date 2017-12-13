using System;
using System.Data;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace LoadModules.Generic.Attachers
{
    public class KVPAttacherPipelineUseCase : PipelineUseCase
    {
        private readonly FlatFileToLoad _file;

        public KVPAttacherPipelineUseCase(KVPAttacher kvpAttacher,FlatFileToLoad file)
        {
            _file = file;
            ExplicitDestination = kvpAttacher;
        }

        public override object[] GetInitializationObjects()
        {
            return new Object[] {_file};
        }

        public override IDataFlowPipelineContext GetContext()
        {
            var context = new DataFlowPipelineContextFactory<DataTable>().Create(PipelineUsage.FixedDestination);
            context.MustHaveSource = typeof(IDataFlowSource<DataTable>);
            
            return context;
        }
    }
}