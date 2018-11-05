using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using CatalogueLibrary.Data;
using CatalogueLibrary.Providers;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Refreshing;
using CatalogueManager.Theme;
using CohortManager.Collections;
using DataExportManager.Collections;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.Events;
using ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality;
using ResearchDataManagementPlatform.WindowManagement.HomePane;
using ReusableLibraryCode.Checks;
using ReusableUIComponents;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Handles creating and tracking the main RDMPCollectionUIs tree views
    /// </summary>
    public class ToolboxWindowManager
    {
        readonly Dictionary<RDMPCollection, PersistableToolboxDockContent> _visibleToolboxes = new Dictionary<RDMPCollection, PersistableToolboxDockContent>();
        
        private readonly DockPanel _mainDockPanel;

        public RDMPMainForm MainForm { get; set; }

        /// <summary>
        /// The location finder for the Catalogue and optionally Data Export databases
        /// </summary>
        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }
        
        public ContentWindowManager ContentManager;
        private readonly WindowFactory _windowFactory;
        
        public event RDMPCollectionCreatedEventHandler CollectionCreated;

        HomeUI _home;
        DockContent _homeContent;

        public ToolboxWindowManager(RDMPMainForm mainForm, RefreshBus refreshBus, DockPanel mainDockPanel, IRDMPPlatformRepositoryServiceLocator repositoryLocator, ICheckNotifier globalErrorCheckNotifier)
        {
            _windowFactory = new WindowFactory(repositoryLocator,mainDockPanel);
            ContentManager = new ContentWindowManager(refreshBus, mainDockPanel, repositoryLocator, _windowFactory, this, globalErrorCheckNotifier);

            _mainDockPanel = mainDockPanel;
            _mainDockPanel.Theme = new VS2005Theme();
            _mainDockPanel.Theme.Extender.FloatWindowFactory = new CustomFloatWindowFactory();
            
            _mainDockPanel.ShowDocumentIcon = true;

            MainForm = mainForm;
            RepositoryLocator = repositoryLocator;
        }
        
        /// <summary>
        /// Creates a new instance of the given RDMPCollectionUI specified by the Enum collectionToCreate at the specified dock position
        /// </summary>
        /// <param name="collectionToCreate"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        public PersistableToolboxDockContent Create(RDMPCollection collectionToCreate, DockState position = DockState.DockLeft)
        {
            PersistableToolboxDockContent toReturn;
            RDMPCollectionUI collection;

            switch (collectionToCreate)
            {
                case RDMPCollection.Catalogue:
                        collection = new CatalogueCollectionUI();
                        toReturn = Show(RDMPCollection.Catalogue, collection, "Catalogues", CatalogueIcons.Catalogue);
                break;

                case RDMPCollection.DataLoad:
                    collection = new LoadMetadataCollectionUI();
                    toReturn = Show(RDMPCollection.DataLoad, collection, "Load Configurations", CatalogueIcons.LoadMetadata);
                break;

                case RDMPCollection.Tables:
                    collection = new TableInfoCollectionUI();
                    toReturn = Show(RDMPCollection.Tables, collection, "Tables",CatalogueIcons.TableInfo);
                break;

                case RDMPCollection.DataExport:
                    if (RepositoryLocator.DataExportRepository == null)
                    {
                        WideMessageBox.Show("Cannot create DataExport Toolbox because DataExportRepository has not been set/created yet");
                        return null;
                    }

                    collection = new DataExportCollectionUI();
                    toReturn = Show(RDMPCollection.DataExport,collection, "Data Export", CatalogueIcons.Project);
                break;

                case RDMPCollection.Cohort:
                    collection = new CohortIdentificationCollectionUI();
                    toReturn = Show(RDMPCollection.Cohort, collection, "Cohort Builder", CatalogueIcons.CohortIdentificationConfiguration);
                break;
                case RDMPCollection.SavedCohorts:
                    collection = new SavedCohortsCollectionUI();
                    toReturn = Show(RDMPCollection.SavedCohorts, collection, "Saved Cohorts", CatalogueIcons.AllCohortsNode);
                break;
                case RDMPCollection.Favourites:
                    collection = new FavouritesCollectionUI();
                    toReturn = Show(RDMPCollection.Favourites, collection, "Favourites", CatalogueIcons.Favourite);
                break;

                default: throw new ArgumentOutOfRangeException("collectionToCreate");
            }

            toReturn.DockState = position;

            collection.SetItemActivator(ContentManager);

            if(CollectionCreated != null)
                CollectionCreated(this, new RDMPCollectionCreatedEventHandlerArgs(collectionToCreate));

            return toReturn;
        }

        

        private PersistableToolboxDockContent Show(RDMPCollection collection,RDMPCollectionUI control, string label, Bitmap image)
        {
            BackColorProvider c = new BackColorProvider();
            image = c.DrawBottomBar(image, collection);
            
            var content = _windowFactory.Create(ContentManager,control, label, image, collection);//these are collections so are not tracked with a window tracker.
            content.Closed += (s, e) => content_Closed(collection);

            _visibleToolboxes.Add(collection, content);
            content.Show(_mainDockPanel, DockState.DockLeft);
            
            return content;
        }

        private void content_Closed(RDMPCollection collection)
        {
            //no longer visible
            _visibleToolboxes.Remove(collection);
        }

        /// <summary>
        /// Closes the specified RDMPCollectionUI (must be open - use IsVisible to check this)
        /// </summary>
        /// <param name="collection"></param>
        public void Destroy(RDMPCollection collection)
        {
            _visibleToolboxes[collection].Close();
        }

        /// <summary>
        /// Brings the specified collection to the front
        /// </summary>
        /// <param name="collection"></param>
        public void Pop(RDMPCollection collection)
        {
            if(!IsVisible(collection))
                throw new Exception("Can only pop when toolbox is already visible");

            if (_visibleToolboxes.ContainsKey(collection))
            {
                _visibleToolboxes[collection].Activate();
            }
        }

        /// <summary>
        /// Returns true if the corresponding RDMPCollectionUI is open (even if it is burried under other windows).
        /// </summary>
        /// <param name="collection"></param>
        /// <returns></returns>
        public bool IsVisible(RDMPCollection collection)
        {
            return _visibleToolboxes.ContainsKey(collection);
        }

        public RDMPCollection GetFocusedCollection()
        {
            foreach (KeyValuePair<RDMPCollection, PersistableToolboxDockContent> t in _visibleToolboxes)
            {
                if (t.Value.ContainsFocus)
                    return t.Key;
            }

            return RDMPCollection.None;
        }

        /// <summary>
        /// Attempts to ensure that a compatible RDMPCollectionUI is made visible for the supplied object which must be one of the expected root Tree types of 
        /// an RDMPCollectionUI.  For example Project is the a root object of DataExportCollectionUI.  If a matching collection is already visible or no collection
        /// supports the supplied object as a root object then nothing will happen.  Otherwise the coresponding collection will be shown
        /// </summary>
        /// <param name="root"></param>
        public void ShowCollectionWhichSupportsRootObjectType(object root)
        {
            RDMPCollection collection = GetCollectionForRootObject(root);

            if(collection == RDMPCollection.None)
                return;

            if(IsVisible(collection))
                return;

            Create(collection);
        }

        public RDMPCollection GetCollectionForRootObject(object root)
        {
            if (FavouritesCollectionUI.IsRootObject(ContentManager,root))
                return RDMPCollection.Favourites;

            if(CatalogueCollectionUI.IsRootObject(root))
                return RDMPCollection.Catalogue;

            if(CohortIdentificationCollectionUI.IsRootObject(root))
                return RDMPCollection.Cohort;

            if(DataExportCollectionUI.IsRootObject(root))
                return RDMPCollection.DataExport;

            if(LoadMetadataCollectionUI.IsRootObject(root))
                return RDMPCollection.DataLoad;

            if(TableInfoCollectionUI.IsRootObject(root))
                return RDMPCollection.Tables;

            if(SavedCohortsCollectionUI.IsRootObject(root))
                return RDMPCollection.SavedCohorts;

            return RDMPCollection.None;
        }
        
        /// <summary>
        /// Displays the HomeUI tab or brings it to the front if it is already open
        /// </summary>
        public void PopHome()
        {
            if(_home == null)
            {
                _home = new HomeUI(this);
                
                _homeContent = _windowFactory.Create(ContentManager, _home, "Home", FamFamFamIcons.application_home);
                _homeContent.Closed += (s, e) => _home = null;
                _homeContent.Show(_mainDockPanel, DockState.Document);
            }
            else
            {
                _homeContent.Activate();
            }
        }

        /// <summary>
        /// Closes all currently open RDMPCollectionUI tabs
        /// </summary>
        public void CloseAllToolboxes()
        {
            foreach (RDMPCollection collection in Enum.GetValues(typeof (RDMPCollection)))
                if (IsVisible(collection))
                    Destroy(collection);
        }

        /// <summary>
        /// Closes all content window tabs (i.e. anything that isn't an RDMPCollectionUI tab - see CloseAllToolboxes)
        /// </summary>
        public void CloseAllWindows()
        {
            ContentManager.WindowFactory.WindowTracker.CloseAllWindows(null);
        }
    }
}
