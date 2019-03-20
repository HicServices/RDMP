// Copyright (c) The University of Dundee 2018-2019
// This file is part of the Research Data Management Platform (RDMP).
// RDMP is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 3 of the License, or (at your option) any later version.
// RDMP is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.
// You should have received a copy of the GNU General Public License along with RDMP. If not, see <https://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Icons;
using CatalogueManager.ItemActivation;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using MapsDirectlyToDatabaseTable;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.TabPageContextMenus;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement
{
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

        private readonly IconFactory _iconFactory = new IconFactory();

        
        public WindowFactory(IRDMPPlatformRepositoryServiceLocator repositoryLocator, WindowManager windowManager)
        {
            _windowManager = windowManager;
            RepositoryLocator = repositoryLocator;
        }

        public PersistableToolboxDockContent Create(IActivateItems activator,Control control, string label, Bitmap image, RDMPCollection collection)
        {
            var content = new PersistableToolboxDockContent(collection);
            
            AddControlToDockContent(activator, control, content, label, image);

            return content;
        }
        
        public PersistableSingleDatabaseObjectDockContent Create(IActivateItems activator, RefreshBus refreshBus,IRDMPSingleDatabaseObjectControl control, Bitmap image, IMapsDirectlyToDatabaseTable databaseObject)
        {
            var content = new PersistableSingleDatabaseObjectDockContent(control, databaseObject,refreshBus);
            _windowManager.AddWindow(content);

            AddControlToDockContent(activator, (Control)control,content,"Loading...",image);
            
            return content;
        }

        public PersistableObjectCollectionDockContent Create(IActivateItems activator, IObjectCollectionControl control, IPersistableObjectCollection objectCollection, Bitmap image)
        {
            //create a new persistable docking tab
            var content = new PersistableObjectCollectionDockContent(activator,control,objectCollection);

            //add the control to the tab
            AddControlToDockContent(activator,(Control)control, content,content.TabText, image);
            
            //add to the window tracker
            _windowManager.AddWindow(content);

            //return the tab
            return content;
        }

        public PersistableSingleDatabaseObjectDockContent Create(IActivateItems activator, IRDMPSingleDatabaseObjectControl control, DatabaseEntity entity)
        {
            var content = new PersistableSingleDatabaseObjectDockContent(control, entity, activator.RefreshBus);

            var img = activator.CoreIconProvider.GetImage(entity);
            AddControlToDockContent(activator, (Control)control, content, entity.ToString(), img);

            return content;
        }


        public DockContent Create(IActivateItems activator, Control control, string label, Bitmap image)
        {
            DockContent content = new DockContent();
            
            AddControlToDockContent(activator, control, content,label, image);

            _windowManager.AddAdhocWindow(content);

            return content;
        }

        private void AddControlToDockContent(IActivateItems activator, Control control,DockContent content, string label, Bitmap image)
        {
            control.Dock = DockStyle.Fill;
            content.Controls.Add(control);
            content.TabText = label;

            if(image != null)
                content.Icon = _iconFactory.GetIcon(image);

            var consult = control as IConsultableBeforeClosing;

            if (consult != null)
                content.FormClosing += consult.ConsultAboutClosing;

            content.KeyPreview = true;
            
            var tab = content as RDMPSingleControlTab;

            if (tab != null)
                content.TabPageContextMenuStrip = new RDMPSingleControlTabMenu(activator, tab, _windowManager);
        }
    }
}
