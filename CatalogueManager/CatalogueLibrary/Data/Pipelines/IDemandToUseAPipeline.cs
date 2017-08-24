using System.Collections.Generic;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace CatalogueLibrary.Data.Pipelines
{
    public interface IDemandToUseAPipeline<T>
    {
        DataFlowPipelineContext<T> GetContext();
        IDataFlowSource<T> GetFixedSourceIfAny();
        IDataFlowDestination<T> GetFixedDestinationIfAny();

        List<object> GetInputObjectsForPreviewPipeline();


    }
}