using System.Collections.Generic;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Interface for all classes which make use of the IArgument/Argument system to record user configured values of [DemandsInitialization] properties.  Allows you
    /// to get the currently configured arguments and create new ones.
    /// </summary>
    public interface IArgumentHost
    {
        IEnumerable<IArgument> GetAllArguments();
        IArgument CreateNewArgument();

        string GetClassNameWhoArgumentsAreFor();
    }
}