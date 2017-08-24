using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using CatalogueLibrary.Repositories;
using CatalogueManager.Collections;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.Refreshing;
using CohortManager.Collections;
using DataExportManager.Collections;
using RDMPStartup;
using ResearchDataManagementPlatform.WindowManagement.ContentWindowTracking.Persistence;
using ResearchDataManagementPlatform.WindowManagement.Events;
using ResearchDataManagementPlatform.WindowManagement.ExtenderFunctionality;
using ResearchDataManagementPlatform.WindowManagement.HomePane;
using ReusableUIComponents;
using WeifenLuo.WinFormsUI.Docking;

namespace ResearchDataManagementPlatform.WindowManagement
{
    /// <summary>
    /// Handles creating and tracking the main collection UIs available for user interaction
    /// </summary>
    public class ToolboxWindowManager
    {
        readonly Dictionary<RDMPCollection, PersistableToolboxDockContent> _visibleToolboxes = new Dictionary<RDMPCollection, PersistableToolboxDockContent>();
        
        private readonly DockPanel _mainDockPanel;

        public IRDMPPlatformRepositoryServiceLocator RepositoryLocator { get; set; }
        
        public ContentWindowManager ContentManager;
        private WindowFactory _windowFactory;
        
        public event RDMPCollectionCreatedEventHandler CollectionCreated;

        public ToolboxWindowManager(RefreshBus refreshBus,DockPanel mainDockPanel, IRDMPPlatformRepositoryServiceLocator repositoryLocator)
        {
            _windowFactory = new WindowFactory(repositoryLocator,mainDockPanel);
            ContentManager = new ContentWindowManager(refreshBus, mainDockPanel, repositoryLocator, _windowFactory,this);

            _mainDockPanel = mainDockPanel;
            _mainDockPanel.Theme = new VS2015LightTheme();
            _mainDockPanel.Theme.Extender.FloatWindowFactory = new CustomFloatWindowFactory();

            RepositoryLocator = repositoryLocator;
        }


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
                    toReturn = Show(RDMPCollection.DataExport,collection, "Data Export", CatalogueIcons.ExtractionConfiguration);
                break;

                case RDMPCollection.Cohort:
                    collection = new CohortIdentificationCollectionUI();
                    toReturn = Show(RDMPCollection.Cohort, collection, "Cohort Builder", CatalogueIcons.CohortIdentificationConfiguration);
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

        
        public void Destroy(RDMPCollection collection)
        {
            _visibleToolboxes[collection].Close();
        }

        public bool IsVisible(RDMPCollection collection)
        {
            return _visibleToolboxes.ContainsKey(collection);
        }

        public void ShowCollectionWhichSupportsRootObjectType(object root)
        {
            RDMPCollection collection = GetCollectionForRootObject(root);

            if(collection == RDMPCollection.None)
                return;

            if(IsVisible(collection))
                return;

            Create(collection);
        }

        private RDMPCollection GetCollectionForRootObject(object root)
        {
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

            return RDMPCollection.None;
        }
        
        HomeUI home;
        DockContent homeContent;

        public void PopHome()
        {
            if(home == null)
            {
                home = new HomeUI(this);
                
                homeContent = _windowFactory.Create(ContentManager, home, "Home", FamFamFamIcons.application_home);
                homeContent.Closed += (s, e) => home = null;
                homeContent.Show(_mainDockPanel, DockState.Document);
            }
            else
            {
                homeContent.Activate();
            }
        }

        public void CloseAllToolboxes()
        {
            foreach (RDMPCollection collection in Enum.GetValues(typeof (RDMPCollection)))
                if (IsVisible(collection))
                    Destroy(collection);
        }

        public void CloseAllWindows()
        {
            ContentManager.WindowFactory.WindowTracker.CloseAllWindows();
        }
    }
}
