// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.CommandExecution;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.UI.Collections;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.CommandExecution;
using Rdmp.UI.ItemActivation.Arranging;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using Rdmp.UI.Theme;

namespace Rdmp.UI.ItemActivation;

/// <summary>
/// Central component class for handling all low level RDMP main user interface systems. This includes things like Tree object child provision, Icon provision,
/// the publish system for notifying out of date objects etc.  Each function is segregated by a component class property e.g. RefreshBus, FavouritesProvider etc
/// 
/// <para>Also exposes the location of the Catalogue / Data Export repository databases via RepositoryLocator</para>
/// </summary>
public interface IActivateItems:IBasicActivateItems
{
    ITheme Theme { get; }

    /// <summary>
    /// Component for publishing the fact that an object has recently been put out of date by you.
    /// </summary>
    RefreshBus RefreshBus { get; }

    /// <summary>
    /// Component for closing and opening multiple windows at once for optimal user experience for achieving a given task (e.g. running a data load)
    /// </summary>
    IArrangeWindows WindowArranger { get;}

    Form ShowWindow(Control singleControlForm, bool asDocument = false);
        
    /// <summary>
    /// Component for starting drag or copy operations
    /// </summary>
    ICombineableFactory CommandFactory { get;}

    /// <summary>
    /// Component for suggesting completion options for an ongoing drag or paste
    /// </summary>
    ICommandExecutionFactory CommandExecutionFactory { get;}
        
    /// <summary>
    /// Records when objects are accessed by the user through the UI to allow navigation to recent objects
    /// </summary>
    HistoryProvider HistoryProvider { get; }

    /// <summary>
    /// Launches a new instance of the specified RDMPSingleDatabaseObjectControl Type with the supplied DatabaseEntity.  If you already have
    /// an ICommandExecutionProposal which facilitates Activation then you can instead use CommandExecutionFactory.Activate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="T2"></typeparam>
    /// <param name="databaseObject"></param>
    /// <returns></returns>
    T Activate<T, T2>(T2 databaseObject) where T : RDMPSingleDatabaseObjectControl<T2>, new() where T2 : DatabaseEntity;

    /// <summary>
    /// Launches a new instance of the specified IObjectCollectionControl with the supplied collection.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="collection"></param>
    /// <returns></returns>
    T Activate<T>(IPersistableObjectCollection collection) where T : Control,IObjectCollectionControl, new();

        
    bool IsRootObjectOfCollection(RDMPCollection collection, object rootObject);
    bool HasProblem(object model);
    string DescribeProblemIfAny(object model);
        

    /// <summary>
    /// Returns xml doc comments from the CommentStore for the given class (or null if it is undocumented)
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    string GetDocumentation(Type type);

    /// <summary>
    /// Returns the current directory (e.g. <see cref="Environment.CurrentDirectory"/>).
    /// </summary>
    string CurrentDirectory { get; }

    /// <summary>
    /// Shows the <paramref name="form"/>.  This method exists so tests can suppress the behaviour and to facilitate standardisation
    /// </summary>
    /// <param name="form"></param>
    /// <returns></returns>
    DialogResult ShowDialog(Form form);
        
    /// <summary>
    /// Closes the Form <paramref name="f"/> and reports the <paramref name="reason"/> to the user
    /// in a highly visible way
    /// </summary>
    /// <param name="f"></param>
    /// <param name="reason"></param>
    void KillForm(Form f, Exception reason);

    /// <summary>
    /// Closes the Form <paramref name="f"/> and reports the <paramref name="reason"/> to the user
    /// in a highly visible way
    /// </summary>
    /// <param name="f"></param>
    /// <param name="reason"></param>
    void KillForm(Form f, string reason);

    /// <summary>
    /// Called when an ErrorProvider validation rule is registered
    /// </summary>
    /// <param name="rule"></param>
    void OnRuleRegistered(IBinderRule rule);

    /// <summary>
    /// Determines system behaviour when an object is found to be out of sync with the remote database.  Typically implementing
    /// class will ask the user if they want to load up the database copy.
    /// </summary>
    /// <param name="databaseEntity"></param>
    /// <returns></returns>
    bool ShouldReloadFreshCopy(DatabaseEntity databaseEntity);

    /// <summary>
    /// Start a new scoped session with a collection of objects
    /// </summary>
    /// <param name="sessionName"></param>
    /// <param name="initialSelectionIfAny">Initial root objects to be in scope (or null if not known)</param>
    /// <param name="initialSearch">The value to set the search textbox to on load if objects are being selected during this operation, or null.</param>
    void StartSession(string sessionName, IEnumerable<IMapsDirectlyToDatabaseTable> initialSelectionIfAny, string initialSearch);
        
    /// <summary>
    /// Returns all currently open session uis
    /// </summary>
    /// <returns></returns>
    IEnumerable<SessionCollectionUI> GetSessions();
}