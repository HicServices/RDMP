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
    public class ObjectSharingObscureDependencyFinder : IObscureDependencyFinder
    {
        private readonly IRDMPPlatformRepositoryServiceLocator _repositoryLocator;

        public ObjectSharingObscureDependencyFinder(IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _repositoryLocator = repositoryLocator;
        }

        public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            if(_repositoryLocator.CatalogueRepository.IsExportedObject(oTableWrapperObject))
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
