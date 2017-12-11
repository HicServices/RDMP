using System.Reflection;
using CatalogueLibrary.Data;

namespace CatalogueLibrary.ExternalDatabaseServerPatching
{
    /// <summary>
    /// Identifies databases belong to a specific .Database assembly that might need patching at Startup.  Document the host and database assembly classes (e.g. CatalogueLibrary
    ///  and CatalogueLibrary.Database).
    /// Identifies databases which belong to a specific .Database assembly that might need patching at Startup.  Document the host and database assembly classes 
    /// (e.g. CatalogueLibrary and CatalogueLibrary.Database).  These databases will be checked at startup for whether they need patching and if so new scripts
    /// in the up folder will be applied and the databse version will be incremented to match the Assembly versions.
    /// 
    /// If you are writing a plugin you should use IPluginPatcher instead which is MEF discoverable
    /// </summary>
    public interface IPatcher
    {
        IExternalDatabaseServer[] FindDatabases(out Assembly hostAssembly, out Assembly dbAssembly);
    }
}