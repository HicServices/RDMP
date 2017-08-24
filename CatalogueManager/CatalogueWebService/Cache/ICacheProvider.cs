namespace CatalogueWebService.Cache
{
    public interface ICacheProvider<T>
    {
        T Get(string key);
        void Set(string key, T value);
        bool Contains(string key);
    }
}