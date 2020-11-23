// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FAnsi.Discovery;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.CohortCommitting.Pipeline;
using Rdmp.Core.CommandLine.Runners;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Curation.Data.Pipelines;
using Rdmp.Core.DataExport.Data;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using ReusableLibraryCode.Checks;

namespace Rdmp.Core.CommandExecution
{
    public interface IBasicActivateItems
    {
        /// <summary>
        /// True for activators that can illicit immediate responses from users.  False for activators designed to run unattended e.g. command line/scripting
        /// </summary>
        bool IsInteractive {get;}

        /// <summary>
        /// Event triggered when objects should be brought to the users attention
        /// </summary>
        event EmphasiseItemHandler Emphasise;

        /// <summary>
        /// Component for recording object tree inheritance (for RDMPCollectionUI primarily but also for anyone who wants to know children of objects or all objects quickly without having to go back to the database)
        /// </summary>
        ICoreChildProvider CoreChildProvider { get; }

        /// <summary>
        /// Component class for discovering the default DQE, Logging servers etc configured in the current RDMP database
        /// </summary>
        IServerDefaults ServerDefaults { get; }

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
        IPipelineRunner GetPipelineRunner(IPipelineUseCase useCase, IPipeline pipeline);

        /// <summary>
        /// Prompts the user to enter a description for a cohort they are trying to create including whether it is intended to replace an old version of another cohort.
        /// </summary>
        /// <param name="externalCohortTable">Where the user will be creating the cohort</param>
        /// <param name="project">The project the cohort should be associated with</param>
        /// <param name="cohortInitialDescription">Optional initial description for the cohort which may be changed by the user</param>
        /// <returns></returns>
        CohortCreationRequest GetCohortCreationRequest(ExternalCohortTable externalCohortTable, IProject project, string cohortInitialDescription);

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

        /// <summary>
        /// Prompts user to pick one of the <paramref name="availableObjects"/>
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="availableObjects">Objects that can be selected</param>
        /// <param name="initialSearchText"></param>
        /// <param name="allowAutoSelect"></param>
        /// <returns></returns>
        IMapsDirectlyToDatabaseTable SelectOne(string prompt, IMapsDirectlyToDatabaseTable[] availableObjects, string initialSearchText = null, bool allowAutoSelect = false);

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
        FileInfo[] SelectFiles(string prompt,string patternDescription, string pattern);

        /// <summary>
        /// Prompts user to select a file on disk (that may or may not exist yet) with the given pattern
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="patternDescription">Type of file to select e.g. "Comma Separated Values"</param>
        /// <param name="pattern">Pattern to restrict files to e.g. *.csv</param>
        /// <returns></returns>
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
        /// <param name="initialValue"></param>
        /// <param name="chosen"></param>
        /// <returns></returns>
        bool SelectValueType(string prompt, Type paramType, object initialValue,out object chosen);

        /// <summary>
        /// Delete the <paramref name="deleteable"/> ideally asking the user for confirmation first (if appropriate)
        /// </summary>
        /// <param name="deleteable"></param>
        /// <returns></returns>
        bool DeleteWithConfirmation(IDeleteable deleteable);

        /// <summary>
        /// Offers the user a binary choice and returns true if they consciously select a value.  This method is blocking.
        /// </summary>
        /// <param name="text">The question to pose</param>
        /// <param name="caption"></param>
        /// <param name="chosen">The answer chosen by the user</param>
        /// <returns>true if user successfully made a choice</returns>
        bool YesNo(string text, string caption, out bool chosen);

        /// <summary>
        /// Offers the user a binary choice returning the choice or false if user cancels (to distinguish between false and cancel
        /// use the overload).  This method is blocking.</summary>
        /// <param name="text">The question to pose</param>
        /// <param name="caption"></param>
        /// <returns>Users choice or false if user cancels</returns>
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
        /// Requests that the activator highlight or otherwise emphasise the supplied item.  Depending on who is subscribed to this event nothing may actually happen
        /// </summary>
        void RequestItemEmphasis(object sender, EmphasiseRequest emphasiseRequest);

        /// <summary>
        /// Returns the root object in the tree hierarchy or the inputted parameter (<paramref name="objectToEmphasise"/>)
        /// </summary>
        /// <param name="objectToEmphasise"></param>
        /// <returns></returns>
        object GetRootObjectOrSelf(IMapsDirectlyToDatabaseTable objectToEmphasise);

        /// <summary>
        /// Requests a selection of one of the values of the <see cref="Enum"/> <paramref name="enumType"/>
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="enumType"></param>
        /// <param name="chosen"></param>
        /// <returns></returns>
        bool SelectEnum(string prompt, Type enumType, out Enum chosen);

        /// <summary>
        /// Requests user select a <see cref="Type"/>
        /// </summary>
        /// <param name="prompt"></param>
        /// <param name="baseTypeIfAny">Pass a base class or interface if the Type must be an inheritor / assignable to a specific Type otherwise pass null</param>
        /// <param name="chosen"></param>
        /// <returns></returns>
        bool SelectType(string prompt, Type baseTypeIfAny, out Type chosen);

        /// <summary>
        /// Requests user select one of the <paramref name="available"/> <see cref="Type"/>
        /// </summary>
        /// <param name="prompt">message to show to user</param>
        /// <param name="available">array of Types selection should be made from</param>
        /// <param name="chosen">The users choice (unless the option is cancelled - see return value)</param>
        /// <returns>True if a choice was made or False if the choice was cancelled</returns>
        bool SelectType(string prompt, Type[] available, out Type chosen);

        /// <summary>
        /// Launches an appropriate user interface for <paramref name="o"/> (or does nothing if
        /// environment is not interactive e.g. console)
        /// </summary>
        /// <param name="o"></param>
        void Activate(DatabaseEntity o);

    }
}