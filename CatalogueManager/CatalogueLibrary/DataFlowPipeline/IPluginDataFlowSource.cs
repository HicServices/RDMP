using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.DataFlowPipeline
{
    [InheritedExport(typeof(IDataFlowSource<>))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginDataFlowSource<T>:IDataFlowSource<T>,ICheckable
    {
        
    }
}