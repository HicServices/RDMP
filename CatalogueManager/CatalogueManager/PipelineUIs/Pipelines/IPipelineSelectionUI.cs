using System;
using CatalogueLibrary.Data.Pipelines;

namespace CatalogueManager.PipelineUIs.Pipelines
{
    public interface IPipelineSelectionUI
    {
        event EventHandler PipelineChanged;
        IPipeline Pipeline { get; set; }
        
        void CollapseToSingleLineMode();
    }
}