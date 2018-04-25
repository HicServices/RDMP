using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Data.Serialization;
using CatalogueLibrary.Repositories;
using ReusableLibraryCode.Checks;

namespace Sharing.Dependency
{
    
    /// <summary>
    /// Facilitiates importing plugins from a remote contributor and creating the local copies of the Plugin dlls in the local CatalogueRepository database.
    /// </summary>
    public class SharedPluginImporter
    {
        private readonly Stream _stream;
        private ShareManager _shareManager;

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
