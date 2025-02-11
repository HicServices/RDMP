// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Aggregation;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.DataViewing;
using Rdmp.Core.Icons.IconProvision;
using Rdmp.Core.Logging;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.MapsDirectlyToDatabaseTable.Versioning;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.Core.ReusableLibraryCode.Checks;
using Rdmp.Core.ReusableLibraryCode.Comments;
using Rdmp.Core.ReusableLibraryCode.Settings;

namespace Rdmp.Core.CommandExecution;

public interface IBasicActivateItems
{
    /// <summary>
    /// True for activators that can elicit immediate responses from users.  False for activators designed to run unattended e.g. command line/scripting
    /// </summary>
    bool IsInteractive { get; }

    /// <summary>
    /// APIs that supply additional commands or alter user interface logic (either CLI or windows client)
    /// </summary>
    List<IPluginUserInterface> PluginUserInterfaces { get; }

    /// <summary>
    /// Component for fetching xmldoc comments from the codebase
    /// </summary>
    public CommentStore CommentStore { get; }

    /// <summary>
    /// When implemented should display a graph of the data in <paramref name="aggregate"/>
    /// </summary>
    /// <param name="aggregate"></param>
    void ShowGraph(AggregateConfiguration aggregate);


    /// <summary>
    /// Display information about the logged activities of the <paramref name="rootObject"/>
    /// </summary>
    /// <param name="rootObject"></param>
    void ShowLogs(ILoggedActivityRootObject rootObject);

    /// <summary>
    /// Display top down information about logged activities
    /// </summary>
    /// <param name="loggingServer">The server to query for logs</param>
    /// <param name="filter"></param>
    void ShowLogs(ExternalDatabaseServer loggingServer, LogViewerFilter filter);


    /// <summary>
    /// True if <see cref="Activate(object)"/> will work for the object
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    bool CanActivate(object o);

    /// <summary>
    /// True to prompt Yes/No and offer common fixes for being unable to directly delete an object.  Note that <see cref="IsInteractive"/> must be true for this flag to be respected
    /// </summary>
    bool InteractiveDeletes { get; set; }

    /// <summary>
    /// Finds the nearest database object to <paramref name="publish"/> in its hierarchy and issues a publish
    /// </summary>
    /// <param name="publish"></param>
    void PublishNearest(object publish);

    /// <summary>
    /// Event triggered when objects should be brought to the users attention
    /// </summary>
    event EmphasiseItemHandler Emphasise;

    /// <summary>
    /// Show some SQL and the data that it returns.  This may be non modal
    /// </summary>
    /// <param name="collection"></param>
    void ShowData(IViewSQLAndResultsCollection collection);

    /// <summary>
    /// Show a table of data that is held in memory already.  This may be non modal
    /// </summary>
    /// <param name="dataTable"></param>
    void ShowData(DataTable dataTable);

    /// <summary>
    /// Returns the repository that stores objects of the given <paramref name="type"/> (Must be <see cref="IMapsDirectlyToDatabaseTable"/>).
    /// Throws if no compatible repo is found.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException">If no repositories are found that own the given <paramref name="type"/></exception>
    IRepository GetRepositoryFor(Type type);

    /// <summary>
    /// Component for recording object tree inheritance (for RDMPCollectionUI primarily but also for anyone who wants to know children of objects or all objects quickly without having to go back to the database)
    /// </summary>
    ICoreChildProvider CoreChildProvider { get; }

    /// <summary>
    /// Component class for discovering the default DQE, Logging servers etc configured in the current RDMP database
    /// </summary>
    IServerDefaults ServerDefaults { get; }

    /// <summary>
    /// Component for telling you whether a given DatabaseEntity is one of the current users favourite objects and for toggling it
    /// </summary>
    FavouritesProvider FavouritesProvider { get; }

    /// <summary>
    /// Provides consistent icon representations of objects
    /// </summary>
    ICoreIconProvider CoreIconProvider { get; }

    /// <summary>
    /// Returns a dictionary of methods to call for each type of constructor parameter needed.  If no Type
    /// exists for the parameter Type then the constructor will not be supported by the <see cref="IBasicActivateItems"/>
    /// </summary>
    /// <returns></returns>
    List<CommandInvokerDelegate> GetDelegates();


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
    /// Create a class capable of running a <see cref="IPipeline"/> under a given <see cref="IPipelineUseCase"/>.  This may be an async process e.g. non modal dialogues
    /// </summary>
    /// <returns></returns>
    IPipelineRunner GetPipelineRunner(DialogArgs args, IPipelineUseCase useCase, IPipeline pipeline);

    #region Select X Modal methods

    /// <summary>
    /// Prompts the user to enter a description for a cohort they are trying to create including whether it is intended to replace an old version of another cohort.
    /// </summary>
    /// <param name="externalCohortTable">Where the user will be creating the cohort</param>
    /// <param name="project">The project the cohort should be associated with</param>
    /// <param name="cohortInitialDescription">Optional initial description for the cohort which may be changed by the user</param>
    /// <returns></returns>
    CohortCreationRequest GetCohortCreationRequest(ExternalCohortTable externalCohortTable, IProject project,
        string cohortInitialDescription);



    CohortHoldoutLookupRequest GetCohortHoldoutLookupRequest(ExternalCohortTable externalCohortTable, IProject project, CohortIdentificationConfiguration cic);

    /// <summary>
    /// Show all objects in RDMP (with search).  If a single selection is made then invoke the callback
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="callback"></param>
    void SelectAnythingThen(string prompt, Action<IMapsDirectlyToDatabaseTable> callback);

    /// <inheritdoc cref="SelectAnythingThen(string, Action{IMapsDirectlyToDatabaseTable})"/>
    void SelectAnythingThen(DialogArgs args, Action<IMapsDirectlyToDatabaseTable> callback);

    /// <summary>
    /// Prompts the user to pick from one of the <paramref name="availableObjects"/> one or more objects.  Returns null or empty if
    /// no objects end up being selected.
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="arrayElementType"></param>
    /// <param name="availableObjects"></param>
    /// <param name="initialSearchText"></param>
    /// <returns></returns>
    IMapsDirectlyToDatabaseTable[] SelectMany(string prompt, Type arrayElementType,
        IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null);

    /// <inheritdoc cref="SelectMany(string, Type, IMapsDirectlyToDatabaseTable[], string)"/>
    IMapsDirectlyToDatabaseTable[] SelectMany(DialogArgs args, Type arrayElementType,
        IMapsDirectlyToDatabaseTable[] availableObjects);

    /// <summary>
    /// Prompts user to pick one of the <paramref name="availableObjects"/>
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="availableObjects">Objects that can be selected</param>
    /// <param name="initialSearchText"></param>
    /// <param name="allowAutoSelect"></param>
    /// <returns></returns>
    IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects,
        string initialSearchText = null, bool allowAutoSelect = false);

    /// <summary>
    /// Prompts user to pick one of the <paramref name="availableObjects"/>
    /// </summary>
    /// <param name="args"></param>
    /// <param name="availableObjects">Objects that can be selected</param>
    IMapsDirectlyToDatabaseTable SelectOne(DialogArgs args, IMapsDirectlyToDatabaseTable[] availableObjects);

    /// <summary>
    /// Prompts user to pick one of <paramref name="available"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prompt"></param>
    /// <param name="available"></param>
    /// <param name="selected"></param>
    /// <param name="initialSearchText"></param>
    /// <param name="allowAutoSelect">Determines behaviour when <paramref name="available"/> has only one element.  True to auto select the only object available</param>
    /// <returns>True if a selection was made</returns>
    bool SelectObject<T>(string prompt, T[] available, out T selected, string initialSearchText = null,
        bool allowAutoSelect = false) where T : class;

    /// <inheritdoc cref="SelectObject{T}(string, T[], out T, string, bool)"/>
    bool SelectObject<T>(DialogArgs args, T[] available, out T selected) where T : class;

    /// <summary>
    /// Prompts user to pick several of <paramref name="available"/> objects
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="prompt"></param>
    /// <param name="available"></param>
    /// <param name="selected"></param>
    /// <param name="initialSearchText"></param>
    bool SelectObjects<T>(string prompt, T[] available, out T[] selected, string initialSearchText = null)
        where T : class;

    /// <inheritdoc cref="SelectObjects{T}(string, T[], out T[], string)"/>
    bool SelectObjects<T>(DialogArgs args, T[] available, out T[] selected) where T : class;

    /// <summary>
    /// Prompts user to select a directory on disk
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    DirectoryInfo SelectDirectory(string prompt);

    /// <summary>
    /// Prompts user to select a file on disk (that may or may not exist yet)
    /// </summary>
    /// <param name="prompt"></param>
    /// <returns></returns>
    FileInfo SelectFile(string prompt);


    /// <summary>
    /// Prompts user to select multiple files on disk that must exist and match the <paramref name="pattern"/>
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="patternDescription">Type of file to select e.g. "Comma Separated Values"</param>
    /// <param name="pattern">Pattern to restrict files to e.g. *.csv</param>
    /// <returns>Selected files or null if no files chosen</returns>
    FileInfo[] SelectFiles(string prompt, string patternDescription, string pattern);

    /// <summary>
    /// Prompts user to select a file on disk (that may or may not exist yet) with the given pattern
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="patternDescription">Type of file to select e.g. "Comma Separated Values"</param>
    /// <param name="pattern">Pattern to restrict files to e.g. *.csv</param>
    /// <returns></returns>
    FileInfo SelectFile(string prompt, string patternDescription, string pattern);


    /// <summary>
    /// User must supply a basic value type e.g. string, double, int
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="paramType"></param>
    /// <param name="initialValue"></param>
    /// <param name="chosen"></param>
    /// <returns></returns>
    bool SelectValueType(string prompt, Type paramType, object initialValue, out object chosen);


    /// <inheritdoc cref="SelectValueType(string, Type, object, out object)"/>
    bool SelectValueType(DialogArgs args, Type paramType, object initialValue, out object chosen);


    /// <summary>
    /// Offers the user a binary choice and returns true if they consciously select a value.  This method is blocking.
    /// </summary>
    /// <param name="text">The question to pose</param>
    /// <param name="caption"></param>
    /// <param name="chosen">The answer chosen by the user</param>
    /// <returns>true if user successfully made a choice</returns>
    bool YesNo(string text, string caption, out bool chosen);

    /// <inheritdoc cref="YesNo(string, string, out bool)"/>
    bool YesNo(DialogArgs args, out bool chosen);

    /// <summary>
    /// Offers the user a binary choice returning the choice or false if user cancels (to distinguish between false and cancel
    /// use the overload).  This method is blocking.</summary>
    /// <param name="text">The question to pose</param>
    /// <param name="caption"></param>
    /// <returns>Users choice or false if user cancels</returns>
    bool YesNo(string text, string caption);

    /// <inheritdoc cref="YesNo(string, string)"/>
    bool YesNo(DialogArgs args);


    /// <summary>
    /// Offers the user a binary choice for whether they want to proceede on a dangerous/complex operation.  Returns true if
    /// environment is not interactive or an external setting auto confirms e.g. a command line argument etc
    /// </summary>
    /// <param name="text">The question to pose</param>
    /// <param name="caption"></param>
    /// <returns></returns>
    bool Confirm(string text, string caption);

    /// <inheritdoc cref="Confirm(string, string)"/>
    bool Confirm(DialogArgs args);

    /// <summary>
    /// Display the given message to the user (e.g. in a MessageBox or out into the Console)
    /// </summary>
    /// <param name="message"></param>
    void Show(string message);

    /// <summary>
    /// Display the given message to the user (e.g. in a MessageBox or out into the Console) as a warning
    /// </summary>
    /// <param name="message"></param>
    void ShowWarning(string message);


    /// <summary>
    /// Display the given message to the user (e.g. in a MessageBox or out into the Console).  If provided <paramref name="title"/> may also be featured in the presentation
    /// </summary>
    /// <param name="title"></param>
    /// <param name="message"></param>
    void Show(string title, string message);

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
    bool TypeText(string header, string prompt, int maxLength, string initialText, out string text,
        bool requireSaneHeaderText);

    /// <inheritdoc cref="TypeText(string, string, int, string, out string, bool)"/>
    bool TypeText(DialogArgs args, int maxLength, string initialText, out string text, bool requireSaneHeaderText);

    /// <summary>
    /// Prompts the user to pick a database
    /// </summary>
    /// <param name="allowDatabaseCreation"></param>
    /// <param name="taskDescription"></param>
    /// <returns></returns>
    DiscoveredDatabase SelectDatabase(bool allowDatabaseCreation, string taskDescription);

    /// <summary>
    /// Prompts user to pick a table
    /// </summary>
    /// <param name="allowDatabaseCreation"></param>
    /// <param name="taskDescription"></param>
    /// <returns></returns>
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
    /// Requests a selection of one of the values of the <see cref="Enum"/> <paramref name="enumType"/>
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="enumType"></param>
    /// <param name="chosen"></param>
    /// <returns></returns>
    bool SelectEnum(string prompt, Type enumType, out Enum chosen);

    /// <inheritdoc cref="SelectEnum(string, Type, out Enum)"/>
    bool SelectEnum(DialogArgs args, Type enumType, out Enum chosen);

    /// <summary>
    /// Requests user select a <see cref="Type"/> (optional one that is assignable to <paramref name="baseTypeIfAny"/>).
    /// This will only offer concrete classes and not abstract classes or interfaces.  To allow picking those use the overload.
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="baseTypeIfAny">Pass a base class or interface if the Type must be an inheritor / assignable to a specific Type otherwise pass null</param>
    /// <param name="chosen"></param>
    /// <returns></returns>
    bool SelectType(string prompt, Type baseTypeIfAny, out Type chosen);

    /// <inheritdoc cref="SelectValueType(string, Type, object, out object)"/>
    bool SelectType(DialogArgs args, Type baseTypeIfAny, out Type chosen);


    /// <summary>
    /// Requests user select a <see cref="Type"/> (optional one that is assignable to <paramref name="baseTypeIfAny"/>).
    /// </summary>
    /// <param name="prompt"></param>
    /// <param name="baseTypeIfAny">Pass a base class or interface if the Type must be an inheritor / assignable to a specific Type otherwise pass null</param>
    /// <param name="allowAbstract">True to offer abstract class Types as well</param>
    /// <param name="allowInterfaces">True to offer interface Types as well</param>
    /// <param name="chosen"></param>
    /// <returns></returns>
    bool SelectType(string prompt, Type baseTypeIfAny, bool allowAbstract, bool allowInterfaces, out Type chosen);

    /// <inheritdoc cref="SelectType(string, Type, bool, bool, out Type)"/>
    bool SelectType(DialogArgs args, Type baseTypeIfAny, bool allowAbstract, bool allowInterfaces, out Type chosen);

    /// <summary>
    /// Requests user select one of the <paramref name="available"/> <see cref="Type"/>
    /// </summary>
    /// <param name="prompt">message to show to user</param>
    /// <param name="available">array of Types selection should be made from</param>
    /// <param name="chosen">The users choice (unless the option is cancelled - see return value)</param>
    /// <returns>True if a choice was made or False if the choice was cancelled</returns>
    bool SelectType(string prompt, Type[] available, out Type chosen);

    /// <inheritdoc cref="SelectType(string, Type[], out Type)"/>
    bool SelectType(DialogArgs args, Type[] available, out Type chosen);

    #endregion

    /// <summary>
    /// Launches an appropriate user interface for <paramref name="o"/> (or does nothing if
    /// environment is not interactive e.g. console)
    /// </summary>
    /// <param name="o"></param>
    void Activate(object o);

    /// <summary>
    /// Handle the creation and configuring of a new <see cref="ICatalogue"/> often with user input about what column(s) should be extractable etc.  Return null if user cancelled the process somehow
    /// </summary>
    /// <param name="tableInfo">A reference to an existing table in a database upon which to point the Catalogue</param>
    /// <param name="initialDescription">Some initial text to </param>
    /// <param name="projectSpecific">Optional project to associate the <see cref="ICatalogue"/> created with </param>
    /// <param name="targetFolder">Optional virtual folder into which to put the <see cref="ICatalogue"/> or null.  This is not a file system folder only a visual display to the user in the RDMP client</param>
    /// <param name="extractionIdentifierColumns">Optional, which column(s) should be <see cref="ConcreteColumn.IsExtractionIdentifier"/></param>
    /// <returns>A fully configured ready to go Catalogue or null if user cancelled process e.g. if <see cref="IsInteractive"/></returns>
    ICatalogue CreateAndConfigureCatalogue(ITableInfo tableInfo, ColumnInfo[] extractionIdentifierColumns,
        string initialDescription, IProject projectSpecific, string targetFolder);

    /// <summary>
    /// Returns true if creating a cohort through an interactive wizard is supported.  If a wizard was shown and suitable input received then <paramref name="cic"/> should be populated with the result
    /// </summary>
    /// <param name="cic"></param>
    /// <returns>True if a wizard was shown even if no configuration was then created</returns>
    bool ShowCohortWizard(out CohortIdentificationConfiguration cic);


    /// <summary>
    /// Requests that the activator highlight or otherwise emphasise the supplied item.  Depending on who is subscribed to this event nothing may actually happen
    /// </summary>
    void RequestItemEmphasis(object sender, EmphasiseRequest emphasiseRequest);

    /// <summary>
    /// Returns the root object in the tree hierarchy or the inputted parameter (<paramref name="objectToEmphasise"/>)
    /// </summary>
    /// <param name="objectToEmphasise"></param>
    /// <returns></returns>
    object GetRootObjectOrSelf(object objectToEmphasise);


    /// <summary>
    /// Prompts user or directly creates a new satelite database (e.g. logging / dqe etc) and returns a persistent reference to it
    /// </summary>
    /// <param name="catalogueRepository">The main catalogue database</param>
    /// <param name="defaultToSet">If the created database is to become the new default database of its type provide this</param>
    /// <param name="db">The server in which the database should be created or null if the user is expected to pick themselves as part of the method e.g. through a UI</param>
    /// <param name="patcher">The schema and patches to run to create the database</param>
    /// <returns></returns>
    ExternalDatabaseServer CreateNewPlatformDatabase(ICatalogueRepository catalogueRepository,
        PermissableDefaults defaultToSet, IPatcher patcher, DiscoveredDatabase db);

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
    /// Delete the <paramref name="deleteable"/> ideally asking the user for confirmation first (if appropriate)
    /// </summary>
    /// <param name="deleteable"></param>
    /// <returns></returns>
    bool DeleteWithConfirmation(IDeleteable deleteable);

    /// <summary>
    /// Component for auditing errors that should be brought to the users attention subtly (e.g. if a plugin crashes while attempting to create menu items)
    /// </summary>
    ICheckNotifier GlobalErrorCheckNotifier { get; }

    /// <summary>
    /// Set to true to force the next <see cref="Publish(IMapsDirectlyToDatabaseTable)"/>
    /// to be a hard refresh that ignores the user's <see cref="UserSettings.SelectiveRefresh"/>
    /// settings
    /// </summary>
    bool HardRefresh { get; set; }

    /// <summary>
    /// True if this is the winforms client otherwise false
    /// </summary>
    bool IsWinForms { get; }

    /// <summary>
    /// True if <see cref="LaunchSubprocess"/> is supported.
    /// </summary>
    bool IsAbleToLaunchSubprocesses { get; }

    /// <summary>
    /// Called when <see cref="BasicCommandExecution.Publish"/> is invoked.  Allows you to respond to publish events outside of UI code.  UI code
    /// should invoke the RefreshBus system in Rdmp.UI
    /// </summary>
    /// <param name="databaseEntity"></param>
    void Publish(IMapsDirectlyToDatabaseTable databaseEntity);

    /// <summary>
    /// Launches a new process running <paramref name="startInfo"/>
    /// </summary>
    /// <param name="startInfo"></param>
    public void LaunchSubprocess(ProcessStartInfo startInfo);


    /// <summary>
    /// Returns true if the <see cref="UserSettings"/> indicate user has opted into
    /// the <see cref="Commit"/> system and the RDMP platform repository we are using
    /// supports commits/rollback (typically db only)
    /// </summary>
    /// <returns></returns>
    public bool UseCommits();
}