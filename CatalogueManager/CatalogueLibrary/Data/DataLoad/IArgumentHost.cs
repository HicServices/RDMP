using System;
using System.Collections.Generic;

namespace CatalogueLibrary.Data.DataLoad
{
    /// <summary>
    /// Interface for all classes which make use of the IArgument/Argument system to record user configured values of [DemandsInitialization] properties.  Allows you
    /// to get the currently configured arguments and create new ones.
    /// </summary>
    public interface IArgumentHost
    {
        /// <summary>
        /// Gets all persisted property values for the class referenced by the <see cref="IArgumentHost"/> (i.e. <see cref="GetClassNameWhoArgumentsAreFor"/>).  Each argument will
        ///  satisfy one public property on the class decorated with <see cref="DemandsInitializationAttribute"/>
        /// </summary>
        /// <returns></returns>
        IEnumerable<IArgument> GetAllArguments();

        /// <summary>
        /// Creates a new empty <see cref="IArgument"/> of a Type compatible with the  <see cref="IArgumentHost"/> e.g. <see cref="ProcessTask"/> would create a <see cref="ProcessTaskArgument"/>
        /// 
        /// <para>It is better to use <see cref="ArgumentFactory"/></para>
        /// </summary>
        /// <returns></returns>
        IArgument CreateNewArgument();

        /// <summary>
        /// The name of the class for which the <see cref="IArgumentHost"/> stores arguments for and is responsible for hosting e.g. a <see cref="ProcessTask"/> could be hosting an blueprint
        /// for a specific configuration of IAttacher, IDataProvider etc.
        /// </summary>
        /// <returns></returns>
        string GetClassNameWhoArgumentsAreFor();

        /// <inheritdoc cref="ArgumentFactory.CreateArgumentsForClassIfNotExistsGeneric"/>
        IArgument[] CreateArgumentsForClassIfNotExists<T>();

        /// <inheritdoc cref="ArgumentFactory.CreateArgumentsForClassIfNotExistsGeneric"/>
        IArgument[] CreateArgumentsForClassIfNotExists(Type underlyingComponentType);
    }
}