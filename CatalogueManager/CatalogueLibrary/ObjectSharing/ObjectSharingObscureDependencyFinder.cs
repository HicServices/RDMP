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
            if(_repositoryLocator.CatalogueRepository.IsImportedObject(oTableWrapperObject))
                _repositoryLocator.CatalogueRepository.GetAllObjects<ObjectImport>().Single(o=>o.IsImportedObject(oTableWrapperObject)).DeleteInDatabase();
        }
    
    }
}
