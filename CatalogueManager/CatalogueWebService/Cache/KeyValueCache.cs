using System.Collections.Generic;

namespace CatalogueWebService.Cache
{
    public class KeyValueCache<T> : ICacheProvider<T>
    {
        public Dictionary<string, T> ChartData;

        public KeyValueCache()
        {
            ChartData = new Dictionary<string, T>();
        }

        public T Get(string url)
        {
            if (!ChartData.ContainsKey(url))
                return default(T);

            return ChartData[url];
        }

        public void Set(string url, T chartData)
        {
            if (ChartData.ContainsKey(url))
                return;

            ChartData.Add(url, chartData);
        }

        public bool Contains(string url)
        {
            return ChartData.ContainsKey(url);
        }
    }
}