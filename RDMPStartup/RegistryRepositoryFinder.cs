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

namespace RDMPStartup
{
    /// <summary>
    /// Records connection strings to the Catalogue and DataExport databases (See LinkedRepositoryProvider) in the Registry in RDMPRegistryRoot for the current
    /// user.
    /// 
    /// Use properties CatalogueRepository and DataExportRepository for interacting with objects saved in those databases (and to create new ones).
    /// </summary>
    public class RegistryRepositoryFinder : IRDMPPlatformRepositoryServiceLocator
    {
        private LinkedRepositoryProvider _linkedRepositoryProvider;

        public CatalogueRepository CatalogueRepository
        {
            get
            {
                if(_linkedRepositoryProvider == null)
                    _linkedRepositoryProvider = RefreshRepositoriesFromRegistry();

                return _linkedRepositoryProvider.CatalogueRepository;
            }
        }

        public IDataExportRepository DataExportRepository
        {
            get
            {
                if (_linkedRepositoryProvider == null)
                    _linkedRepositoryProvider = RefreshRepositoriesFromRegistry();

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

        public const string RDMPRegistryRoot = @"HKEY_CURRENT_USER\Software\HICDataManagementPlatform";

        public bool KeyExists
        {
            get { return Registry.GetValue(RDMPRegistryRoot, "CatalogueConnectionString", null) != null; }
        }

        private static readonly Dictionary<RegistrySetting, string> _registryConnectionStrings = new Dictionary<RegistrySetting, string>
        {
            {RegistrySetting.Catalogue, "CatalogueConnectionString"},
            {RegistrySetting.DataExportManager, "DataExportManagerConnectionString"}
        };
        
        private LinkedRepositoryProvider RefreshRepositoriesFromRegistry()
        {
            //we have mef?
            MEF mef = null;
            
            //if we have a catalogue repository with loaded MEF then grab it
            if (_linkedRepositoryProvider != null && _linkedRepositoryProvider.CatalogueRepository != null &&
                _linkedRepositoryProvider.CatalogueRepository.MEF != null)
                mef = _linkedRepositoryProvider.CatalogueRepository.MEF;

            //user must have a Catalogue
            string catalogueString = GetRegistryValue(RegistrySetting.Catalogue);
            
            //user may have a DataExportManager
            string dataExportManagerConnectionString = GetRegistryValue(RegistrySetting.DataExportManager);

            var newrepo = new LinkedRepositoryProvider(catalogueString, dataExportManagerConnectionString);

            //preserve the currently loaded MEF assemblies
            if (newrepo.CatalogueRepository != null && newrepo.CatalogueRepository.MEF == null && !newrepo.CatalogueRepository.MEF.HaveDownloadedAllAssemblies)
                newrepo.CatalogueRepository.MEF = mef;

            return newrepo;

        }

        private string GetRegistryValue(RegistrySetting registrySetting)
        {
            //There are no configuration settings yet available, fetch some from the registry!
            object valueRead = Registry.GetValue(RDMPRegistryRoot, _registryConnectionStrings[registrySetting], null);

            if (valueRead == null)
                return null;

            return valueRead.ToString();
        }

        public void SetRegistryValue(RegistrySetting registrySetting, string value)
        {
            //There are no configuration settings yet available, fetch some from the registry!
            Registry.SetValue(RDMPRegistryRoot, _registryConnectionStrings[registrySetting], value, RegistryValueKind.String);
            _linkedRepositoryProvider = RefreshRepositoriesFromRegistry();
        }
    }

    public enum RegistrySetting
    {
        Catalogue,
        DataExportManager
    }
}