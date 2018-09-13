using System.IO;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;

namespace Sharing.Dependency
{
    
    /// <summary>
    /// Facilitiates importing plugins from a remote contributor and creating the local copies of the Plugin dlls in the local CatalogueRepository database.
    /// </summary>
    public class SharedPluginImporter
    {
        private readonly ShareManager _shareManager;

        public SharedPluginImporter(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _shareManager = new ShareManager(repositoryLocator);
        }

        public Plugin Import(Stream stream)
        {
            return _shareManager.ImportSharedObject(stream, deleteExisting: true).OfType<Plugin>().Single();
        }
    }
}
