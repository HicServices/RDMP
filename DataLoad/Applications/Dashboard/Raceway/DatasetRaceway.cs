using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CatalogueLibrary.Data;
using CatalogueLibrary.Data.Dashboarding;
using CatalogueManager.DashboardTabs.Construction;
using CatalogueManager.Icons.IconOverlays;
using CatalogueManager.Icons.IconProvision;
using CatalogueManager.ItemActivation;
using CatalogueManager.LocationsMenu;
using CatalogueManager.Refreshing;
using CatalogueManager.TestsAndSetup.ServicePropogation;
using DataExportLibrary.Data.DataTables.DataSetPackages;
using DataExportLibrary.Providers;
using DataQualityEngine.Data;
using MapsDirectlyToDatabaseTable;
using MapsDirectlyToDatabaseTableUI;
using ReusableLibraryCode.Icons.IconProvision;

namespace Dashboard.Raceway
{
    /// <summary>
    /// Allows you to quickly view the timespan of each of your datasets, which sections of your datasets are failing validation (e.g. 'Prescribing' 2001-2002 records are all failing
    /// validation but 2003 onwards are fine) and identify any gaps in your record coverage.
    /// 
    /// <para>Each dataset appears as a green/red bar along a shared axis (See RacewayRenderAreaUI).  You can switch from viewing all months for which you have data, only
    /// the last decade, year or last 6 months.</para>
    /// 
    /// <para>By default the row height of each bar in a dataset indicates the proportion of records in that month relative to the average number of records per month, this allows you to see for
    /// example the gradual increase in volume of records per month in a dataset and identify any periods where it doubles (may indicate duplication) or a hole appears.  If you tick 'Ignore
    /// Row Counts' then full bars will appear only, this lets you identify which datasets are responsible for sparse errors (e.g. if 'Biochemistry' has 1,00,000,000 and some records have
    /// dates sprinkled between 1900-01-01 and 2000-01-01 then these will appear on the axis but won't be visible due to how sparse the number of error records are).</para>
    /// </summary>
    public partial class DatasetRaceway : UserControl,IDashboardableControl
    {


        public DatasetRaceway()
        {
            InitializeComponent();
            ddShowPeriod.ComboBox.DataSource = Enum.GetValues(typeof (RacewayShowPeriod));


            btnRemoveAll.Image = FamFamFamIcons.delete_multi;
            _ignoreRowCounts = CatalogueIcons.RowCounts_Ignore;
            _respectRowCounts = CatalogueIcons.RowCounts_Respect;
        }

        private DashboardControl _dashboardControlDatabaseRecord;
        private DatasetRacewayObjectCollection _collection;
        private IActivateItems _activator;
        private bool _isEditmodeOn;

        private bool isFirstTime = true;
        private Bitmap _ignoreRowCounts;
        private Bitmap _respectRowCounts;

        public void GenerateChart()
        {
            ragSmiley1.Reset();
            ragSmiley1.SetVisible(false);
            
            var allCatalogues = _collection.GetCatalogues();

            Dictionary<Catalogue, Dictionary<DateTime, ArchivalPeriodicityCount>> cataloguesToAdd = new Dictionary<Catalogue, Dictionary<DateTime, ArchivalPeriodicityCount>>();

            DQERepository dqeRepository;

            try
            {
                dqeRepository = new DQERepository(_activator.RepositoryLocator.CatalogueRepository);
            }
            catch (NotSupportedException e)
            {
                ragSmiley1.SetVisible(true);
                ragSmiley1.Fatal(e);
                return;
            }
            
            foreach (var cata in allCatalogues.OrderBy(c => c.Name))
            {
                var eval = dqeRepository.GetMostRecentEvaluationFor(cata);
                
                Dictionary<DateTime, ArchivalPeriodicityCount> dictionary = null;

                if (eval != null)
                    dictionary = PeriodicityState.GetPeriodicityCountsForEvaluation(eval);

                cataloguesToAdd.Add(cata, dictionary);
            }
            
            //every month seen in every dataset ever
            var buckets = GetBuckets(cataloguesToAdd);

            racewayRenderArea.AddTracks(_activator,cataloguesToAdd, buckets, _collection.IgnoreRows);
            racewayRenderArea.Refresh();

            this.Invalidate();
        }

        private DateTime[] GetBuckets(Dictionary<Catalogue, Dictionary<DateTime, ArchivalPeriodicityCount>> cataloguesDictionary)
        {
            var buckets = new List<DateTime>();

            foreach (var dictionary in cataloguesDictionary.Values)
                if (dictionary != null)
                    foreach (DateTime dt in dictionary.Keys)
                    {
                        //if it is before the start or after today
                        if (dt < GetUserPickedStartDate() || dt.Date > DateTime.Now.Date)
                            continue;

                        if (!buckets.Contains(dt))
                            buckets.Add(dt);
                    }

            return buckets.OrderBy(d => d).ToArray(); 
        }

        private DateTime GetUserPickedStartDate()
        {
            switch (_collection.ShowPeriod)
            {
                case RacewayShowPeriod.AllTime:
                    return DateTime.MinValue;
                case RacewayShowPeriod.LastDecade:
                    return DateTime.Now.AddYears(-10);
                case RacewayShowPeriod.LastYear:
                    return DateTime.Now.AddYears(-1);
                case RacewayShowPeriod.LastSixMonths:
                    return DateTime.Now.AddMonths(-6);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        public enum RacewayShowPeriod
        {
            AllTime,
            LastDecade,
            LastYear,
            LastSixMonths
        }
        
        public void RefreshBus_RefreshObject(object sender, RefreshObjectEventArgs e)
        {
            
        }

        public void SetCollection(IActivateItems activator, IPersistableObjectCollection collection)
        {
            _activator = activator;
            _collection = (DatasetRacewayObjectCollection) collection;


            btnAddCatalogue.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.Catalogue, OverlayKind.Import);
            btnAddExtractableDatasetPackage.Image = _activator.CoreIconProvider.GetImage(RDMPConcept.ExtractableDataSetPackage, OverlayKind.Import);
            

            ddShowPeriod.ComboBox.SelectedItem = _collection.ShowPeriod;
            cbIgnoreRowCounts.Checked = _collection.IgnoreRows;
            UpdateIgnoreRowCountCheckBoxIconAndText();

            if(isFirstTime)
            {
                isFirstTime = false;
                racewayRenderArea.RequestDeletion += (c) =>
                {
                    _collection.RemoveCatalogue(c);
                    SaveCollectionChanges();
                };
                racewayRenderArea.NotifyEditModeChange(_isEditmodeOn);
            }
                
            GenerateChart();
        }

        public IPersistableObjectCollection GetCollection()
        {
            return _collection;
        }

        public string GetTabName()
        {
            return Text;
        }

        public IPersistableObjectCollection ConstructEmptyCollection(DashboardControl databaseRecord)
        {
            _dashboardControlDatabaseRecord = databaseRecord;

            return new DatasetRacewayObjectCollection();
        }

        public void NotifyEditModeChange(bool isEditModeOn)
        {
            _isEditmodeOn = isEditModeOn;

            racewayRenderArea.NotifyEditModeChange(isEditModeOn);

            if (isEditModeOn)
                Controls.Add(toolStrip1);
            else
                Controls.Remove(toolStrip1);
        }
        
        private void btnAddCatalogue_Click(object sender, EventArgs e)
        {
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(_activator.RepositoryLocator.CatalogueRepository.GetAllCatalogues().Except(_collection.GetCatalogues()), false, false);
            dialog.AllowMultiSelect = true;

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var selectedCatalogues = dialog.MultiSelected;
                foreach (var catalogue in selectedCatalogues)
                    AddCatalogue((Catalogue)catalogue);

                SaveCollectionChanges();
                GenerateChart();
            }
        }

        private void SaveCollectionChanges()
        {
            _dashboardControlDatabaseRecord.SaveCollectionState(_collection);
        }

        private void btnRemoveAll_Click(object sender, EventArgs e)
        {
            _collection.ClearDatabaseObjects();

            SaveCollectionChanges();
            GenerateChart();
        }

        private void btnAddExtractableDatasetPackage_Click(object sender, EventArgs e)
        {
            var dataExportChildProvider = _activator.CoreChildProvider as DataExportChildProvider;

            if(dataExportChildProvider == null)
                return;
            
            var dialog = new SelectIMapsDirectlyToDatabaseTableDialog(dataExportChildProvider.AllPackages, false, false);
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                var packageToAdd = (ExtractableDataSetPackage) dialog.Selected;

                var contents = dataExportChildProvider.PackageContents.GetAllDataSets(packageToAdd,dataExportChildProvider.ExtractableDataSets);

                foreach (var cata in contents.Select(ds => ds.Catalogue))
                {
                    if(!_collection.GetCatalogues().Contains(cata))
                        AddCatalogue((Catalogue) cata);
                }
                
                SaveCollectionChanges();
                GenerateChart();
            }

        }

        private void AddCatalogue(Catalogue cata)
        {
            _collection.AddCatalogue(cata);
        }

        private void ddShowPeriod_DropDownClosed(object sender, EventArgs e)
        {
            if (ddShowPeriod.SelectedItem == null)
                return;

            var newPeriod = (RacewayShowPeriod)ddShowPeriod.SelectedItem;

            //no change
            if (newPeriod == _collection.ShowPeriod)
                return;

            _collection.ShowPeriod = newPeriod;
            GenerateChart();
            SaveCollectionChanges();
        }

        private void cbIgnoreRowCounts_CheckedChanged(object sender, EventArgs e)
        {
            
            UpdateIgnoreRowCountCheckBoxIconAndText();
            _collection.IgnoreRows = cbIgnoreRowCounts.Checked;
            GenerateChart();
            SaveCollectionChanges();
        }

        private void UpdateIgnoreRowCountCheckBoxIconAndText()
        {
            cbIgnoreRowCounts.Image = cbIgnoreRowCounts.Checked ? _ignoreRowCounts : _respectRowCounts;
            cbIgnoreRowCounts.Text = cbIgnoreRowCounts.Checked ? "Respect Row Counts":"Ignore Row Counts";
        }
    }
}
