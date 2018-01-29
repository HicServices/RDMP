using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.DataProvider
{
    /// <summary>
    /// MEF discoverable (Plugin) version of IDataProvider
    /// </summary>
    [InheritedExport(typeof(IDataProvider))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginDataProvider : IDataProvider
    {

    }
}