using System;
using System.Collections.Generic;
using MapsDirectlyToDatabaseTable;

namespace CatalogueLibrary.Repositories
{
    /// <summary>
    /// Use when you have an already initialized set of repositories and only want to fetch objects from the catalogue/data export repositories
    /// </summary>
    public class RepositoryProvider : IRDMPPlatformRepositoryServiceLocator
    {
        public CatalogueRepository CatalogueRepository { get; protected set; }
        public IDataExportRepository DataExportRepository { get; protected set; }
        readonly Dictionary<string, Type> _cachedTypesByNameDictionary = new Dictionary<string, Type>();

        /// <summary>
        /// Use when you have an already initialized set of repositories.  Sets up the class to fetch objects from the Catalogue/Data export databases only.
        /// 
        /// <para>If possible consider using LinkedRepositoryProvider or Startup (these support plugin repositories, DQE repository etc)</para>
        /// 
        /// </summary>
        /// <param name="dataExportRepository"></param>
        public RepositoryProvider(IDataExportRepository dataExportRepository)
        {
            CatalogueRepository = dataExportRepository.CatalogueRepository;
            DataExportRepository = dataExportRepository;
        }

        protected RepositoryProvider()
        {
            
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

        protected virtual IRepository GetRepository(string s)
        {
            var repoType = GetTypeByName(s, typeof(IRepository));

            if (typeof(ICatalogueRepository).IsAssignableFrom(repoType))
                return CatalogueRepository;

            if (typeof(IDataExportRepository).IsAssignableFrom(repoType))
                return DataExportRepository;

            throw new NotSupportedException("Did not know what instance of IRepository to use for IRepository Type '" + repoType + "' , expected it to either be CatalogueRepository or DataExportRepository");

        }
        
        object oLockDictionary = new object();
        private Type GetTypeByName(string s, Type expectedBaseClassType)
        {
            Type toReturn;
            lock (oLockDictionary)
            {
                if (_cachedTypesByNameDictionary.ContainsKey(s))
                    return _cachedTypesByNameDictionary[s];

                toReturn = CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(s, expectedBaseClassType);

                if (toReturn == null)
                    throw new TypeLoadException("Could not find Type called '" + s + "'");

                if (expectedBaseClassType != null)
                    if (!expectedBaseClassType.IsAssignableFrom(toReturn))
                        throw new TypeLoadException("Found Type '" + s +
                                                    "' which we managed to find but it did not match an expected base Type (" +
                                                    expectedBaseClassType + ")");

                //cache known type to not hammer reflection all the time!
                _cachedTypesByNameDictionary.Add(s, toReturn);
            }
            return toReturn;
        }
    }
}