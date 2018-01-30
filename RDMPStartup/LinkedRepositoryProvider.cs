using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.ObjectSharing;
using CatalogueLibrary.Reports.Exceptions;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary;
using DataExportLibrary.Repositories;
using HIC.Common.Validation.Dependency;
using MapsDirectlyToDatabaseTable;

namespace RDMPStartup
{
    /// <summary>
    /// Records the location of the Catalogue and DataExport databases in which RDMP stores all configuration information (what datasets there are, what extraction
    /// projects there are, what IFilters are available etc - literally everything, just look at who inherits from IMapsDirectlyToDatabaseTable!).
    /// 
    /// See also RegistryRepositoryFinder
    /// </summary>
    public class LinkedRepositoryProvider : IRDMPPlatformRepositoryServiceLocator
    {
        public CatalogueRepository CatalogueRepository { get; private set; }
        public IDataExportRepository DataExportRepository { get; private set; }

        private List<IPluginRepositoryFinder> _pluginRepositoryFinders = null;

        public LinkedRepositoryProvider(string catalogueConnectionString, string dataExportConnectionString)
        {
            try
            {
                CatalogueRepository = string.IsNullOrWhiteSpace(catalogueConnectionString)
                    ? null
                    : new CatalogueRepository(new SqlConnectionStringBuilder(catalogueConnectionString));
            }
            catch (SourceCodeNotFoundException)
            {
                throw;
            }
            catch (Exception)
            {
                CatalogueRepository = null;
                throw;
            }
            
            try
            {
                DataExportRepository =  string.IsNullOrWhiteSpace(dataExportConnectionString) ? null : new DataExportRepository(new SqlConnectionStringBuilder(dataExportConnectionString), CatalogueRepository);
            }
            catch (Exception)
            {
                DataExportRepository = null;
                throw;
            }

            if (CatalogueRepository != null)
                ConfigureObscureDependencies();
        }

        public LinkedRepositoryProvider(string catalogueConnectionString, string dataExportConnectionString, out Exception catalogueException, out Exception dataExportException)
        {
            catalogueException = null;
            dataExportException = null;

            try
            {
                CatalogueRepository = string.IsNullOrWhiteSpace(catalogueConnectionString) ? null : new CatalogueRepository(new SqlConnectionStringBuilder(catalogueConnectionString));
            }
            catch (Exception e)
            {
                catalogueException = e;
                CatalogueRepository = null;
            }

            try
            {
                DataExportRepository = string.IsNullOrWhiteSpace(dataExportConnectionString) ? null : new DataExportRepository(new SqlConnectionStringBuilder(dataExportConnectionString), CatalogueRepository);
            }
            catch (Exception e)
            {
                dataExportException = e;
                DataExportRepository = null;
            }

            if (CatalogueRepository != null)
                ConfigureObscureDependencies();
        }

        private void ConfigureObscureDependencies()
        {
            //get the catalogues obscure dependency finder
            var finder = (CatalogueObscureDependencyFinder)CatalogueRepository.ObscureDependencyFinder;

            finder.AddOtherDependencyFinderIfNotExists<BetweenCatalogueAndDataExportObscureDependencyFinder>(this);
            finder.AddOtherDependencyFinderIfNotExists<ValidationXMLObscureDependencyFinder>(this);
            finder.AddOtherDependencyFinderIfNotExists<ObjectSharingObscureDependencyFinder>(this);

            if(DataExportRepository == null)
                return;

            if ( DataExportRepository.ObscureDependencyFinder == null)
                DataExportRepository.ObscureDependencyFinder = new ObjectSharingObscureDependencyFinder(this);
            else
                if(!(DataExportRepository.ObscureDependencyFinder is ObjectSharingObscureDependencyFinder))
                    throw new Exception("Expected DataExportRepository.ObscureDependencyFinder to be an ObjectSharingObscureDependencyFinder");
        }

        

        public IMapsDirectlyToDatabaseTable GetArbitraryDatabaseObject(string repositoryTypeName, string databaseObjectTypeName, int objectId)
        {
            IRepository repository = GetRepository(repositoryTypeName);
            Type objectType = GetTypeByName(databaseObjectTypeName, typeof(IMapsDirectlyToDatabaseTable));
            
            if (!repository.StillExists(objectType, objectId))
                return null;

            return repository.GetObjectByID(objectType, objectId);
        }

        public bool ArbitraryDatabaseObjectExists(string repositoryTypeName, string databaseObjectTypeName, int objectID)
        {
            IRepository repository = GetRepository(repositoryTypeName);
            Type objectType = GetTypeByName(databaseObjectTypeName, typeof(IMapsDirectlyToDatabaseTable));

            return repository.StillExists(objectType, objectID);
        }

        private IRepository GetRepository(string s)
        {
            var repoType = GetTypeByName(s, typeof(IRepository));

            if (repoType == typeof(CatalogueRepository))
                return CatalogueRepository;

            if (repoType == typeof(DataExportRepository))
                return DataExportRepository;

            LoadPluginRepositoryFindersIfNotLoaded();

            foreach (IPluginRepositoryFinder repoFinder in _pluginRepositoryFinders)
            {
                if (repoFinder.GetRepositoryType().FullName.Equals(s))
                {
                    var toReturn =  repoFinder.GetRepositoryIfAny();
                    if(toReturn == null)
                        throw new NotSupportedException("IPluginRepositoryFinder '" + repoFinder + "' said that it was the correct repository finder for repository of type '" + s + "' but it was unable to find an existing repository instance (GetRepositoryIfAny returned null)");

                    return toReturn;
                }
            }

            throw new NotSupportedException("Did not know what instance of IRepository to use for IRepository Type '" + repoType + "' , expected it to either be CatalogueRepository or DataExportRepository");
        }

        private void LoadPluginRepositoryFindersIfNotLoaded()
        {
            if(_pluginRepositoryFinders !=null)
                return;
            
            _pluginRepositoryFinders = new List<IPluginRepositoryFinder>();
            var constructor = new ObjectConstructor();

            //it's a plugin?
            foreach (Type type in CatalogueRepository.MEF.GetTypes<IPluginRepositoryFinder>())
                _pluginRepositoryFinders.Add((IPluginRepositoryFinder) constructor.Construct(type, this));
        }
        
        Dictionary<string, Type> _cachedTypesByNameDictionary = new Dictionary<string, Type>();

        private Type GetTypeByName(string s, Type expectedBaseClassType)
        {
            if (_cachedTypesByNameDictionary.ContainsKey(s))
                return _cachedTypesByNameDictionary[s];
               
            var toReturn = CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(s,expectedBaseClassType);

            if (toReturn == null)
                throw new TypeLoadException("Could not find Type called '" + s + "'");

            if (expectedBaseClassType != null)
                if (!expectedBaseClassType.IsAssignableFrom(toReturn))
                    throw new TypeLoadException("Found Type '" + s + "' which we managed to find but it did not match an expected base Type (" + expectedBaseClassType + ")");

            //cache known type to not hammer reflection all the time!
            _cachedTypesByNameDictionary.Add(s, toReturn);

            return toReturn;
        }
    }
}