using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.DataFlowPipeline;
using ReusableLibraryCode.Checks;

namespace RDMPAutomationService.Interfaces
{
    /// <summary>
    /// MEF discoverable version of IAutomationSource
    /// </summary>
    [InheritedExport(typeof(IDataFlowSource<OnGoingAutomationTask>))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginAutomationSource : IAutomationSource
    {
    }
}
