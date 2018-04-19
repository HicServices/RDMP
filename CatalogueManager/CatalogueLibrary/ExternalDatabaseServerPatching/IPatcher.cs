using System.ComponentModel.Composition;
using System.Reflection;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    /// <summary>
    /// Identifies databases belong to a specific .Database assembly that might need patching at Startup.  Document the host and database assembly classes (e.g. CatalogueLibrary
    /// and CatalogueLibrary.Database).
	///
    /// <para>If you are writing a plugin you should use IPluginPatcher instead which is MEF discoverable</para>
    /// </summary>
    public interface IPatcher
    {
        Assembly GetHostAssembly();
        Assembly GetDbAssembly();
    }
}
