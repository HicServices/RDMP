using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using CatalogueLibrary.Data;
using CatalogueLibrary.Reports.Exceptions;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using DataExportLibrary;
using DataExportLibrary.Repositories;
using HIC.Common.Validation.Dependency;
using MapsDirectlyToDatabaseTable;

namespace RDMPStartup
{
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
            catch (Exception )
            {
                CatalogueRepository = null;
            }
            
            try
            {
                DataExportRepository =  string.IsNullOrWhiteSpace(dataExportConnectionString) ? null : new DataExportRepository(new SqlConnectionStringBuilder(dataExportConnectionString), CatalogueRepository);
            }
            catch (Exception)
            {
                DataExportRepository = null;
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

            //and add one of this type if there isn't already one of that type
            if (finder.OtherDependencyFinders.All(f => f.GetType() != typeof(BetweenCatalogueAndDataExportObscureDependencyFinder)))
                finder.OtherDependencyFinders.Add(new BetweenCatalogueAndDataExportObscureDependencyFinder(this));

            //same again in green, but for validationxml dependencies
            if (finder.OtherDependencyFinders.All(f => f.GetType() != typeof(ValidationXMLObscureDependencyFinder)))
                finder.OtherDependencyFinders.Add(new ValidationXMLObscureDependencyFinder(this));
        }


        public IMapsDirectlyToDatabaseTable GetArbitraryDatabaseObject(string repositoryTypeName, string databaseObjectTypeName, int objectId)
        {
            IRepository repository = GetRepository(repositoryTypeName);
            Type objectType = GetTypeByName(databaseObjectTypeName, typeof(IMapsDirectlyToDatabaseTable));
            
            if (!repository.StillExists(objectType, objectId))
                return null;

            return repository.GetObjectByID(objectType, objectId);
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

        private Type GetTypeByName(string s, Type expectedBaseClassType)
        {
            var toReturn = CatalogueRepository.MEF.GetTypeByNameFromAnyLoadedAssembly(s);

            if (toReturn == null)
                throw new TypeLoadException("Could not find Type called '" + s + "'");

            if (expectedBaseClassType != null)
                if (!expectedBaseClassType.IsAssignableFrom(toReturn))
                    throw new TypeLoadException("Persistence string included a reference to Type '" + s + "' which we managed to find but it did not match an expected base Type (" + expectedBaseClassType + ")");

            return toReturn;
        }
    }
}