using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MapsDirectlyToDatabaseTable;

namespace Rdmp.Core.CommandExecution
{
    public interface ICommandInvokerArgProvider
    {
        /// <summary>
        /// Returns a dictionary of methods to call for each type of constructor parameter needed.  If no Type
        /// exists for the parameter Type then the constructor will not be supported by the <see cref="ICommandInvokerArgProvider"/>
        /// </summary>
        /// <returns></returns>
        Dictionary<Type, Func<object>> GetDelegates();

        /// <summary>
        /// Commands that should not be returned in the list of supported commands by a <see cref="CommandInvoker"/>
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetIgnoredCommands();

        object PickMany(ParameterInfo parameterInfo, Type arrayElementType,IMapsDirectlyToDatabaseTable[] availableObjects);

        object SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null, bool allowAutoSelect = false);

        DirectoryInfo PickDirectory(ParameterInfo parameterInfo, Type paramType);

        
        /// <summary>
        /// Return all Types of the given {T} which also implement 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<IMapsDirectlyToDatabaseTable> GetAll<T>();

        /// <summary>
        /// User must supply a basic value type e.g. string, double, int
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <param name="paramType"></param>
        /// <returns></returns>
        object PickValueType(ParameterInfo parameterInfo, Type paramType);
    }

}