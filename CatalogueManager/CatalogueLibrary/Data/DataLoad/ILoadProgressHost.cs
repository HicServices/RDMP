using System.Collections.Generic;

namespace CatalogueLibrary.Data.DataLoad
{
    public interface ILoadProgressHost
    {
        IEnumerable<ILoadProgress> GetLoadProgresses();
    }
}