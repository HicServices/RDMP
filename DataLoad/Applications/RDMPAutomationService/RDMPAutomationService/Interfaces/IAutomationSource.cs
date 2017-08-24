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
    public interface IAutomationSource : IDataFlowSource<OnGoingAutomationTask>, IPipelineRequirement<AutomationServiceSlot>
    {

    }
}
