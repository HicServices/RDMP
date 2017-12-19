using System;
using CatalogueLibrary.Data.Pipelines;

namespace CatalogueLibrary.DataFlowPipeline.Requirements
{
    /// <summary>
    /// See DataFlowPipelineContext T Generic
    /// </summary>
    public interface IDataFlowPipelineContext
    {
        Type GetFlowType();
        bool IsAllowable(IPipeline pipeline);
    }
}