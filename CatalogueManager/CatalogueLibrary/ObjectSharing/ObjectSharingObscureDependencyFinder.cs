using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.ObjectSharing
{
    /// <summary>
    /// Handles preventing deletion of shareable references to existing classes e.g. if a Catalogue is shared (has an entry in ObjectExport table) then you
    /// cannot delete it.  Also handles cascading deletes of imported classes e.g. if a Catalogue was imported from somewhere else (has an entry in ObjectImport) and
    /// then you delete it the ObjectImport reference will also be deleted.
    /// </summary>
    public class ObjectSharingObscureDependencyFinder : IObscureDependencyFinder
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

        public ObjectSharingObscureDependencyFinder(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
        }

        public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            if(_repositoryLocator.CatalogueRepository.ShareManager.IsExportedObject(oTableWrapperObject))
                throw new Exception("You cannot Delete '" + oTableWrapperObject + "' because it is an Exported object declared in the ObjectExport table");
        }

        public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            if (oTableWrapperObject.GetType() != typeof (ObjectImport))
            {
                foreach (var import in _repositoryLocator.CatalogueRepository.GetAllObjects<ObjectImport>())
                {
                    if (!import.LocalObjectStillExists(_repositoryLocator))
                        import.DeleteInDatabase();
                }

            }
        }
    
    }
}
