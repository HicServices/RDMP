// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using MapsDirectlyToDatabaseTable;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Cohort;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.Curation.Data.DataLoad;
using Rdmp.Core.Curation.Data.Defaults;
using Rdmp.Core.Providers;
using Rdmp.Core.Repositories;
using Rdmp.UI.Collections;
using Rdmp.UI.Collections.Providers;
using Rdmp.UI.DataViewing.Collections;
using Rdmp.UI.ExtractionUIs.FilterUIs;
using Rdmp.UI.Icons.IconProvision;
using Rdmp.UI.ItemActivation.Arranging;
using Rdmp.UI.ItemActivation.Emphasis;
using Rdmp.UI.PluginChildProvision;
using Rdmp.UI.Refreshing;
using Rdmp.UI.Rules;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ReusableLibraryCode.Checks;
using ReusableLibraryCode.Comments;
using ReusableUIComponents.CommandExecution;
using ReusableUIComponents.Theme;

namespace Rdmp.UI.ItemActivation
{
    /// <summary>
    /// Central component class for handling all low level RDMP main user interface systems. This includes things like Tree object child provision, Icon provision,
    /// the publish system for notifying out of date objects etc.  Each function is segregated by a component class property e.g. RefreshBus, FavouritesProvider etc
    /// 
    /// <para>Also exposes the location of the Catalogue / Data Export repository databases via RepositoryLocator</para>
    /// </summary>
    public interface IActivateItems
    {
        ITheme Theme { get; }

        IServerDefaults ServerDefaults { get; }

        /// <summary>
        /// Component for publishing the fact that an object has recently been put out of date by you.
        /// </summary>
        RefreshBus RefreshBus { get; }
        
        /// <summary>
        /// Component for telling you whether a given DatabaseEntity is one of the current users favourite objects and for toggling it
        /// </summary>
        FavouritesProvider FavouritesProvider { get;}
        
        /// <summary>
        /// Component for recording object tree inheritance (for RDMPCollectionUI primarily but also for anyone who wants to know children of objects or all objects quickly without having to go back to the database)
        /// </summary>
        ICoreChildProvider CoreChildProvider { get; }

        /// <summary>
        /// List of the currently loaded IPluginUserInterface classes (these allow injection of additional tree items, tailoring context menus etc).  This list will
        /// include some intrinsic RDMP IPluginUserInterfaces as part of its own internal design but most of these will be third party plugins.
        /// </summary>
        List<IPluginUserInterface> PluginUserInterfaces { get; }

        /// <summary>
        /// Component for closing and opening multiple windows at once for optimal user experience for achieving a given task (e.g. running a data load)
        /// </summary>
        IArrangeWindows WindowArranger { get;}

        Form ShowWindow(Control singleControlForm, bool asDocument = false);

        Form ShowRDMPSingleDatabaseObjectControl(IRDMPSingleDatabaseObjectControl control, DatabaseEntity objectOfTypeT);

        /// <summary>
        /// Stores the location of the Catalogue / Data Export repository databases and provides access to their objects
        /// </summary>
        IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; }

        /// <summary>
        /// Component for providing access to RDMPConcept icon images, these are 19x19 pixel icons representing specific objects/concepts in RDMP
        /// </summary>
        ICoreIconProvider CoreIconProvider { get; }

        /// <summary>
        /// Component for auditing errors that should be brought to the users attention subtly (e.g. if a plugin crashes while attempting to create menu items)
        /// </summary>
        ICheckNotifier GlobalErrorCheckNotifier { get; }
        
        /// <summary>
        /// Component for starting drag or copy operations
        /// </summary>
        ICommandFactory CommandFactory { get;}

        /// <summary>
        /// Component for suggesting completion options for an ongoing drag or paste
        /// </summary>
        ICommandExecutionFactory CommandExecutionFactory { get;}

        /// <summary>
        /// Component for fetching xmldoc comments from the codebase 
        /// </summary>
        CommentStore CommentStore { get; }
        
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

        bool DeleteWithConfirmation(object sender, IDeleteable deleteable);

        event EmphasiseItemHandler Emphasise;

        /// <summary>
        /// Requests that the activator highlight or otherwise emphasise the supplied item.  Depending on who is subscribed to this event nothing may actually happen
        /// </summary>
        void RequestItemEmphasis(object sender, EmphasiseRequest request);

        void ActivateLookupConfiguration(object sender, Catalogue catalogue,TableInfo optionalLookupTableInfo=null);

        void ViewFilterGraph(object sender,FilterGraphObjectCollection collection);

        void ActivateViewCohortIdentificationConfigurationSql(object sender, CohortIdentificationConfiguration cic);
        void ActivateViewLog(LoadMetadata loadMetadata);

        IRDMPSingleDatabaseObjectControl ActivateViewLoadMetadataDiagram(object sender, LoadMetadata loadMetadata);

        bool IsRootObjectOfCollection(RDMPCollection collection, object rootObject);
        bool HasProblem(object model);
        string DescribeProblemIfAny(object model);
        object GetRootObjectOrSelf(IMapsDirectlyToDatabaseTable objectToEmphasise);

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

        DialogResult ShowDialog(Form form);
        
        /// <summary>
        /// Closes the Form <paramref name="f"/> and reports the <paramref name="reason"/> to the user
        /// in a highly visible way
        /// </summary>
        /// <param name="c"></param>
        /// <param name="reason"></param>
        void KillForm(Form f, Exception reason);

        /// <summary>
        /// Called when an ErrorProvider validation rule is registered
        /// </summary>
        /// <param name="rule"></param>
        void OnRuleRegistered(IBinderRule rule);
    }
}
