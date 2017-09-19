using System;
using System.Collections.Generic;
using CatalogueLibrary.Data.Pipelines;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace RDMPObjectVisualisation.Pipelines
{
    public interface IPipelineSelectionUI
    {
        event EventHandler PipelineChanged;
        IPipeline Pipeline { get; set; }
        void SetContext(IDataFlowPipelineContext context);

        List<object> InitializationObjectsForPreviewPipeline { get; set; }

        void CollapseToSingleLineMode();
    }
}