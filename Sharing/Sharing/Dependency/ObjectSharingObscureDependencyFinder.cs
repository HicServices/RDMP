using System;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.ImportExport;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using Sharing.Sharing;

namespace Sharing.Dependency
{
    /// <summary>
    /// Handles preventing deletion of shareable references to existing classes e.g. if a Catalogue is shared (has an entry in ObjectExport table) then you
    /// cannot delete it.  Also handles cascading deletes of imported classes e.g. if a Catalogue was imported from somewhere else (has an entry in ObjectImport) and
    /// then you delete it the ObjectImport reference will also be deleted.
    /// </summary>
    public class ObjectSharingObscureDependencyFinder : IObscureDependencyFinder
    {
        private ShareManager _shareManager;

        public ObjectSharingObscureDependencyFinder(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _shareManager = new ShareManager(repositoryLocator);
        }

        public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            if (_shareManager.IsExportedObject(oTableWrapperObject))
                throw new Exception("You cannot Delete '" + oTableWrapperObject + "' because it is an Exported object declared in the ObjectExport table");
        }

        public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            if (oTableWrapperObject.GetType() != typeof (ObjectImport))
                _shareManager.DeleteAllOrphanImportDefinitions();
        }
    
    }
}
