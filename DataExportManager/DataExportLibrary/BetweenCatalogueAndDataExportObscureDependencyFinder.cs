using System;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Cohort;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Data;
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

            //they are trying to delete a catalogue
            if (cata != null)
            {
                //they are deleting a catalogue! see if it has an ExtractableDataSet associated with it
                ExtractableDataSet[] dependencies = _serviceLocator.DataExportRepository.GetAllObjects<ExtractableDataSet>("WHERE Catalogue_ID = " + cata.ID).ToArray();
            
                //we have any dependant catalogues?
                if(dependencies.Any())
                    throw new Exception("Cannot delete Catalogue " + cata + " because there are ExtractableDataSets which depend on them (IDs=" +string.Join(",",dependencies.Select(ds=>ds.ID.ToString())) +")");
            }
        }

        public void HandleCascadeDeletesForDeletedObject(IMapsDirectlyToDatabaseTable oTableWrapperObject)
        {
            var cic = oTableWrapperObject as CohortIdentificationConfiguration;

            //if the object being deleted is a CohortIdentificationConfiguration (in Catalogue database) then delete the associations it has to Projects in Data Export database
            if (cic != null)
            {
                //data export functionality is not available?
                if (_serviceLocator.DataExportRepository == null)
                    return;

                //delete all associations where the cic ID matches
                foreach (
                    ProjectCohortIdentificationConfigurationAssociation association in
                        _serviceLocator.DataExportRepository
                            .GetAllObjects<ProjectCohortIdentificationConfigurationAssociation>()
                            .Where(assoc => assoc.CohortIdentificationConfiguration_ID == cic.ID))
                    association.DeleteInDatabase();
            }
        }
    }
}
