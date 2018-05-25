using CatalogueLibrary.Repositories;

namespace CachingEngine.Requests
{
    /// <summary>
    /// Cache Chunk used with the other DoNothing classes to test operations
    /// </summary>
    public class DoNothingCacheChunk : ICacheChunk
    {
        public DoNothingCacheChunk(ICatalogueRepository catalogueRepository)
        {
            Request = new CacheFetchRequest(catalogueRepository);
        }

        public int RunIteration { get; set; }
        public ICacheFetchRequest Request { get; private set; }
    }
}