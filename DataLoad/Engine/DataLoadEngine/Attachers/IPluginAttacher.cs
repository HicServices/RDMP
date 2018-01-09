using System.ComponentModel.Composition;
using ReusableLibraryCode.Checks;

namespace DataLoadEngine.Attachers
{
    /// <summary>
    /// MEF discoverable version of IAttacher (See Attacher).
    /// </summary>
    [InheritedExport(typeof(IAttacher))]
    [InheritedExport(typeof (ICheckable))]
    public interface IPluginAttacher : IAttacher
    {
        
    }
}