using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace CatalogueLibrary.DataFlowPipeline
{
    /// <summary>
    /// MEF discoverable version of for IDataFlowComponent.  Also forces you to write a Check implementation in which you confirm or deny that your component is in a runnable
    /// state.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [InheritedExport(typeof(IDataFlowComponent<>))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginDataFlowComponent<T>:IDataFlowComponent<T>,ICheckable
    {
        
    }
}