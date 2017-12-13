using System.ComponentModel.Composition;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    /// <summary>
    /// MEF discoverable version of IPatcher
    /// </summary>
    [InheritedExport(typeof(IPatcher))]
    public interface IPluginPatcher : IPatcher
    {
    }
}