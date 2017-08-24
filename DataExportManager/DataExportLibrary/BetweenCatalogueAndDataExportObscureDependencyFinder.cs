using System;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data.DataTables;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;

namespace DataExportLibrary
{
    public class BetweenCatalogueAndDataExportObscureDependencyFinder : IObscureDependencyFinder
    {
        private readonly IDataExportRepositoryServiceLocator _serviceLocator;

        public BetweenCatalogueAndDataExportObscureDependencyFinder(IDataExportRepositoryServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public void ThrowIfDeleteDisallowed(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            var cata = oTableWrapperObject as Catalogue;

            //if there isn't a data export database then we don't care, delete away
            if (_serviceLocator.DataExportRepository == null)
                return;

            //they are trying to delete something that isn't a Catalogue thats fine too
            if (cata == null) return;
            
            //they are deleting a catalogue! see if it has an ExtractableDataSet associated with it
            ExtractableDataSet[] dependencies = _serviceLocator.DataExportRepository.GetAllObjects<ExtractableDataSet>("WHERE Catalogue_ID = " + cata.ID).ToArray();
            

            //we have any dependant catalogues?
            if(dependencies.Any())
                throw new Exception("Cannot delete Catalogue " + cata + " because there are ExtractableDataSets which depend on them (IDs=" +string.Join(",",dependencies.Select(ds=>ds.ID.ToString())) +")");
        }

        public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            
        }
    }
}
