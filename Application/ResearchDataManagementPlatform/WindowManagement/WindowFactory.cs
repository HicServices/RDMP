// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System.Windows.Forms;
using Rdmp.Core;
using Rdmp.Core.Curation.Data;
using Rdmp.Core.Curation.Data.Dashboarding;
using Rdmp.Core.MapsDirectlyToDatabaseTable;
using Rdmp.Core.Repositories;
using Rdmp.UI;
using Rdmp.UI.Collections;
using Rdmp.UI.ItemActivation;
using Rdmp.UI.Refreshing;
using Rdmp.UI.SimpleControls;
using Rdmp.UI.SingleControlForms;
using Rdmp.UI.TestsAndSetup.ServicePropogation;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.TabPageContextMenus;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement;

/// <summary>
/// Translates Controls into docked tabs (DockContent).  Provides overloads for the two main control Types IRDMPSingleDatabaseObjectControl and
/// IObjectCollectionControl (for <see cref="RDMPCollectionUI"/> see <see cref="WindowManager"/>).
/// </summary>
public class WindowFactory
{
    private readonly WindowManager _windowManager;

    /// <summary>
    /// Location of the Catalogue / Data export repository databases (and allows access to repository objects)
    /// </summary>
    public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }

    private readonly IconFactory _iconFactory = IconFactory.Instance;


    public WindowFactory(IRDMPPlatformRepositoryServiceLocator repositoryLocator, WindowManager windowManager)
    {
        _windowManager = windowManager;
        RepositoryLocator = repositoryLocator;
    }

    public PersistableToolboxDockContent Create(IActivateItems activator, Control control, string label,
        Image<Rgba32> image, RDMPCollection collection)
    {
        var content = new PersistableToolboxDockContent(collection);

        AddControlToDockContent(activator, control, content, label, image);

        return content;
    }

    public PersistableSingleDatabaseObjectDockContent Create(IActivateItems activator, RefreshBus refreshBus,
        IRDMPSingleDatabaseObjectControl control, Image<Rgba32> image, IMapsDirectlyToDatabaseTable databaseObject)
    {
        var content = new PersistableSingleDatabaseObjectDockContent(control, databaseObject, refreshBus);
        _windowManager.AddWindow(content);

        AddControlToDockContent(activator, (Control)control, content, "Loading...", image);

        if (!RDMPMainForm.Loading)
            activator.HistoryProvider.Add(databaseObject);

        return content;
    }

    public PersistableObjectCollectionDockContent Create(IActivateItems activator, IObjectCollectionControl control,
        IPersistableObjectCollection objectCollection, Image<Rgba32> image)
    {
        //create a new persistable docking tab
        var content = new PersistableObjectCollectionDockContent(activator, control, objectCollection);

        //add the control to the tab
        AddControlToDockContent(activator, (Control)control, content, content.TabText, image);

        //add to the window tracker
        _windowManager.AddWindow(content);

        //return the tab
        return content;
    }

    public PersistableSingleDatabaseObjectDockContent Create(IActivateItems activator,
        IRDMPSingleDatabaseObjectControl control, DatabaseEntity entity)
    {
        var content = new PersistableSingleDatabaseObjectDockContent(control, entity, activator.RefreshBus);

        var img = activator.CoreIconProvider.GetImage(entity);
        AddControlToDockContent(activator, (Control)control, content, entity.ToString(), img);

        if (!RDMPMainForm.Loading)
            activator.HistoryProvider.Add(entity);

        return content;
    }


    public DockContent Create(IActivateItems activator, Control control, string label, Image<Rgba32> image)
    {
        DockContent content = new RDMPSingleControlTab(activator.RefreshBus, control,null);

        AddControlToDockContent(activator, control, content, label, image);

        _windowManager.AddAdhocWindow(content);

        return content;
    }

    private void AddControlToDockContent(IActivateItems activator, Control control, DockContent content, string label,
        Image<Rgba32> image)
    {
        control.Dock = DockStyle.Fill;
        content.Controls.Add(control);
        content.TabText = label;

        if (image != null) content.Icon = _iconFactory.GetIcon(image);


        if (control is IConsultableBeforeClosing consult)
            content.FormClosing += consult.ConsultAboutClosing;

        if (control is ISaveableUI saveable)
            content.FormClosing += (s, e) => saveable.GetObjectSaverButton()?.CheckForUnsavedChangesAnOfferToSave();

        content.KeyPreview = true;

        if (content is RDMPSingleControlTab tab)
        {
            content.TabPageContextMenuStrip = new RDMPSingleControlTabMenu(activator, tab, _windowManager);

            //Create handler for AfterPublish
            void Handler(object s, RefreshObjectEventArgs e)
            {
                // After global changes, rebuild the context menu

                if (!content.IsDisposed)
                    content.TabPageContextMenuStrip = new RDMPSingleControlTabMenu(activator, tab, _windowManager);
                else activator.RefreshBus.AfterPublish -= Handler; //don't leak handlers
            }

            //register the event handler
            activator.RefreshBus.AfterPublish += Handler;
        }
    }
}