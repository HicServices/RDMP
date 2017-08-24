using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline.Requirements;
using RDMPAutomationService.Interfaces;

namespace RDMPAutomationService.Pipeline
{
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
