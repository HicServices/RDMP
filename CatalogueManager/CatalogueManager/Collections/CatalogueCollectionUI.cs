using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using BrightIdeasSoftware;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Aggregation;
using CatalogueLibrary.Data.PerformanceImprovement;
using CatalogueLibrary.Nodes;
using CatalogueManager.Collections.Providers;
using CatalogueManager.CommandExecution;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.Menus;
using CatalogueManager.Refreshing;
using MapsDirectlyToDatabaseTable;
using RDMPObjectVisualisation.Copying;
using ReusableLibraryCode.Checks;

namespace CatalogueManager.Collections
{
    /// <summary>
    /// Main window for Data Management, this Control lets you view all your datasets, curate descriptive metadata, configure extractable columns, generate reports etc
    /// 
    /// The tree shows you all the datasets you have configured for use with the RDMP.  Double clicking on a dataset (called a Catalogue) will show you the descriptive data you 
    /// have recorded. Right clicking a Catalogue will give you access to operations relevant to Catalogues (e.g. viewing dataset extraction logic if any).  Right clicking a
    /// CatalogueItem will give you access to operations relevant to CatalogueItems (e.g. adding an Issue).  And so on.
    /// 
    /// Each Catalogue has 1 or more CatalogueItems (visible through the CatalogueItems tab), these are the columns in the dataset that are maintained by RDMP. If you have very 
    /// wide data tables with hundreds of columns you might only configure a subset of those columns (the ones most useful  to researchers) for extraction.
    /// 
    /// You can also drag Catalogues between folders or into other Controls (e.g. dragging a Catalogue into a CohortIdentificationCollectionUI container to add the dataset to the identification
    /// criteria).
    /// 
    /// Pressing the Del key will prompt you to delete the selected item.
    /// 
    /// The Right click menu for empty space includes:
    /// 
    ///     Create New Catalogue (creates an empty header record for a dataset - NOT ADVISED)
    /// 
    ///     Create New Catalogue by importing a flat file (Recommended method if you don't have a table already loaded in your database)
    /// 
    /// Right clicking a dataset lets you:
    /// 
    ///     View Extraction SQL - based on the CatalogueItems / ExtractionInformation configured the QueryBuilder will show you how it would execute SELECT queries against the dataset
    /// 
    ///     View Checks - Checks the integrity of the catalogue (does the extraction SQL actually work, is at least 1 row returned etc)
    /// 
    ///     Configure Lookups - Launches AdvancedLookupConfiguration
    /// 
    ///     ViewSupportingSQL - Launches SupportingSQLTableViewer
    /// 
    ///     View Supporting Documents - Launches SupportingDocumentsViewer
    /// 
    ///     View Issues - Launches ViewAllIssues
    /// 
    ///     View Issues All Catalogues - Same as above but for all issues in all datasets, not just the one right clicked
    /// 
    ///     Choose Time Cover Column - Lets you select a single extractable CatalogueItem (column/transform) as the time indicator for the dataset e.g. DatePrescriptionCollected
    /// 
    ///     Choose Pivot Category Column - Lets you select a single extractable CatalogueItem (column/transform) as a category indicator for the dataset e.g. Healthboard (make sure it doesn't
    ///  have too many values!)
    /// 
    ///     Configure Aggregates - Launches AggregateManagement
    /// 
    ///     Configure Validation - Launches ValidationSetupForm
    /// 
    ///     Import Validation - Launches ImportValidationFromUnderlyingColumnInfos
    /// 
    ///     Configure Load Metadata - Launches SelectLoadMetadataOrCreateNewUI
    /// 
    ///     Disassociate Catalogue From Load Metadata - Unregisters the Catalogue from it's load logic (used if you no longer wish to load the underlying tables as part of that load)
    /// 
    ///     Configure Logging - Launches ChooseLoggingTaskDialog
    /// 
    ///     Clone Catalogue - Creates an exact copy of the (metadata only!) of a Catalogue... not sure why you would want to do this tbh
    /// 
    ///     Set Deprecated, Internal, Cold Storage status of the dataset
    /// 
    ///     Delete the Catalogue
    /// 
    ///     View Dependencies of the Catalogue - Launches DependencyGraph
    /// 
    /// Finally you can launch 'Checking' for every dataset, this will attempt to verify the extraction SQL you
    /// have configured for each dataset and to ensure that it runs and that at least 1 row of data is returned.  Checking all the datasets can take a while so runs asynchronously.
    /// </summary>
    public partial class CatalogueCollectionUI : RDMPCollectionUI
    {
        private IActivateItems _activator;

        private string _filter;
        private bool _showInternal = false;
        private bool _showDeprecated = false;
        private bool _showColdStorage = false;

        private Catalogue[] _allCatalogues;

        //constructor

        public CatalogueCollectionUI()
        {
            InitializeComponent();
            //prevent visual studio crashes
            if (VisualStudioDesignMode)
                return;

            olvCheckResult.ImageGetter += CheckImageGetter;
            olvFilters.AspectGetter += FilterAspectGetter;
        }

        //The color to highlight each Catalogue based on its extractability status
        private object ocheckResultsDictionaryLock = new object();
        Dictionary<ICheckable, CheckResult> checkResultsDictionary = new Dictionary<ICheckable, CheckResult>();
        private Thread checkingThread;

        public void CheckCatalogues()
        {
            if (checkingThread != null && checkingThread.IsAlive)
            {
                MessageBox.Show("Checking is already happening");
                return;
            }
            
            //reset the dictionary
            lock (ocheckResultsDictionaryLock)
            {
                checkResultsDictionary = new Dictionary<ICheckable, CheckResult>();
            }

            var visibleCatalogues = tlvCatalogues.FilteredObjects.OfType<Catalogue>().ToArray();

            //reset the progress bar
            progressBar1.Maximum = visibleCatalogues.Length;
            progressBar1.Value = 0;
            progressBar1.Visible = true;

            checkingThread = new Thread(() =>
            {
                //only check the items that are visible int he listview
                foreach (var catalogue in visibleCatalogues)//make copy to prevent synchronization issues
                {
                    var notifier = new ToMemoryCheckNotifier();
                    catalogue.Check(notifier);

                    lock (ocheckResultsDictionaryLock)
                        checkResultsDictionary.Add(catalogue, notifier.GetWorst());

                    //increase progress bar count by one
                    Invoke(new MethodInvoker(() => { progressBar1.Value++; }));
                }
                
                
                foreach (var configuration in RepositoryLocator.CatalogueRepository.GetAllObjects<AggregateConfiguration>().ToArray())
                {
                    var notifier = new ToMemoryCheckNotifier();
                    configuration.Check(notifier);
                        
                    lock (ocheckResultsDictionaryLock)
                        checkResultsDictionary.Add(configuration, notifier.GetWorst());
                }


                //now load images to UI
                Invoke(new MethodInvoker(() =>
                {
                    progressBar1.Visible = false;
                    tlvCatalogues.RebuildColumns();//should update the images?
                }));

            });

            checkingThread.Start();
        }

        private object CheckImageGetter(object rowobject)
        {
            var checkable = rowobject as ICheckable;
            if (checkable == null)
                return null;
            
            lock (ocheckResultsDictionaryLock)
            {
                if (checkResultsDictionary.ContainsKey(checkable))
                    return CommonFunctionality.CoreIconProvider.GetImage(checkResultsDictionary[checkable]);

            }
            //not been checked yet
            return null;
        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (checkingThread != null)
                checkingThread.Abort();

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void tlvCatalogues_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                var deleteable = tlvCatalogues.SelectedObject as IDeleteable;
                var columnInfoLink = tlvCatalogues.SelectedObject as LinkedColumnInfoNode;

                if (columnInfoLink != null)
                    DeleteColumnInfoLink(columnInfoLink);

                if(deleteable == null)
                    return;
                
                _activator.DeleteWithConfirmation(this, deleteable);
            }

            if (e.KeyCode == Keys.N && e.Control)
            {
                var c = new Catalogue(RepositoryLocator.CatalogueRepository, "New Catalogue " + Guid.NewGuid());
                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(c));
            }


            var cataItem = tlvCatalogues.SelectedObject as CatalogueItem;

            if (cataItem != null)
                new CatalogueItemMenu(_activator,cataItem,_classifications[cataItem.ID]).HandleKeyPress(e);

        }


        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                ApplyFilters();

            }
        }

        public bool ShowDeprecated
        {
            get { return _showDeprecated; }
            set
            {
                _showDeprecated = value;
                ApplyFilters();
            }
        }

        public bool ShowInternal
        {
            get { return _showInternal; }
            set
            {
                _showInternal = value;
                ApplyFilters();
            }
        }

        public bool ShowColdStorage
        {
            get { return _showColdStorage; }
            set
            {
                _showColdStorage = value;
                ApplyFilters();
            }
        }
        
        public bool ShowCatalogueItems
        {
            get { return _showCatalogueItems; }
            set
            {
                //if going from not showing to showing we must refresh classifications
                _showCatalogueItems = value; 
            
                ApplyFilters();
            }
        }

        private bool isFirstTime = true;

        public void RefreshUIFromDatabase(object oRefreshFrom)
        {
            Stopwatch sw = new Stopwatch();
            sw.Restart();

            _classifications = _activator.CoreChildProvider.CatalogueItemClassifications;
            
            //if there are new catalogues we don't already have in our tree
            if (_allCatalogues != null)
            {
                var newCatalogues = CommonFunctionality.CoreChildProvider.AllCatalogues.Except(_allCatalogues);
                if (newCatalogues.Any())
                {
                    oRefreshFrom = CatalogueFolder.Root;//refresh from the root instead
                    tlvCatalogues.RefreshObject(oRefreshFrom);
                }
            }

            _allCatalogues = CommonFunctionality.CoreChildProvider.AllCatalogues;

            Console.WriteLine("Stage 1:"+sw.ElapsedMilliseconds);
            
            if(isFirstTime || Equals(oRefreshFrom, CatalogueFolder.Root))
            {
                tlvCatalogues.RefreshObject(CatalogueFolder.Root);
                ExpandAllFolders(CatalogueFolder.Root);
                isFirstTime = false;
            }


        }

        private void ExpandAllFolders(CatalogueFolder model)
        {
            //expand it
            tlvCatalogues.Expand(model);
            
            foreach (var folder in tlvCatalogues.GetChildren(model).OfType<CatalogueFolder>())
                ExpandAllFolders(folder);
        }

        #region RightClick Menu and Double Clicking

        private void tlvCatalogues_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            var o = e.Model;

            
            if (o == null || o is CatalogueFolder)
                e.MenuStrip = new CatalogueMenu( _activator, null);
            
            if (o is Catalogue)
                e.MenuStrip = new CatalogueMenu( _activator, (Catalogue)o);

            if (o is CatalogueItem)
                e.MenuStrip = new CatalogueItemMenu( _activator, (CatalogueItem)o, _classifications[((CatalogueItem)o).ID]);
            
            if(o is ExtractionInformation)
                e.MenuStrip = new ExtractionInformationMenu(_activator,(ExtractionInformation)o);
            
            if (o is LinkedColumnInfoNode)
                e.MenuStrip = new ColumnInfoMenu( _activator, ((LinkedColumnInfoNode)o).ColumnInfo);

            if (o is DocumentationNode)
                e.MenuStrip = new DocumentationNodeMenu(_activator, (DocumentationNode) o);

            if (o is AggregatesNode)
                e.MenuStrip = new AggregatesNodeMenu(_activator, (AggregatesNode) o);

            if (o is CatalogueItemsNode)
                e.MenuStrip = new CatalogueItemsNodeMenu(_activator, (CatalogueItemsNode) o);
        }

        private void DeleteColumnInfoLink(LinkedColumnInfoNode col)
        {
            //delete the relationship to the CatalogueItem
            var colInfo = col.ColumnInfo;
            var cataItem = col.CatalogueItem;

            if (MessageBox.Show("Delete relationship between CatalogueItem " + cataItem + " and ColumnInfo " + colInfo + "?", "Break Relationship", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                cataItem.SetColumnInfo(null);

                var ei = cataItem.ExtractionInformation;

                if (ei != null && MessageBox.Show("Also DELETE ExtractionInformation and all Filters etc?", "Delete ExtractionInformation", MessageBoxButtons.YesNo) == DialogResult.Yes)
                    ei.DeleteInDatabase();

                _activator.RefreshBus.Publish(this, new RefreshObjectEventArgs(cataItem));
            }
        }


        private void tlvCatalogues_ItemActivate(object sender, EventArgs e)
        {
            var o = tlvCatalogues.SelectedObject;
            var issue = o as CatalogueItemIssue;
            var doc = o as SupportingDocument;
            var sql = o as SupportingSQLTable;
            
            if(issue != null)
                _activator.ActivateCatalogueItemIssue(this,issue);

            if (doc != null)
                _activator.ActivateSupportingDocument(this, doc);

            if (sql != null)
                _activator.ActivateSupportingSqlTable(this, sql);

        }

        #endregion
        
        public void ApplyFilters()
        {
            CommonFunctionality.SecondaryFilter = new CatalogueCollectionFilter(_activator.CoreChildProvider,ShowInternal, ShowDeprecated, ShowColdStorage); 
        }

        public enum HighlightCatalogueType
        {
            None,
            Extractable,
            ExtractionBroken,
            TOP1Worked
        }

        private object _lastSelected = null;
        private bool _showCatalogueItems;

        private void btnCheckCatalogues_Click(object sender, EventArgs e)
        {
            CheckCatalogues();
        }

        private Dictionary<int, CatalogueItemClassification> _classifications;

        private object FilterAspectGetter(object rowObject)
        {
            var cataItem = rowObject as CatalogueItem;
            if (cataItem != null)
                if (_classifications.ContainsKey(cataItem.ID))
                    return _classifications[cataItem.ID].ExtractionFilterCount;

            return null;
        }

        
        
        private void btnExpandOrCollapse_Click(object sender, EventArgs e)
        {
            if(!CommonFunctionality.ExpandOrCollapse(btnExpandOrCollapse))
                ExpandAllFolders(CatalogueFolder.Root);
        }

        public override void SetItemActivator(IActivateItems activator)
        {
            _activator = activator;
            //important to register the setup before the lifetime subscription so it gets priority on events
            CommonFunctionality.SetUp(
                tlvCatalogues,
                _activator,
                olvColumn1, //the icon column
                tbFilter,//we have our own custom filter logic so no need to pass tbFilter
                olvColumn1 //also the renameable column
                );

            _activator.RefreshBus.EstablishLifetimeSubscription(this);

            tlvCatalogues.AddObject(CatalogueFolder.Root);
            
            ApplyFilters();

            RefreshUIFromDatabase(CatalogueFolder.Root);
        }

        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            var o = e.Object;
            var cata = o as Catalogue;

            if (cata != null)
            {
                var oldFolder = tlvCatalogues.GetParent(cata) as CatalogueFolder;

                //if theres a change to the folder of the catalogue or it is a new Catalogue (no parent folder) we have to rebuild the entire tree
                if (oldFolder == null || !oldFolder.Path.Equals(cata.Folder.Path))
                    RefreshUIFromDatabase(CatalogueFolder.Root);
                else
                    RefreshUIFromDatabase(o);
                return;
            }

            if( o is CatalogueItem || o is AggregateConfiguration ||
            o is ColumnInfo || o is TableInfo || o is ExtractionFilter || o is ExtractionFilterParameter ||
            o is ExtractionFilterParameterSet || o is ExtractionInformation ||
            o is AggregateFilterContainer || o is AggregateFilter || o is AggregateFilterParameter|| o is CatalogueItemIssue)
            {
                //then refresh us
                RefreshUIFromDatabase(o);
            }
        }

        private bool _expandFlags = true;


        private void btnShowFlags_Click(object sender, EventArgs e)
        {
            splitContainer1.Panel2Collapsed = !_expandFlags;
            _expandFlags = !_expandFlags;

            btnShowFlags.Text = _expandFlags ? "+" : "-";
        }

        private void rbFlag_CheckedChanged(object sender, EventArgs e)
        {
            if (sender == rbColdStorage)
                ShowColdStorage = rbColdStorage.Checked;
            if (sender == rbWarmStorage)
                ShowColdStorage = !rbWarmStorage.Checked;
            if (sender == rbDeprecated)
                ShowDeprecated = rbDeprecated.Checked;
            if (sender == rbLive)
                ShowDeprecated = !rbLive.Checked;
            if (sender == rbInternal)
                ShowInternal = rbInternal.Checked;
            if (sender == rbNotInternal)
                ShowInternal = !rbNotInternal.Checked;
        }

        private void tbFilter_TextChanged(object sender, EventArgs e)
        {
            Filter = tbFilter.Text;
        }

        public static bool IsRootObject(object root)
        {
            return root.Equals(CatalogueFolder.Root);
        }

        public void SelectCatalogue(Catalogue catalogue)
        {
            tlvCatalogues.SelectObject(catalogue, true);
        }
    }
}
