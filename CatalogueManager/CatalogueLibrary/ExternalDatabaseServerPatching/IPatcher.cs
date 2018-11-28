using System.Reflection;

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
        /// <summary>
        /// Returns the assembly containing all the class definitions for objects stored in the database e.g. CatalogueLibrary.dll
        /// </summary>
        /// <returns></returns>
        Assembly GetHostAssembly();

        /// <summary>
        /// Returns the dot database assembly containing all the Sql scripts to run to bring the database up to the current version e.g. CatalogueLibrary.Database.dll
        /// </summary>
        /// <returns></returns>
        Assembly GetDbAssembly();
    }
}
