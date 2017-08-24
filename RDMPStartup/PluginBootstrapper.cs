using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CatalogueLibrary.Data;
using CatalogueLibrary.ExternalDatabaseServerPatching;
using CatalogueLibrary.Repositories;
using CatalogueLibrary.Repositories.Construction;
using MapsDirectlyToDatabaseTable.Versioning;

namespace RDMPStartup
{
    public class PluginBootstrapper
    {
        public string[] PatcherTypesToIgnore { get; set; }

        private readonly CatalogueRepository _repository;

        public PluginBootstrapper(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public IEnumerable<Type> FindPatchers()
        {
            return _repository.MEF.GetTypes<IPatcher>().Where(type => type.IsPublic);
        }

        private readonly ObjectConstructor _constructor = new ObjectConstructor();

        public IPatcher Create(Type patcherType)
        {
            return (IPatcher) _constructor.Construct(patcherType, _repository, false);
        }
    }
}