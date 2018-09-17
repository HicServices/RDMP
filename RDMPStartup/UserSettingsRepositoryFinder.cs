using System;
using CatalogueLibrary.Repositories;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.Comments;
using ReusableLibraryCode.Settings;

namespace RDMPStartup
{
    /// <summary>
    /// Records connection strings to the Catalogue and DataExport databases (See LinkedRepositoryProvider) in the user settings file for the current
    /// user.
    /// 
    /// <para>Use properties CatalogueRepository and DataExportRepository for interacting with objects saved in those databases (and to create new ones).</para>
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
            CommentStore commentStore = null;
            
            //if we have a catalogue repository with loaded MEF then grab it
            if (_linkedRepositoryProvider != null && _linkedRepositoryProvider.CatalogueRepository != null)
            {

                if(_linkedRepositoryProvider.CatalogueRepository.MEF != null)
                    mef = _linkedRepositoryProvider.CatalogueRepository.MEF;

                if (_linkedRepositoryProvider.CatalogueRepository.CommentStore != null)
                    commentStore = _linkedRepositoryProvider.CatalogueRepository.CommentStore;
            }

            //user must have a Catalogue
            string catalogueString = UserSettings.CatalogueConnectionString;
            
            //user may have a DataExportManager
            string dataExportManagerConnectionString = UserSettings.DataExportConnectionString;

            var newrepo = new LinkedRepositoryProvider(catalogueString, dataExportManagerConnectionString);

            //preserve the currently loaded MEF assemblies


            //if we have a new repo
            if (newrepo.CatalogueRepository != null)
            {
                //and the new repo doesn't have MEF loaded
                if(newrepo.CatalogueRepository.MEF != null && !newrepo.CatalogueRepository.MEF.HaveDownloadedAllAssemblies && mef != null && mef.HaveDownloadedAllAssemblies)
                    //use the old MEF    
                    newrepo.CatalogueRepository.MEF = mef;

                newrepo.CatalogueRepository.CommentStore = commentStore ?? newrepo.CatalogueRepository.CommentStore;

            }
            

            _linkedRepositoryProvider = newrepo;
        }
    }
}
