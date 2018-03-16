using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data.Automation;
using CatalogueLibrary.DataFlowPipeline;
using CatalogueLibrary.DataFlowPipeline.Requirements;

namespace RDMPAutomationService.Interfaces
{
    /// <summary>
    /// Automation service pipeline source component responsible for deciding when new automation jobs are available for processing.  GetChunk will be polled. 
    /// Once you identify a descrete job (e.g. run an DQE evaluation on a dataset), construct your corresponding IAutomateable (e.g. AutomatedDQERun) and return
    /// IAutomateable.GetTask().
    /// 
    /// IMPORTANT: GetChunk will be called regularly so ensure that once you have released an OnGoingAutomationTask, you mark that work package as ongoing so you
    /// do not release the same / semantically similar work package again into the automation pipeline.
    /// </summary>
    public interface IAutomationSource : IDataFlowSource<OnGoingAutomationTask>, IPipelineRequirement<AutomationServiceSlot>
    {

    }
}
