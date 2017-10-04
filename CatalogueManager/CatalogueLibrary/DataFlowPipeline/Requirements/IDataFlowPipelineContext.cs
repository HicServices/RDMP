using System;
using CatalogueLibrary.Data.Pipelines;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    public interface IDataFlowPipelineContext
    {
        Type GetFlowType();
        bool IsAllowable(IPipeline pipeline);
    }
}