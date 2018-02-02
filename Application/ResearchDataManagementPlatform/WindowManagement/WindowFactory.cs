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
using RDMPStartup;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.TabPageContextMenus;
using ResearchDataManagementPlatform.WindowManagement.UserSettings;
using ReusableUIComponents;
using ReusableUIComponents.SingleControlForms;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Translates Controls into docked tabs (DockContent).  Provides overloads for the two main control Types IRDMPSingleDatabaseObjectControl and 
    /// IObjectCollectionControl (for RDMPCollectionUI see ToolboxWindowManager).  Also tracks tab activation (focus) and ensures that when a tab is closed
    /// a suitable tab is activated (brought to focus) in it's place.
    /// </summary>
    public class WindowFactory
    {
        /// <summary>
        /// Location of the Catalogue / Data export repository databases (and allows access to repository objects)
        /// </summary>
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }

        private readonly IconFactory _iconFactory = new IconFactory();

        public ContentWindowTracker WindowTracker { get; private set; }

        List<DockContent> activationQueue = new List<DockContent>();
        public event TabChangedHandler TabChanged;

        public WindowFactory(IRDMPPlatformRepositoryServiceLocator repositoryLocator, DockPanel mainDockPanel)
        {
            RepositoryLocator = repositoryLocator;
            WindowTracker = new ContentWindowTracker();
            mainDockPanel.ActiveDocumentChanged += ActiveDocumentChanged;
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
            WindowTracker.AddWindow(content);

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
            WindowTracker.AddWindow(content);

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

            WindowTracker.AddAdhocWindow(content);

            return content;
        }

        private void AddControlToDockContent(IActivateItems activator, Control control,DockContent content, string label, Bitmap image)
        {
            var repoUser = control as IRepositoryUser;
            if(repoUser != null && repoUser.RepositoryLocator == null)
                repoUser.RepositoryLocator = RepositoryLocator;

            control.Dock = DockStyle.Fill;
            content.Controls.Add(control);
            content.TabText = label;

            if(image != null)
                content.Icon = _iconFactory.GetIcon(image);

            var consult = control as IConsultableBeforeClosing;

            if (consult != null)
                content.FormClosing += consult.ConsultAboutClosing;

            content.KeyPreview = true;
            content.KeyUp += ContentOnKeyUp;
            content.FormClosed += FormClosed; //when content is closed activate the last focused document
            
            var tab = content as RDMPSingleControlTab;

            if (tab != null)
                content.TabPageContextMenuStrip = new RDMPSingleControlTabMenu(activator, tab, WindowTracker);
        }
        
        private void ActiveDocumentChanged(object sender, EventArgs e)
        {
            var docContent = (DockContent)((DockPanel) sender).ActiveDocument;

            if (TabChanged != null)
                TabChanged(this, docContent);

            if(docContent == null)
                return;
            
            //whatever document got activated, bump it to the top of the list so that any close event makes it the next document to be activated
            activationQueue.Remove(docContent);
            activationQueue.Add(docContent);
        }
        private void FormClosed(object sender, FormClosedEventArgs e)
        {
            //remove the closed form
            activationQueue.Remove((DockContent) sender);
            
            //and any other mysteriously dead forms
            PruneList();

            //then get the last one the user focused
            var last = activationQueue.LastOrDefault();

            if(last != null)
                last.Activate();//and pop it up
        }

        private void PruneList()
        {
            //get rid of any DockContent that have died somehow or are not visible
            foreach (var dc in activationQueue.ToArray())
                if (dc.IsDisposed || !dc.IsHandleCreated || dc.IsHidden)
                    activationQueue.Remove(dc);
        }


        private void ContentOnKeyUp(object sender, KeyEventArgs keyEventArgs)
        {
            if(keyEventArgs.KeyCode == Keys.W && keyEventArgs.Control)
                ((DockContent)sender).Close();
        }

    }
}
