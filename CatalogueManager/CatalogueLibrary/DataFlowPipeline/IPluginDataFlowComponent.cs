using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.DataFlowPipeline
{
    [InheritedExport(typeof(IDataFlowComponent<>))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginDataFlowComponent<T>:IDataFlowComponent<T>,ICheckable
    {
        
    }
}