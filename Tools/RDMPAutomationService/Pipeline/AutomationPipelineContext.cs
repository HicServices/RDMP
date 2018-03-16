using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using RDMPAutomationService.Interfaces;

namespace RDMPAutomationService.Pipeline
{
    /// <summary>
    /// Describes the format of components compatible with Automation Pipelines.  Specifies that the flow (T) of the pipeline is OnGoingAutomationTask objects and
    /// that the source must be IAutomationSource.
    /// </summary>
    public class AutomationPipelineContext:DataFlowPipelineContext<OnGoingAutomationTask>
    {
        public static DataFlowPipelineContext<OnGoingAutomationTask> Context;

        static AutomationPipelineContext()
        {
            var factory = new DataFlowPipelineContextFactory<OnGoingAutomationTask>();
            Context = factory.Create(PipelineUsage.FixedDestination);
            
            Context.MustHaveSource = typeof (IAutomationSource);
        }
    }
}
