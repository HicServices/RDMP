using CachingEngine.Requests;
using CatalogueLibrary.Data.Cache;
using CatalogueLibrary.Repositories;

namespace CachingEngine.Factories
{
    public class CacheRebuilderFactory
    {
        private readonly CatalogueRepository _repository;

        public CacheRebuilderFactory(CatalogueRepository repository)
        {
            _repository = repository;
        }

        public ICacheRebuilder Create(string typeName)
        {
            return _repository.MEF.FactoryCreateA<ICacheRebuilder>(typeName);
        }

    }
}