using System.Collections.Generic;

namespace CatalogueLibrary.Data.DataLoad
{
    public interface IArgumentHost
    {
        IEnumerable<IArgument> GetAllArguments();
        IArgument CreateNewArgument();

        string GetClassNameWhoArgumentsAreFor();
    }
}