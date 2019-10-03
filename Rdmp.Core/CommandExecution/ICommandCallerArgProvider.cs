using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using MapsDirectlyToDatabaseTable;
using ReusableLibraryCode.CommandExecution;
using ReusableLibraryCode.CommandExecution.AtomicCommands;

namespace Rdmp.Core.CommandExecution
{
    public interface ICommandCallerArgProvider
    {
        /// <summary>
        /// Returns a dictionary of methods to call for each type of constructor parameter needed.  If no Type
        /// exists for the parameter Type then the constructor will not be supported by the <see cref="ICommandCallerArgProvider"/>
        /// </summary>
        /// <returns></returns>
        Dictionary<Type, Func<object>> GetDelegates();

        /// <summary>
        /// Commands that should not be returned in the list of supported commands by a <see cref="CommandCaller"/>
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetIgnoredCommands();

        object PickMany(ParameterInfo parameterInfo, Type arrayElementType,IMapsDirectlyToDatabaseTable[] availableObjects);

        object PickOne(ParameterInfo parameterInfo, Type paramType, IMapsDirectlyToDatabaseTable[] availableObjects);

        DirectoryInfo PickDirectory(ParameterInfo parameterInfo, Type paramType);

        /// <summary>
        /// Called when the user attempts to run a command marked <see cref="ICommandExecution.IsImpossible"/>
        /// </summary>
        /// <param name="instance"></param>
        void OnCommandImpossible(IAtomicCommand instance);

        /// <summary>
        /// Called when a command completes successfully
        /// </summary>
        /// <param name="instance"></param>
        void OnCommandFinished(IAtomicCommand instance);

        /// <summary>
        /// Called when a command execute call completes with exception
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="exception"></param>
        void OnCommandExecutionException(IAtomicCommand instance, Exception exception);

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