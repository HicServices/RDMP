using System.Collections.Generic;
using System.ComponentModel.Composition;

namespace CatalogueManager.PluginDbInit
{
    [InheritedExport(typeof(IPluginDbInitalize))]
    public interface IPluginDbInitalize
    {
        IEnumerable<DbInitCommands> DbCommands();
    }
}