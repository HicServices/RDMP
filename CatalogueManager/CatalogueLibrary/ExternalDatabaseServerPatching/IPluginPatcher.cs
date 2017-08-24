using System.ComponentModel.Composition;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    [InheritedExport(typeof(IPatcher))]
    public interface IPluginPatcher : IPatcher
    {
    }
}