using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// MEF discoverable version of IDataFlowSource
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [InheritedExport(typeof(IDataFlowSource<>))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginDataFlowSource<T>:IDataFlowSource<T>,ICheckable
    {
        
    }
}