using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
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

        IServerDefaults ServerDefaults { get; }

        /// <summary>
        /// Returns a dictionary of methods to call for each type of constructor parameter needed.  If no Type
        /// exists for the parameter Type then the constructor will not be supported by the <see cref="IBasicActivateItems"/>
        /// </summary>
        /// <returns></returns>
        List<KeyValuePair<Type, Func<RequiredArgument,object>>> GetDelegates();
        
        /// <summary>
        /// Stores the location of the Catalogue / Data Export repository databases and provides access to their objects
        /// </summary>
        IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        /// <summary>
        /// Commands that should not be returned in the list of supported commands by a <see cref="CommandInvoker"/>
        /// </summary>
        /// <returns></returns>
        IEnumerable<Type> GetIgnoredCommands();

        /// <summary>
        /// Prompts the user to pick from one of the <paramref name="availableObjects"/> one or more objects.  Returns null or empty if
        /// no objects end up being selected.
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="arrayElementType"></param>
        /// <param name="availableObjects"></param>
        /// <param name="initialSearchText"></param>
        /// <returns></returns>
        IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,IMapsDirectlyToDatabaseTable[] availableObjects,string initialSearchText = null);

        IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null, bool allowAutoSelect = false);

        DirectoryInfo SelectDirectory(string prompt);

        FileInfo SelectFile(string prompt);

        FileInfo SelectFile(string prompt,string patternDescription, string pattern);
        
        /// <summary>
        /// Return all Types of the given {T} which should be <see cref="IMapsDirectlyToDatabaseTable"/>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetAll<T>();

        /// <summary>
        ///  Return all Types of the given <paramref name="t"/> which should be <see cref="IMapsDirectlyToDatabaseTable"/>
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        IEnumerable<IMapsDirectlyToDatabaseTable> GetAll(Type t);

        /// <summary>
        /// User must supply a basic value type e.g. string, double, int
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="paramType"></param>
        /// <returns></returns>
        object SelectValueType(string prompt, Type paramType);

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
        
        /// <summary>
        /// Shows the given error message to the user, optionally with the <paramref name="exception"/> stack trace / Message with high visibility
        /// </summary>
        /// <param name="errorText"></param>
        /// <param name="exception"></param>
        void ShowException(string errorText, Exception exception);

        
        /// <summary>
        /// Block until the <paramref name="task"/> is completed with optionally showing the user some kind of ongoing operation
        /// indication (ui) and letting them cancel the task with the <paramref name="cts"/>.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="task"></param>
        /// <param name="cts"></param>
        void Wait(string title, Task task, CancellationTokenSource cts);

        
        /// <summary>
        /// Requests that the activator highlight or otherwise emphasise the supplied item.  Depending on who is subscribed to this event nothing may actually happen
        /// </summary>
        void RequestItemEmphasis(object sender, EmphasiseRequest emphasiseRequest);

        /// <summary>
        /// Requests a selection of one of the values of the <see cref="Enum"/> <paramref name="enumType"/>
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="enumType"></param>
        /// <param name="chosen"></param>
        /// <returns></returns>
        bool SelectEnum(string prompt, Type enumType, out Enum chosen);
    }
}