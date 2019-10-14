using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution
{
    public interface IBasicActivateItems
    {
        /// <summary>
        /// Component for recording object tree inheritance (for RDMPCollectionUI primarily but also for anyone who wants to know children of objects or all objects quickly without having to go back to the database)
        /// </summary>
        ICoreChildProvider CoreChildProvider { get; }

        /// <summary>
        /// Returns a dictionary of methods to call for each type of constructor parameter needed.  If no Type
        /// exists for the parameter Type then the constructor will not be supported by the <see cref="IBasicActivateItems"/>
        /// </summary>
        /// <returns></returns>
        List<KeyValuePair<Type, Func<ParameterInfo,object>>> GetDelegates();
        
        /// <summary>
        /// Stores the location of the Catalogue / Data Export repository databases and provides access to their objects
        /// </summary>
        IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        /// <summary>
        /// Commands that should not be returned in the list of supported commands by a <see cref="CommandInvoker"/>
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetIgnoredCommands();

        object PickMany(ParameterInfo parameterInfo, Type arrayElementType,IMapsDirectlyToDatabaseTable[] availableObjects);

        object SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null, bool allowAutoSelect = false);

        DirectoryInfo PickDirectory(ParameterInfo parameterInfo);

        
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

        /// <summary>
        /// Delete the <paramref name="deleteable"/> ideally asking the user for confirmation first (if appropriate)
        /// </summary>
        /// <param name="deleteable"></param>
        /// <returns></returns>
        bool DeleteWithConfirmation(IDeleteable deleteable);

        /// <summary>
        /// Offers the user a binary choice and returns true if they accept it.  This method is blocking.
        /// </summary>
        /// <param name="text">The question to pose</param>
        /// <param name="caption"></param>
        /// <returns></returns>
        bool YesNo(string text, string caption);

        /// <summary>
        /// Component for auditing errors that should be brought to the users attention subtly (e.g. if a plugin crashes while attempting to create menu items)
        /// </summary>
        ICheckNotifier GlobalErrorCheckNotifier { get; }

        /// <summary>
        /// Called when <see cref="BasicCommandExecution.Publish"/> is invoked.  Allows you to respond to publish events outside of UI code.  UI code
        /// should invoke the RefreshBus system in Rdmp.UI
        /// </summary>
        /// <param name="databaseEntity"></param>
        void Publish(DatabaseEntity databaseEntity);

        /// <summary>
        /// Display the given message to the user (e.g. in a MessageBox or out into the Console)
        /// </summary>
        /// <param name="message"></param>
        void Show(string message);

        
        /// <summary>
        /// Prompts user to provide some textual input
        /// </summary>
        /// <param name="header"></param>
        /// <param name="prompt"></param>
        /// <param name="maxLength"></param>
        /// <param name="initialText"></param>
        /// <param name="text"></param>
        /// <param name="requireSaneHeaderText"></param>
        /// <returns></returns>
        bool TypeText(string header, string prompt, int maxLength, string initialText, out string text, bool requireSaneHeaderText);

        DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription);

        DiscoveredTable SelectTable(bool allowDatabaseCreation, string taskDescription);
    }
}