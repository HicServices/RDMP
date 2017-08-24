using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.Attachers
{
    [InheritedExport(typeof(IAttacher))]
    [InheritedExport(typeof (ICheckable))]
    public interface IPluginAttacher : IAttacher
    {
        
    }
}