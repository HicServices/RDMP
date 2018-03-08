using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Reports;
using CatalogueLibrary.Repositories;
using DataExportLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using Microsoft.Win32;
using ReusableLibraryCode.Checks;
using ReusableUIComponents.Settings;

namespace RDMPStartup
{
    /// <summary>
    /// Records connection strings to the Catalogue and DataExport databases (See LinkedRepositoryProvider) in the user settings file for the current
    /// user.
    /// 
    /// Use properties CatalogueRepository and DataExportRepository for interacting with objects saved in those databases (and to create new ones).
    /// </summary>
    public class UserSettingsRepositoryFinder : IRDMPPlatformRepositoryServiceLocator
    {
        private LinkedRepositoryProvider _linkedRepositoryProvider;

        public CatalogueRepository CatalogueRepository
        {
            get
            {
                if(_linkedRepositoryProvider == null)
                    RefreshRepositoriesFromUserSettings();

                if (_linkedRepositoryProvider == null)
                    throw new Exception("RefreshRepositoriesFromUserSettings failed to populate_linkedRepositoryProvider as expected ");

                return _linkedRepositoryProvider.CatalogueRepository;
            }
        }

        public IDataExportRepository DataExportRepository
        {
            get
            {
                if (_linkedRepositoryProvider == null)
                    RefreshRepositoriesFromUserSettings();

                if (_linkedRepositoryProvider == null)
                    throw new Exception("RefreshRepositoriesFromUserSettings failed to populate_linkedRepositoryProvider as expected ");

                return _linkedRepositoryProvider.DataExportRepository; 
            }
        }

        public IMapsDirectlyToDatabaseTable GetArbitraryDatabaseObject(string repositoryTypeName, string databaseObjectTypeName, int objectID)
        {
            return _linkedRepositoryProvider.GetArbitraryDatabaseObject(repositoryTypeName, databaseObjectTypeName,objectID);
        }

        public bool ArbitraryDatabaseObjectExists(string repositoryTypeName, string databaseObjectTypeName, int objectID)
        {
            return _linkedRepositoryProvider.ArbitraryDatabaseObjectExists(repositoryTypeName, databaseObjectTypeName, objectID);
        }

        public void RefreshRepositoriesFromUserSettings()
        {
            //we have mef?
            MEF mef = null;
            
            //if we have a catalogue repository with loaded MEF then grab it
            if (_linkedRepositoryProvider != null && _linkedRepositoryProvider.CatalogueRepository != null &&
                _linkedRepositoryProvider.CatalogueRepository.MEF != null)
                mef = _linkedRepositoryProvider.CatalogueRepository.MEF;

            //user must have a Catalogue
            string catalogueString = UserSettings.CatalogueConnectionString;
            
            //user may have a DataExportManager
            string dataExportManagerConnectionString = UserSettings.DataExportConnectionString;

            var newrepo = new LinkedRepositoryProvider(catalogueString, dataExportManagerConnectionString);

            //preserve the currently loaded MEF assemblies
            if (
                //if we have a new repo
                newrepo.CatalogueRepository != null &&
                
                //and the new repo doesn't have MEF loaded
                newrepo.CatalogueRepository.MEF != null && !newrepo.CatalogueRepository.MEF.HaveDownloadedAllAssemblies 
                
                //but the old one did
                && mef != null && mef.HaveDownloadedAllAssemblies) 

                //use the old MEF    
                newrepo.CatalogueRepository.MEF = mef;

            _linkedRepositoryProvider = newrepo;
        }
    }
}