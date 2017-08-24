using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.DataProvider
{
    [InheritedExport(typeof(IDataProvider))]
    [InheritedExport(typeof(ICheckable))]
    public interface IPluginDataProvider : IDataProvider
    {

    }
}